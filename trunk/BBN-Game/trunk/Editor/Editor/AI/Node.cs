using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
namespace BBN_Game.AI
{
    /// <summary>
    /// Path node class
    /// </summary>
    class Node:Marker
    {
        /// <summary>
        /// All the connected edges are bidirectional
        /// </summary>
        private List<Edge> connectedEdges = new List<Edge>();
        /// <summary>
        /// Constructors to accomodate inheritance from Marker class
        /// </summary>
        public Node():
            base() { }        
        public Node(Vector3 pos, int OwningTeam):
            base(pos,OwningTeam) { }
        /// <summary>
        /// Gets the number of connected edges
        /// </summary>
        /// <returns>positive integer</returns>
        public int getEdgeCount()
        {
            return connectedEdges.Count;
        }
        /// <summary>
        /// get edge at index specified
        /// </summary>
        /// <param name="edgeIndex">index of edge in question. Must be in range.</param>
        /// <returns>Edge instance</returns>
        public Edge getEdge(int edgeIndex)
        {
            return connectedEdges.ElementAt(edgeIndex);
        }
        /// <summary>
        /// Method to establish connection to another node.
        /// </summary>
        /// <param name="anotherNode">Node not the same instance as the node the method is invoked on</param>
        /// <param name="weightOfConnection">Weight of the established connection (not the distance, but a higher number will result in the edge being less desirable for the AI</param>
        public void connectToNode(Node anotherNode, float weightOfConnection)
        {
            if (anotherNode != this)
            {
                foreach (Edge e in this.connectedEdges)
                    if (e.node1 == anotherNode || e.node2 == anotherNode)
                        return;         //already connected
                Edge conn = new Edge(this, anotherNode, weightOfConnection);
                //Bidirectional edge:
                connectedEdges.Add(conn);
                anotherNode.connectedEdges.Add(conn);
            }
        }
        /// <summary>
        /// Method to establish connection to another node.
        /// </summary>
        /// <param name="anotherNode">Node not the same instance as the node the method is invoked on</param>
        /// <param name="weightOfConnection">Weight of the established connection (not the distance, but a higher number will result in the edge being less desirable for the AI</param>
        /// <param name="distanceToNode">Sets the distance</param>
        public void connectToNode(Node anotherNode, float weightOfConnection, float distanceToNode)
        {
            connectToNode(anotherNode, weightOfConnection);
            foreach (Edge e in this.connectedEdges)
                if (e.node1 == anotherNode || e.node2 == anotherNode)
                    e.distance = distanceToNode;
        }
        /// <summary>
        /// Method to disconnect another node
        /// </summary>
        /// <param name="otherNode">node to disconnect</param>
        public void disconnectFromNode(Node otherNode)
        {
            if (otherNode == this) return;
            Edge foundEdge = null;
            foreach (Edge e in this.connectedEdges)
                if (e.node1 == otherNode || e.node2 == otherNode)
                {
                    foundEdge = e;
                    break;
                }
            if (foundEdge != null)
            {
                foundEdge.node1.connectedEdges.Remove(foundEdge);
                foundEdge.node2.connectedEdges.Remove(foundEdge);
            }
        }
        /// <summary>
        /// Method to disconnect node from all other nodes
        /// </summary>
        public void disconnectAllEdges()
        {
            foreach (Edge e in this.connectedEdges)
            {
                Node other = (e.node1 == this ? e.node2 : e.node1);
                other.connectedEdges.Remove(e);
            }
            connectedEdges.Clear();
        }
    }
}
