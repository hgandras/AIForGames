using OpenTK;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Timers;


/*Calculate goal iproperly
 Flip preceding order in Node CompareTo*/
namespace Prong
{
    internal class PlayerAIAStar : Player
    {
        private const int STATE_ROUND_INTERVAL = 5;
        private const float TimeOut = 0.05f;
        private  float timeDelta = 0.016f;
        private Node root;
        private float predictedBallY;
        private StaticState config;
        private Player otherPlayer=new PlayerAIReactive();
        PongEngine forwardModel;

        /// <summary>
        /// Node class to store states and associated data for the A* algorithm.
        /// </summary>
        public class Node:IComparable<Node>
        {
            public DynamicState state { get; set; }
            public Node parent { get; set; } 
            public PlayerAction action { get; set; } //The action leading to the state
            public float Cost { get; set; } //The path cost so far until this node
            public float Heuristic { get; set; } //The predicted cost until the goal
            public float F { 
                get
                {
                    return Cost + Heuristic;
                } 
            }

            public Node Clone()
            {
                Node clonedNode = new Node()
                {
                    state = state.Clone(),
                    parent = this.parent,
                    action = this.action,
                    Cost = this.Cost,
                    Heuristic = this.Heuristic,
                };
                return clonedNode;
            }

            private int applyInterval(float num)
            {
                return (int)Math.Round(num / STATE_ROUND_INTERVAL) * STATE_ROUND_INTERVAL;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Node)) return false;
                Node other = (Node)obj;

                bool cond1 = applyInterval(other.state.plr1PaddleY) == applyInterval(state.plr1PaddleY);
                bool cond2 = applyInterval(other.state.plr2PaddleY) == applyInterval(state.plr2PaddleY);
                bool cond3 = applyInterval(other.state.ballX) == applyInterval(state.ballX);
                bool cond4 = applyInterval(other.state.ballY) == applyInterval(state.ballY);

                return cond1 && cond2 && cond3 && cond4;
            }

            private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
            {
                Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
            }

            /// <summary>
            /// Encoding the information about the states.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {

