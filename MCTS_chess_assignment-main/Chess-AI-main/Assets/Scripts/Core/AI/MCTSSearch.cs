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

            throw new NotImplementedException();
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