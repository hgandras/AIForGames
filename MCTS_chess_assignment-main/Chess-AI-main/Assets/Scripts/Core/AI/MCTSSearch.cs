namespace Chess
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using static System.Math;
    using System.Linq;
    class MCTSSearch : ISearch
    {
        public event System.Action<Move> onSearchComplete;

        MoveGenerator moveGenerator;

        Move bestMove;
        int bestEval;
        bool abortSearch;

        MCTSSettings settings;
        Board board;
        Evaluation evaluation;
        MCTSNode root;

        System.Random rand;

        // Diagnostics
        public SearchDiagnostics Diagnostics { get; set; }
        System.Diagnostics.Stopwatch searchStopwatch;

        public MCTSSearch(Board board, MCTSSettings settings)
        {
            this.board = board;
            this.settings = settings;
            evaluation = new Evaluation();
            moveGenerator = new MoveGenerator();
            rand = new System.Random();
            root = new MCTSNode(null, this.board,Move.InvalidMove);
        }

        public void StartSearch()
        {
            InitDebugInfo();

            // Initialize search settings
            bestEval = 0;
            bestMove = Move.InvalidMove;

            moveGenerator.promotionsToGenerate = settings.promotionsToSearch;
            abortSearch = false;
            Diagnostics = new SearchDiagnostics();

            SearchMoves();

            onSearchComplete?.Invoke(bestMove);

            if (!settings.useThreading)
            {
                LogDebugInfo();
            }
        }

        public void EndSearch()
        {
            if (settings.useTimeLimit)
            {
                abortSearch = true;
            }
        }

        void SearchMoves()
        {
            // TODO
            // Don't forget to end the search once the abortSearch parameter gets set to true.
            int i = 0;
            while (!abortSearch)
            {
                if (settings.limitNumOfPlayouts && i == settings.maxNumOfPlayouts)
                    break;
                //Debug.Log("Iteration: "+i);
                MCTSNode leafNode = Selection(root);
                MCTSNode expandedNode;
                if (leafNode == root)
                    expandedNode = ExpandNode(leafNode, true);
                else
                    expandedNode = ExpandNode(leafNode, false);

               // Debug.Log(expandedNode.Parent == root);
               // Debug.Log("Selection ended"");
                float reward = Simulate(expandedNode);
               // Debug.Log("Simulation ended");
                Backpropagate(expandedNode, reward);
                //Debug.Log("Beckprop ended");
                bestMove = root.Children.OrderByDescending(x => x.AvgReward).First().Move;
                //Break if search is aborted, or max num playouts is reached
                i++;
            }
            //Debug.Log("Num iterations: "+i);
        }

        /// <summary>
        /// Traverses the tree by the UCB policy, and stops at a leaf node. If children was not generated for a node, the node is
        /// expanded and all of the possible children are generated. Then the UCB policy is applied.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        MCTSNode Selection(MCTSNode currentNode)
        {
            while (!currentNode.isLeafNode)
            {
                currentNode = currentNode.getBestChild();
            }
            return currentNode;
        }
        
        /// <summary>
        /// Generates the child nodes of a node that is unexpanded.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="root"></param>
        MCTSNode ExpandNode(MCTSNode currentNode,bool root)
        {
            if (currentNode.childrenGenerated) return currentNode.getBestChild();

            //Get the moves, generate new states and nodes with them.
            List<Move> possibleMoves = moveGenerator.GenerateMoves(currentNode.State, root);
            possibleMoves.Reverse();
            foreach (Move move in possibleMoves)
            {
                Board newState = currentNode.State.Clone();
                newState.MakeMove(move);
                MCTSNode newChild = new MCTSNode(currentNode, newState,move);
                currentNode.AddChild(newChild);
            }
            currentNode.childrenGenerated = true;
            return currentNode.getBestChild();
            //Debug.Log(currentNode.Children.Count+" "+currentNode.childrenGenerated);
        }

        /// <summary>
        /// Makes a playout from the given node until the game is finished, or the maximum simulation steps are reached.
        /// Returns whether the player of the starting node won, or lost. A win returns a 1, a loss a 0, and an unfinished simulation returns a number between 1 and 0 (calls evaluation.EvaluateSimBoard).
        /// </summary>
        /// <param name="startNode"></param>
        private float Simulate(MCTSNode startNode)
        {
            SimPiece[,] currentState = startNode.State.GetLightweightClone();
            bool team = startNode.State.WhiteToMove;
            bool nodeTeam = team;
            bool gameFinished = false;
            for (int i = 0; i < settings.playoutDepthLimit;i++)
            {
                List<SimMove> possibleMoves=moveGenerator.GetSimMoves(currentState, team);
                if (possibleMoves.Count == 1)
                {
                    //Debug.Log("Game finished");
                    gameFinished = true;
                    break;
                }
                int index=UnityEngine.Random.Range(0, possibleMoves.Count);
                SimMove move = possibleMoves[index];
                simMoveLightWeight(currentState,move);
                team = !team;
            }
            /** If the game is not finished, during backpropagation no wins are added to any of the
            * nodes, so it is different from simply losing.*/
            if (!gameFinished)
            {
                //Debug.Log("Team1: " + evaluation.EvaluateSimBoard(currentState, nodeTeam) + "Team2: " + evaluation.EvaluateSimBoard(currentState, !nodeTeam));
                return evaluation.EvaluateSimBoard(currentState, nodeTeam);
            }
            else if (nodeTeam == team)
                return 1;
            return 0;
        }

        /// <summary>
        /// Simulates a step of the lightweight clone.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        private void simMoveLightWeight(SimPiece[,] state, SimMove move)
        {
            SimPiece piece = state[move.startCoord1, move.startCoord2];
            state[move.startCoord1, move.startCoord2] = null;
            state[move.endCoord1, move.endCoord2] = piece;
        }

        /// <summary>
        /// Moves back from the selected node, and based on the outcome of the game, it sets the tree's nodes wins,
        /// and TimesVisited parameters.
        /// </summary>
        /// <param name="leafNode"></param>
        /// <param name="reward"></param>
        private void Backpropagate(MCTSNode leafNode, float reward)
        {
            MCTSNode currentNode = leafNode;
            currentNode.TimesVisited += 1;
            currentNode.Reward += reward;
            bool teamSwitch = true; //Keeping track of the team the current node belongs to
            while(currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
                teamSwitch=!teamSwitch;
                currentNode.TimesVisited += 1;
                if (teamSwitch)
                    currentNode.Reward += reward;
                else if (!teamSwitch)
                    currentNode.Reward += (1 - reward);
            }
        }


        void LogDebugInfo()
        {
            // Optional
        }

        void InitDebugInfo()
        {
            searchStopwatch = System.Diagnostics.Stopwatch.StartNew();
            // Optional
        }
    }
}