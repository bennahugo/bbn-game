using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBN_Game.AI
{
    class PathInformation
    {
        private List<Node> objectRemainingPath;
        internal List<Node> remainingPath
        {
            get
            {
                return objectRemainingPath;
            }
            set
            {
                objectRemainingPath = value;
                previousNode = null;
                if (objectRemainingPath != null)
                {
                    if (objectRemainingPath.Count > 0)
                        currentWaypoint = objectRemainingPath.Last();
                }
                else
                {
                    currentWaypoint = null;
                    objectRemainingPath = new List<Node>();
                }
                currentEdge = null;
            }
        }
        internal Node previousNode;
        internal Node currentWaypoint;
        internal Edge currentEdge;
        public PathInformation()
        {
            objectRemainingPath = new List<Node>();
        }
        internal void calculateCurrentEdge()
        {
            if (previousNode == null)
                currentEdge = null;    //there is no previous edge
            if (objectRemainingPath != null)
            {
                if (objectRemainingPath.Count > 0)
                {
                    foreach (Edge e in previousNode.connectedEdges)
                        if (e.node1 == currentWaypoint || e.node2 == currentWaypoint)
                        {
                            currentEdge = e;
                            break;
                        }
                }
                else currentEdge = null; //path is now finished
            }
            else currentEdge = null; //no path
        }
        internal void reachedWaypoint()
        {
            if (objectRemainingPath != null) //if there is a path
            {
                if (objectRemainingPath.Count > 1) //if we have not reached our destination yet
                {
                    previousNode = objectRemainingPath.Last();
                    objectRemainingPath.Remove(objectRemainingPath.Last());
                    currentWaypoint = objectRemainingPath.Last();
                    calculateCurrentEdge();
                }
                else //the destination have been reached 
                {
                    previousNode = null;
                    objectRemainingPath.Clear();
                    currentWaypoint = null;
                    currentEdge = null;
                }
            }
        }
    }
}
