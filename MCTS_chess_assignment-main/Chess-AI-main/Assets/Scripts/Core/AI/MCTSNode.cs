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
        private const float C = 1.41f; //Some number, not final
        public MCTSNode Parent { get; set; }
        public int Wins { get; set; }
        public int TimesVisited { get; set; }
        public Board State { get; set; }
        public List<MCTSNode> Children { get { return childrenList; } }

        public MCTSNode(MCTSNode parent,Board state)
        {
            childrenList= new List<MCTSNode>();
            Parent = parent;
            Wins = 0;
            TimesVisited = 0;
            State = state;
        }

        public void AddChild(MCTSNode child)
        {
            childrenList.Add(child);
        }

        /// <summary>
        /// Returns the child with maximum UCT value
        /// </summary>
        /// <returns></returns>
        public MCTSNode getBestChild()
        {
            return childrenList.OrderByDescending(x => x.UCTVal).First(); 
        }

        //Property, or can be a static method.
        public float UCTVal{
            get
            {
                float log = Mathf.Log(Parent.TimesVisited) / TimesVisited;
                return Wins/TimesVisited+C*Mathf.Sqrt(log);
            }
        }
    }

    
}