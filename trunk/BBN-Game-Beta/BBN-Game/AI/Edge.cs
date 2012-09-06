using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
namespace BBN_Game.AI
{
    /// <summary>
    /// Basic edge connecting path nodes
    /// </summary>
    class Edge
    {
        /// <summary>
        /// An edge can be used exclusively by one AI object
        /// </summary>
        internal Boolean beingUsedByAI { get; set; } 
        /// <summary>
        /// There are two nodes per edge
        /// </summary>
        public Node node1 { get; private set; }
        public Node node2 { get; private set; }
        /// <summary>
        /// Returns the distance between nodes, precomputed to speed up runtime performance
        /// </summary>
        public float distance { get; internal set; }
        /// <summary>
        /// Weight assosiated with this edge. A heigher weight will make the edge less attractive to the AI
        /// </summary>
        public float weight { get; set; }
        /// <summary>
        /// Constructor of an edge
        /// </summary>
        /// <param name="node1">node 1 where node 1 != node 2</param>
        /// <param name="node2">node 2 where node 2 != node 1</param>
        /// <param name="edgeWeight">Initial weight of the edge ( a higher number is less attractive to the AI )</param>
        public Edge(Node node1, Node node2, float edgeWeight)
        {
            this.node1 = node1;
            this.node2 = node2;
            distance = (node1.Position - node2.Position).Length();   //precompute heuristic so that the run-time of the game is spead up
            weight = edgeWeight;
        }

    }
}
