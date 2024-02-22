namespace Chess
{
    using System;
    using System.Collections.Generic;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using System.Linq;

    /// <summary>
    /// - A node should contain the parameters of the UCT policy, so it can be calculated. 
    /// - It should contain the parent node, so the backpropagation can be done
    /// - Current state, so the model can be moved forward
    /// - Must have the child nodes, so the next state can be chosen. 
    ///  
    /// </summary>
    public class MCTSNode
    {
        private List<MCTSNode> childrenList;
        public bool childrenGenerated;
        private const float C = 1.0f; //Some number, not final
        public MCTSNode Parent { get; set; } //null for root node
        public float Reward { get; set; } //Accumulated rewards
        public int TimesVisited { get; set; } 
        public Board State { get; set; }
        public List<MCTSNode> Children { get { return childrenList; } }
        public Move Move { get; set; }
        public float AvgReward { get { return Reward / TimesVisited; } }

        public MCTSNode(MCTSNode parent,Board state, Move move)
        {
            childrenList= new List<MCTSNode>();
            Parent = parent;
            Reward = 0;
            TimesVisited = 0;
            State = state;
            Move = move;
            childrenGenerated = false;
        }

        public void AddChild(MCTSNode child)
        {
            childrenList.Add(child);
        }

        /// <summary>
        /// Returns the child with maximum UCT value. If the node does not have any children, it returns null.
        /// </summary>
        /// <returns></returns>
        public MCTSNode getBestChild()
        {
            if (childrenList.Count == 0)
            {
                return null;
            }
            return childrenList.OrderByDescending(x => x.UCTVal).First(); 
        }

        //Returns infinity if the node was not visited.
        public float UCTVal{
            get
            {
                if (TimesVisited == 0)
                    return float.PositiveInfinity;
                float log = Mathf.Log(Parent.TimesVisited) / TimesVisited;
                return Reward/TimesVisited+C*Mathf.Sqrt(log);
            }
        }

        /// <summary>
        /// There are 2 type of leaf nodes:
        /// -Either a node, that has at least one children without playouts
        /// -A node without children if it is not expanded.
        /// </summary>
        /// <param name="Node"></param>
        /// <returns></returns>
        public bool isLeafNode
        {
            get
            {
                if(!childrenGenerated)
                    return true;

                foreach(MCTSNode child in childrenList)
                {
                    if (child.TimesVisited == 0)
                        return true;
                }
                return false;
            }
        }
    }

    
}