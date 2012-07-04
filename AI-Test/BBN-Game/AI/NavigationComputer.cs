using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.Objects;
using Microsoft.Xna.Framework;
using BBN_Game.Utils;

namespace BBN_Game.AI
{
    class NavigationComputer
    {
        private const float DISTANCE_TO_WAYPOINT_IN_SECONDS_WHEN_CLOSE = 0.5f;
        private const float DISTANCE_TO_WAYPOINT_IN_SECONDS_WHEN_VERY_CLOSE = 0.05f;
        private const double EPSILON_DISTANCE = 0.0001f;
        private const int TURN_INTERPOLATION_STEPS = 10;
        private const float TURNING_SPEED_COEF = 1.05f;
        private Dictionary<DynamicObject, List<Node>> objectPaths;
        //debug only:
        public List<Node> getPath(DynamicObject o)
        {
            return objectPaths[o];
        }
        /// <summary>
        /// Default constructor for the class
        /// </summary>
        public NavigationComputer()
        {
            objectPaths = new Dictionary<DynamicObject, List<Node>>();
        }
        /// <summary>
        /// Sets a new path for an object if it is already registered
        /// </summary>
        /// <param name="AIObject">Any registered dynamic object</param>
        /// <param name="start">Start node (should be close to the object if possible)</param>
        /// <param name="end">End node</param>
        public void setNewPathForRegisteredObject(DynamicObject AIObject, Node start, Node end)
        {
            if (objectPaths.Keys.Contains(AIObject))
                objectPaths[AIObject] = AStar(start, end);
        }
        /// <summary>
        /// Method to register an object for computer-based navigation
        /// </summary>
        /// <param name="AIObject">Any dynamic object capable of moving</param>
        public void registerObject(DynamicObject AIObject)
        {
            if (!objectPaths.Keys.Contains(AIObject))
                objectPaths.Add(AIObject, null);
        }
        /// <summary>
        /// Method to deregister an object in order to stop computer-based navigation for that object
        /// </summary>
        /// <param name="AIObject">Currently registered dynamic object</param>
        public void deregisterObject(DynamicObject AIObject)
        {
            if (objectPaths.Keys.Contains(AIObject))
                objectPaths.Remove(AIObject);
        }
        /// <summary>
        /// Method to update the movement of all registered AI characters. The list of waypoints have to be in reverse order (as returned by the A*)
        /// </summary>
        /// <param name="gt">Game time as passed on by the game loop</param>
        public void updateAIMovement(GameTime gt)
        {
            foreach (DynamicObject ai in objectPaths.Keys)
            {
                List<Node> path = objectPaths[ai];
                float closeToWaypoint = ai.getMaxSpeed * DISTANCE_TO_WAYPOINT_IN_SECONDS_WHEN_CLOSE;
                float veryCloseToWaypoint = ai.getMaxSpeed * DISTANCE_TO_WAYPOINT_IN_SECONDS_WHEN_VERY_CLOSE;
                if (path != null) //if there is a path
                {
                    if (path.Count > 0) //if we have not reached our destination yet
                    {
                        Node nextWaypoint = path.Last(); //the last waypoint is the next to move to
                        float distToWayPoint = (nextWaypoint.Position - ai.Position).Length();
                        //if very close to the next waypoint remove that waypoint so that we can go to the next:
                        if (distToWayPoint <= veryCloseToWaypoint)
                            path.Remove(nextWaypoint);
                        else
                        {   //We want our ship to slowly rotate towards the direction it has to move in:
                            //Calculate yaw and pitch for view direction and target direction
                            Vector3 vWantDir = Vector3.Normalize(nextWaypoint.Position - ai.Position);
                            float distance = (float)Math.Sqrt(vWantDir.Z * vWantDir.Z + vWantDir.X * vWantDir.X);
                            float tpitch = distance == 0 ? (float)Math.Sign(-vWantDir.Y) * (float)Math.PI / 2 : -(float)Math.Atan2(vWantDir.Y, distance);
                            float tyaw = (float)Math.Atan2(vWantDir.X, vWantDir.Z);
                            Vector3 vLookDir = Vector3.Normalize(-Matrix.CreateFromQuaternion(ai.rotation).Forward);
                            distance = (float)Math.Sqrt(vLookDir.Z * vLookDir.Z + vLookDir.X * vLookDir.X);
                            float cyaw = (float)Math.Atan2(vLookDir.X, vLookDir.Z);
                            float cpitch = distance == 0 ? (float)Math.Sign(-vLookDir.Y) * (float)Math.PI / 2 : -(float)Math.Atan2(vLookDir.Y, distance);

                            //now rotate towards the target yaw and pitch
                            float diffy = tyaw - cyaw;
                            float diffp = tpitch - cpitch;

                            //get the direction we need to rotate in:
                            if (Math.Abs(diffy) > Math.PI)
                                if (tyaw > cyaw)
                                    diffy = -(float)(Math.PI * 2 - Math.Abs(diffy));
                                else
                                    diffy = (float)(Math.PI * 2 - Math.Abs(diffy));

                            if (Math.Abs(diffp) > Math.PI)
                                if (tpitch > cpitch)
                                    diffp = -(float)(Math.PI * 2 - Math.Abs(diffp));
                                else
                                    diffp = (float)(Math.PI * 2 - Math.Abs(diffp));

                            if (Math.Abs(diffp) > Math.Abs(ai.getpitchSpeed))
                                diffp = Math.Sign(diffp) * Math.Abs(ai.getpitchSpeed);
                            if (Math.Abs(diffy) > Math.Abs(ai.getYawSpeed))
                                diffy = Math.Sign(diffy) * Math.Abs(ai.getYawSpeed);

                            
                            Matrix m = Matrix.CreateFromQuaternion(ai.rotation);
                            Quaternion pitch = Quaternion.CreateFromAxisAngle(m.Right, diffp * (float)(gt.ElapsedGameTime.TotalSeconds));    
                            Quaternion yaw = Quaternion.CreateFromAxisAngle(m.Up, diffy * (float)(gt.ElapsedGameTime.TotalSeconds));
                            //special case: deal with the pitch if its PI/2 or -PI/2, because if its slightly off it causes problems:
                            if (Math.Abs(Math.Abs(tpitch) - Math.PI / 2) <= EPSILON_DISTANCE && !(Math.Abs(diffy) <= EPSILON_DISTANCE))
                                ai.rotation = Quaternion.CreateFromYawPitchRoll(tyaw, tpitch, 0);
                            else
                                ai.rotation = yaw * pitch * ai.rotation;
                            
                            //now set the speed:
                            float compLookOntoWant = Vector3.Dot(vLookDir, vWantDir);
                            if (Math.Abs(compLookOntoWant) > 1)
                                compLookOntoWant = 1;
                            if (distToWayPoint <= closeToWaypoint)
                                ai.ShipMovementInfo.speed = ai.getMaxSpeed * (float)(Math.Pow(TURNING_SPEED_COEF,- Math.Abs(Math.Acos(compLookOntoWant) * 180 / Math.PI)));
                            else
                                ai.ShipMovementInfo.speed = ai.getMaxSpeed * (float)(Math.Pow(TURNING_SPEED_COEF, -Math.Abs(Math.Acos(compLookOntoWant) * 180 / Math.PI)));
                        }
                    }
                    else objectPaths[ai].Clear();
                }
            }
        }
        /// <summary>
        /// A* path finding algorithm.
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="end">End node</param>
        /// <returns>Path to end node if one is found, otherwise null</returns>
        public static List<Node> AStar(Node start, Node end)
        {

            List<Node> openList = new List<Node>();
            List<Node> visitedList = new List<Node>();
            openList.Add(start);
            start.heuristic = (end.Position - start.Position).Length(); //calculate heuristic
            while (openList.Count > 0)  //while we have more nodes to explore
            {
                Node bestChoice = openList.ElementAt<Node>(0);
                //get the best node choice:
                foreach (Node node in openList)                       
                    if (bestChoice.pathCost + bestChoice.heuristic < node.pathCost + node.heuristic)
                    {
                        bestChoice = node;
                        break;
                    }
                //move best choice to visited list
                visitedList.Add(bestChoice);
                bestChoice.hasBeenVisited = true;
                openList.Remove(bestChoice);

                if (bestChoice == end) //REACHED DESTINATION!!!
                {
                    List<Node> result = new List<Node>();
                    
                    result.Add(end);
                    Node it = (end.edgeToPrevNode.node1 == end) ? end.edgeToPrevNode.node2 : end.edgeToPrevNode.node1;
                    
                    while (it != null)
                    {
                        result.Add(it);
                        if (it.edgeToPrevNode != null)
                            it = (it.edgeToPrevNode.node1 == it) ? it.edgeToPrevNode.node2 : it.edgeToPrevNode.node1;
                        else
                            it = null;
                    }
                    //Finally clear node data for the next iteration of the A*:
                    foreach (Node node in visitedList)
                        node.clear();
                    return result;
                }
                //Not yet at destination so we look at the best choice's neighbours:
                foreach (Edge neighbourEdge in bestChoice.connectedEdges)
                {
                    Node neighbour = (neighbourEdge.node1 == bestChoice) ? neighbourEdge.node2 : neighbourEdge.node1;
                    if (neighbour.hasBeenVisited) continue;

                    visitedList.Add(neighbour);
                    double distToNeighbour = neighbourEdge.distance + neighbourEdge.weight;
                    double newMoveLength = distToNeighbour + bestChoice.pathCost;
                    neighbour.heuristic = (neighbour.Position - end.Position).Length();
                    Boolean shouldMove = false;

                    if (openList.IndexOf(neighbour) < 0)
                    {
                        openList.Add(neighbour);
                        shouldMove = true;
                    }
                    else if (distToNeighbour < neighbour.pathCost)
                        shouldMove = true;
                    else
                        shouldMove = false;

                    if (shouldMove == true)
                    {
                        neighbour.edgeToPrevNode = neighbourEdge;
                        neighbour.pathCost = neighbour.heuristic + distToNeighbour;
                    }
                }
            }
            return null; //Not found
        }
    }
}
