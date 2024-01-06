namespace Chess
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using static System.Math;
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
            root = new MCTSNode(null, board);
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
            MCTSNode selectedNode=Selection(root);
            int winner=Simulate(selectedNode);
            Backpropagate(selectedNode,winner);
        }

        /// <summary>
        /// Traverses the tree by the UCTS policy, and stops at a leaf node. If children was not generated for a node, the node is
        /// expanded and all of athe possible children are generated. Then the UCT policy is applied. 
        /// </summary>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        MCTSNode Selection(MCTSNode currentNode)
        { 
            ExpandNode(currentNode,true);//Does nothing if children are generated
            currentNode = currentNode.getBestChild();
            while(!currentNode.isLeafNode)
            {
                currentNode = currentNode.getBestChild();
                ExpandNode(currentNode, false);
            }
            return currentNode;
        }
        
        /// <summary>
        /// Generates the child nodes of a node that is unexpanded.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="root"></param>
        void ExpandNode(MCTSNode currentNode,bool root)
        {
            if (currentNode.childrenGenerated) return;

            //Get the moves, generate new states and nodes with them.
            List<Move> possibleMoves = moveGenerator.GenerateMoves(currentNode.State, root);
            foreach (Move move in possibleMoves)
            {
                Board newState = currentNode.State.Clone();
                newState.MakeMove(move);
                MCTSNode newChild = new MCTSNode(currentNode, newState);
                currentNode.AddChild(newChild);
            }
            currentNode.childrenGenerated = true;
        }

        /// <summary>
        /// Makes a playout from the given node until the game is finished, or the maximum simulation steps are reached.
        /// Returns whether the player of the starting node won, or lost. A win returns of 1, a loss a 0, and an unfinished simulation returns a 2.
        /// </summary>
        /// <param name="startNode"></param>
        private  int Simulate(MCTSNode startNode)
        {
            SimPiece[,] currentState = startNode.State.GetLightweightClone();
            bool team = currentState[0,0].team;
            bool nodeTeam = team;
            bool gameFinished = false;
            for (int i = 0; i < settings.playoutDepthLimit;i++)
            {
                List<SimMove> possibleMoves=moveGenerator.GetSimMoves(currentState, team);
                if (possibleMoves.Count == 1)
                {
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
                return 2;
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
        /// <param name="outcome"></param>
        private void Backpropagate(MCTSNode leafNode, int outcome)
        {
            MCTSNode currentNode = leafNode;
            currentNode.TimesVisited += 1;
            if (outcome == 1) 
                currentNode.Wins += 1;
            int teamSwitch = 0; //Keeping track of the team the current node belongs to
            do
            {
                currentNode = currentNode.Parent;
                teamSwitch++;
                currentNode.TimesVisited += 1;
                if ((outcome == 1 && teamSwitch % 2 == 0) || (outcome == 0 && teamSwitch % 2 == 1))
                    currentNode.Wins += 1;
            }while (currentNode != root);
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