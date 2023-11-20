using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prong
{
    internal class PlayerAIAStar : Player
    {
        private const float TimeOut = 0.05f;
        private  float timeDelta = 0.05f;
        private PriorityQueue<Node> frontier = new PriorityQueue<Node>();
        private DynamicState root;
        PongEngine forwardModel;

        public class Node
        {
            public DynamicState state { get; set; }
            public Node parent { get; set; }
            public float priority { get; set; }
            public PlayerAction action { get; set; }
            public float Cost { get; set; }

            public Node Clone()
            {
                Node clonedNode = new Node()
                {
                    state = state.Clone(),
                    parent = this.parent,
                    priority = this.priority,
                    action = this.action,
                    Cost = this.Cost
                };
                return clonedNode;
            }
        }


        public PlayerAction GetAction(StaticState config, DynamicState state)
        {
            root = state;
            forwardModel=new PongEngine(config);
            forwardModel=new PongEngine(config);
            Node finalNode;
            PlayerAction action;
            if (forwardModel.ballFlyingRight())
                return PlayerAction.NONE;
            else
            {
                finalNode = AStarSearch(config);
                action = returnAction(finalNode);
            }
            return action;
        }

        

        /// <summary>
        /// Idea for heuristic: Take the ball's speed into account, and make the heuristic the paddle's distance from the ball's 
        /// future position.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private float heuristic(Node node)
        {
            return 0;
        }

        /// <summary>
        /// Performs the A* algorithm
        /// </summary>
        /// <param name="root"></param>
        /// <param name="goal"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private Node AStarSearch(StaticState config)
        {
            forwardModel.SetState(root);
            Dictionary<Node,float> costs=new Dictionary<Node, float>();
            Node rootNode = new Node() { };
            frontier.Enqueue(rootNode,0);
            while(frontier.Size>0) 
            {
                Node current_node = frontier.Pop();
                if (forwardModel.ballHitsLeftPaddle())
                    return current_node;
                foreach(PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
                {
                    Node nextState=current_node.Clone();
                    TickResult res = forwardModel.Tick(nextState.state, action, PlayerAction.NONE, timeDelta);
                    nextState.Cost = pathCost(nextState) + heuristic(nextState);
                    if (costs.ContainsKey(nextState))
                        if (nextState.Cost >= costs[nextState])
                            continue;
                    costs.Add(nextState, nextState.Cost);
                    frontier.Enqueue(nextState, nextState.Cost);
                }
            }

            return new Node();
        }

        /// <summary>
        /// Returns the path from the goal node, to the start node.
        /// </summary>
        /// <returns></returns>
        private PlayerAction returnAction(Node finalNode)
        {
            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private float pathCost(Node node)
        {
            return 0;
        }

    }
}