                int hash = 13;
                hash = hash * 23 + applyInterval(state.plr1PaddleY);
                hash = hash * 23 + applyInterval(state.plr2PaddleY);
                hash = hash * 23 + applyInterval(state.ballX);
                hash = hash * 23 + applyInterval(state.ballY);
                return hash;
            }

            /// <summary>
            /// Comprator, so the priority queue works with this.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            int IComparable<Node>.CompareTo(Node other)
            {
                if (other.F < F)
                    return 1;
                else if (other.F == F)
                    return 0;
                else
                    return -1;
            }
        }

        /// <summary>
        /// For this I assumed the agen is player1, and only uses A* calculation when the 
        /// ball is flying towards it.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public PlayerAction GetAction(StaticState config, DynamicState state)
        {
            root = new Node() { state=state,parent=null,action=PlayerAction.NONE,Cost=0};
            forwardModel=new PongEngine(config);
            this.config = config;
            forwardModel.SetState(root.state);
            Node finalNode;
            PlayerAction action;
            //predictedBallY = getBallPositionOnHit(config, state);
            finalNode = AStarSearch();
            action = returnAction(finalNode);
         
            return action;
        }

        /// <summary>
        /// The A* algorithm. The timeout part of the algorithm is not used.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="goal"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private Node AStarSearch()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Dictionary<Node,float> visitedCosts=new Dictionary<Node, float>();
            PriorityQueue<Node> frontier = new PriorityQueue<Node>();
            frontier.Enqueue(root);
            while(frontier.Size>0) 
            {
                Node current_node = frontier.Dequeue();
                if (goalTest(current_node))
                    return current_node;
                PlayerAction otherPlayerAction = otherPlayer.GetAction(config, current_node.state);
                foreach(PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
                {
                    Node nextState=current_node.Clone();
                    nextState.parent = current_node;
                    nextState.action = action;
                    TickResult res = forwardModel.Tick(nextState.state, action,otherPlayerAction, timeDelta);
                    nextState.Cost = pathCost(nextState);
                    nextState.Heuristic = heuristic(nextState);
                    if (visitedCosts.ContainsKey(nextState))
                        if (nextState.F >= visitedCosts[nextState])
                            continue;
                    visitedCosts[nextState]=nextState.F;
                    frontier.Enqueue(nextState);
                }
                if(timer.Elapsed.Milliseconds > TimeOut/1000)
                    return frontier.Dequeue();
            }
            return new Node();
        }

        /// <summary>
        /// Returns the ball's predicted Y position at the player'a X position.
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        private float getBallPositionOnHit(StaticState config, DynamicState currentState)
        {
            float borderXCoord = config.ClientSize_Width / 2;
            float borderYCoord = config.ClientSize_Height / 2;
            float paddle1PosX = -borderXCoord + config.paddleWidth();
            float ballYVelocity = currentState.ballVelocityY * currentState.ballYDirection;

            float deltaTime = Math.Abs(paddle1PosX - currentState.ballX) / Math.Abs(currentState.ballVelocityX);
            float ballYPosDelta = deltaTime * ballYVelocity;
            float distFromWall;
            if(ballYVelocity<0)
            {
                distFromWall = Math.Abs((-1)*borderYCoord-currentState.ballY);
            }
            else
            {
                distFromWall = Math.Abs(borderYCoord - currentState.ballY);
            }
            if (Math.Abs(ballYPosDelta) < distFromWall)
            {
                return ballYPosDelta + currentState.ballY;
            }
            float ballYPosDeltaAbs=Math.Abs(ballYPosDelta) - distFromWall;
            int numBounces = (int)Math.Floor(ballYPosDeltaAbs / config.ClientSize_Height)+1;
            float leftoverY = ballYPosDeltaAbs % config.ClientSize_Height;
            if (numBounces % 2 == 1)
            {
                if (ballYVelocity < 0)
                    return -Math.Sign(ballYVelocity) * leftoverY - borderYCoord;
                else
                    return -Math.Sign(ballYVelocity) * leftoverY + borderYCoord;
            }   
            if (ballYVelocity < 0)
                return Math.Sign(ballYVelocity) * leftoverY + borderYCoord;
            else
                return Math.Sign(ballYVelocity) * leftoverY - borderYCoord;
        }

        /// <summary>
        /// The distance between the predicted ball position, and the paddle position.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private float heuristic(Node node)
        {
            forwardModel.SetState(node.state);
            Vector2 ballPos =new Vector2(node.state.ballX,node.state.ballY);
            Vector2 paddlePos = new Vector2(forwardModel.plr1PaddleBounceX(), node.state.plr1PaddleY);

            float paddleUpperSide = node.state.plr1PaddleY + 3*config.paddleHeight() / 8;
            float paddleLowerSide = node.state.plr1PaddleY - 3* config.paddleHeight() / 8;

            if (Math.Abs(ballPos.Y-paddlePos.Y)<40) 
            {
                if (ballPos.Y < paddleLowerSide || ballPos.Y > paddleUpperSide)
                    return 0;
                else
                    return 100;
            }
                
            return (ballPos-paddlePos).Length;
        }

        /// <summary>
        /// Returns the path from the goal node, to the start node.
        /// </summary>
        /// <returns></returns>
        private PlayerAction returnAction(Node finalNode)
        {
            if (finalNode.parent==null)
                return finalNode.action;
            Node firstNodeAfterRoot = finalNode.parent;
            Node prevNode = finalNode;
            while(firstNodeAfterRoot.parent!=null)
            {
                prevNode=firstNodeAfterRoot;
                firstNodeAfterRoot = firstNodeAfterRoot.parent;
            }
            return prevNode.action;
        }

        /// <summary>
        /// Calculates the cost of the node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private float pathCost(Node node)
        {
            //return node.parent.Cost+Math.Abs(node.parent.state.plr1PaddleY-node.state.plr1PaddleY);
            return node.parent.Cost += 1;
        }


        /// <summary>
        /// Returns whether the node is a goal state
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool goalTest(Node node)
        {
            /*if (predictedBallY - config.paddleHeight() / 4.0f<=node.state.plr1PaddleY && node.state.plr1PaddleY<=predictedBallY+config.paddleHeight()/4.0f)
                return true;*/
            forwardModel.SetState(node.state);
            return forwardModel.ballHitsLeftPaddle();
        }
    }
}
