using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.Objects;
using Microsoft.Xna.Framework;
using BBN_Game.Grid;

namespace BBN_Game.AI
{
    class NavigationComputer
    {
        private const float DISTANCE_TO_WAYPOINT_IN_SECONDS_WHEN_CLOSE = 0.5f;
        private const float DISTANCE_TO_WAYPOINT_IN_SECONDS_WHEN_VERY_CLOSE = 0.05f;
        private const double EPSILON_DISTANCE = 0.0001f;
        private const float TURNING_SPEED_COEF = 1.13f;
        private const int RETAIN_DODGE_PATH_TICKS = 100;
        private const int YIELD_TICKS = 30;
        private const int DODGE_DISTANCE_MULTIPLIER = 2; //multiplies the sum of the two radi of the objects and controls the minimum distance when the ai will start dodging
        private const float DOT_ANGLE_TO_STOP_DOGFIGHT_MOVE = 0.6f;
        private const int RADIUS_FACTOR_TO_GO_ABOVE_TARGET = 4;
        private const int RADIUS_MULTIPLIER_TO_GO_BEHIND_TARGET = 10;
        private const int TARGET_WP_BUFFER = 120;
        private const float CHASE_WHEN_FURTHER = 70;
        private const float ASTAR_RANDOM_FACTOR = 50;
        private const int AI_MOVEMENT_SLOT_COUNT = 5;
        private GridStructure spatialGrid;
        internal Dictionary<DynamicObject, PathInformation> objectPaths;
        private Dictionary<DynamicObject,int> movementYieldList;
        private Dictionary<DynamicObject, int> dodgeInactiveCountDown;
        private int currentSlot = 0;
        //private int ticks_Till_Next_Update = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="collisionGrid">Non-null collision grid</param>
        public NavigationComputer(GridStructure collisionGrid)
        {
            objectPaths = new Dictionary<DynamicObject, PathInformation>();
            movementYieldList = new Dictionary<DynamicObject,int>();
            dodgeInactiveCountDown = new Dictionary<DynamicObject, int>(); 
            spatialGrid = collisionGrid;
        }
        /// <summary>
        /// Method to get the current Path of a registered object
        /// </summary>
        /// <param name="o">registered object</param>
        /// <returns>path if it exists. Either an empty list or Null otherwise</returns>
        public List<Node> getPath(DynamicObject o)
        {
            return objectPaths[o].remainingPath;
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
            {
                List<Node> path = AStar(start, end, AIObject is Destroyer);
                if (path != null)
                    objectPaths[AIObject].remainingPath = path;
                else //we just go the the start node
                {
                    path = new List<Node>();
                    path.Add(start);
                    objectPaths[AIObject].remainingPath = path;
                }
            }
        }
        /// <summary>
        /// Method to register an object for computer-based navigation
        /// </summary>
        /// <param name="AIObject">Any dynamic object capable of moving</param>
        public void registerObject(DynamicObject AIObject)
        {
            if (!objectPaths.Keys.Contains(AIObject))
                objectPaths.Add(AIObject, new PathInformation());
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
        /// Checks if an object is registered in the navigation computer
        /// </summary>
        /// <param name="AIObject">Object to check</param>
        /// <returns>true iff object is registered</returns>
        public bool isObjectRegistered(DynamicObject AIObject)
        {
            return objectPaths.Keys.Contains(AIObject);
        }
        /// <summary>
        /// Checks if there are any obstructions in close proximity which must be dodged
        /// </summary>
        /// <param name="AIObject">ai object to check around</param>
        /// <returns>list of possible obstructions</returns>
        private List<StaticObject> obstructionsInCloseProximity(DynamicObject AIObject)
        {
            List<StaticObject> results = new List<StaticObject>();
            foreach (GridObjectInterface obj in this.spatialGrid.checkNeighbouringBlocks(AIObject))
            {
                if (obj is StaticObject && !(obj is Bullet || obj is Missile))
                {
                    float radiusToCheck = (obj.getBoundingSphere().Radius + AIObject.getBoundingSphere().Radius) * DODGE_DISTANCE_MULTIPLIER;
                    if ((AIObject.Position - obj.Position).Length() <= radiusToCheck)
                    {
                            results.Add(obj as StaticObject);
                    }
                }
            }
            return results;
        }
        /// <summary>
        /// Checks if a particular dodge is valid
        /// </summary>
        private bool isDodgeValid(DynamicObject callingAI,float dodgeAngle,int dodgeAngleMultiplierYaw,int dodgeAngleMultiplierPitch,PathInformation pathInfo, StaticObject closestObstruction,float dodgeDistance, ref Vector3 dodgeWp)
        {
            bool bFlag = false;
            //Define a conal area around the current path to choose another path from
            Quaternion qRot = Quaternion.CreateFromYawPitchRoll(dodgeAngle * dodgeAngleMultiplierYaw, dodgeAngle * dodgeAngleMultiplierPitch, 0);
            Vector3 choiceVector = Vector3.Normalize(
                Vector3.Transform(pathInfo.currentWaypoint.Position - callingAI.Position,
                    Matrix.CreateFromQuaternion(qRot)));
            dodgeWp = callingAI.Position + choiceVector * dodgeDistance;
            if ((dodgeWp - closestObstruction.Position).Length() > dodgeDistance)
            {
                foreach (GridObjectInterface o in spatialGrid.checkNeighbouringBlocks(dodgeWp))
                {
                    if (o != callingAI)
                    {

                        if (o is StaticObject)
                        {
                            if ((o.Position - dodgeWp).Length() > (o.getBoundingSphere().Radius + callingAI.getBoundingSphere().Radius * (DODGE_DISTANCE_MULTIPLIER)))
                                bFlag = true;
                        }
                        if (o is DynamicObject)
                        {
                            if (isObjectRegistered(o as DynamicObject))
                            {
                                Node otherObjectsWaypoint = objectPaths[o as DynamicObject].currentWaypoint;
                                if (otherObjectsWaypoint != null)
                                {
                                    if ((otherObjectsWaypoint.Position - dodgeWp).Length() < (o.getBoundingSphere().Radius + callingAI.getBoundingSphere().Radius * (DODGE_DISTANCE_MULTIPLIER)))
                                        bFlag = false;
                                }
                            }
                        }
                        if (!spatialGrid.isInGrid(new Node(dodgeWp,-1)))
                            bFlag = false;
                    }
                }
            }
            return bFlag;
        }
        /// <summary>
        /// Method to set the path of an ai unit in order to dodge an object
        /// </summary>
        /// <param name="callingAI">ai to perform dodge</param>
        /// <param name="closestObstruction">closest obstruction</param>
        private void dodgeObject(DynamicObject callingAI, StaticObject closestObstruction)
        {
            if (!this.isObjectRegistered(callingAI)) return;
            PathInformation pathInfo = objectPaths[callingAI];
            if (pathInfo.currentWaypoint == null) return;
            if (!dodgeInactiveCountDown.Keys.Contains(callingAI))
                dodgeInactiveCountDown.Add(callingAI, RETAIN_DODGE_PATH_TICKS);
            else
                return;
            Vector3 dodgeWp = new Vector3();
            bool bFlag = false;

            // Set the new path:
            float dodgeDistance = (callingAI.getBoundingSphere().Radius + closestObstruction.getBoundingSphere().Radius) * DODGE_DISTANCE_MULTIPLIER;
            float distanceToObject = (callingAI.Position - closestObstruction.Position).Length();
            float distanceToCurrentWp = (callingAI.Position - pathInfo.currentWaypoint.Position).Length();
            float dodgeAngle = (float)Math.Abs(Math.Atan2(distanceToObject, dodgeDistance));
            for (int i = (int)Math.Ceiling(dodgeAngle); i * dodgeAngle < Math.PI; ++i)
            {
                for (int j = (int)Math.Ceiling(dodgeAngle); j * dodgeAngle < Math.PI; ++j)
                {
                    if (isDodgeValid(callingAI, dodgeAngle, i, j, pathInfo, closestObstruction, dodgeDistance, ref dodgeWp))
                    {
                        bFlag = true;
                        break;
                    }
                    if (isDodgeValid(callingAI, -dodgeAngle, i, j, pathInfo, closestObstruction, dodgeDistance, ref dodgeWp))
                    {
                        bFlag = true;
                        break;
                    }
                }
                if (bFlag)
                    break;
            }
            List<Node> path = pathInfo.remainingPath;
            if (path.Count > 0)
                path.Remove(path.Last());
            path.Add(new Node(dodgeWp, -1));
            pathInfo.remainingPath = path;
        }
        /// <summary>
        /// Method to test if a collision will occur on the current route of the calling ai (and which collision will occur first). If such
        /// a possible collision is detected the callingAI will attempt to dodge the object.
        /// </summary>
        /// <param name="callingAI">AI that the test is performed for.</param>
        /// <param name="obstructionsList">A list of obstructions as returned by PathIntersectTest</param>
        private void avoidCollisions(DynamicObject callingAI, List<StaticObject> obstructionsList)
        {
            //find the closest obstruction and dodge it:
            if (obstructionsList.Count == 0) return;

            PathInformation pathInfo = objectPaths[callingAI];
            StaticObject closestObstruction = obstructionsList.First();
            float closestObstructionDistance = (closestObstruction.Position - callingAI.Position).Length();
            
            for (int i = 1; i < obstructionsList.Count; ++i)
            {
                StaticObject obstruction = obstructionsList.ElementAt(i);
                float distanceToObstruction = (obstruction.Position - callingAI.Position).Length();
                if (distanceToObstruction < closestObstructionDistance)
                {
                    closestObstruction = obstruction;
                    closestObstructionDistance = distanceToObstruction;
                }
            }
            if (closestObstruction is DynamicObject)
                if (isObjectRegistered(closestObstruction as DynamicObject))
                    if (objectPaths[closestObstruction as DynamicObject].currentWaypoint != null)
                        if (objectPaths[closestObstruction as DynamicObject].currentWaypoint.connectedEdges.Count == 0 || closestObstruction is playerObject)
                            return; // that object is dodging already don't make it stop
            //Make the obstruction yield for the next few steps:
            if (closestObstruction is DynamicObject)
                if (!movementYieldList.Keys.Contains(closestObstruction as DynamicObject))
                    movementYieldList.Add(closestObstruction as DynamicObject, YIELD_TICKS);

            //Now dodge it:
            dodgeObject(callingAI, closestObstruction);
        }

        /// <summary>
        /// Method to update the movement of all registered AI characters. The list of waypoints have to be in reverse order (as returned by the A*)
        /// </summary>
        /// <param name="gt">Game time as passed on by the game loop</param>
        public void updateAIMovement(GameTime gt)
        {
            //Clear the yield list at the beginning of the step:
            for (int i = AI_MOVEMENT_SLOT_COUNT * currentSlot; i < AI_MOVEMENT_SLOT_COUNT * (currentSlot + 1) && i < objectPaths.Keys.Count; ++i)
            {
                DynamicObject ai = objectPaths.Keys.ElementAt(i);
                //reset the speed:
                ai.ShipMovementInfo.speed = 0;

                //counts down the path retaining table:
                if (dodgeInactiveCountDown.Keys.Contains(ai))
                    if (dodgeInactiveCountDown[ai]-- <= 0)
                        dodgeInactiveCountDown.Remove(ai);
                //if the object has to yield then do nothing
                if (movementYieldList.Keys.Contains(ai))
                    if (movementYieldList[ai]-- > 0)
                        continue;
                    else
                        movementYieldList.Remove(ai);

                //get the current path and check for obsticles
                PathInformation pathInfo = objectPaths[ai];
                float closeToWaypoint = ai.getMaxSpeed * DISTANCE_TO_WAYPOINT_IN_SECONDS_WHEN_CLOSE;
                float veryCloseToWaypoint = ai.getMaxSpeed * DISTANCE_TO_WAYPOINT_IN_SECONDS_WHEN_VERY_CLOSE;
                if (pathInfo.currentWaypoint != null) //if there is a path
                {
                    float distToWayPoint = (pathInfo.currentWaypoint.Position - ai.Position).Length();
                    //List<StaticObject> obstructionsList = pathIntersectTest(ai);
                    List<StaticObject> nearbyList = this.obstructionsInCloseProximity(ai);
                    avoidCollisions(ai, nearbyList.ToList());

                    //if the object has to yield then do nothing
                    if (movementYieldList.Keys.Contains(ai))
                        if (movementYieldList[ai] > 0)
                            continue;
  
                    //if very close to the next waypoint remove that waypoint so that we can go to the next:
                    if (distToWayPoint <= veryCloseToWaypoint)
                        pathInfo.reachedWaypoint();
                    else// if (nearbyList.Count == 0)
                    {   //We want our ship to slowly rotate towards the direction it has to move in:
                        Vector3 vLookDir = Vector3.Zero, vWantDir = Vector3.Zero;
                        turnAI(ref vWantDir, ref vLookDir, ai, pathInfo.currentWaypoint.Position, gt);
                        //now set the speed:
                        float compLookOntoWant = Vector3.Dot(vLookDir, vWantDir);
                        if (Math.Abs(compLookOntoWant) > 1)
                            compLookOntoWant = 1;
                        ai.ShipMovementInfo.speed = ai.getMaxSpeed *
                            (float)(Math.Pow(TURNING_SPEED_COEF, -Math.Abs(Math.Acos(compLookOntoWant) * 180 / Math.PI)));
                    }
                }
            }
            if ((currentSlot + 1) * AI_MOVEMENT_SLOT_COUNT < objectPaths.Keys.Count)
                currentSlot++;
            else
                currentSlot = 0;
        }
        /// <summary>
        /// Performs a dogfight move against the opponent
        /// </summary>
        /// <param name="ti">Team on which the ai is registered</param>
        /// <param name="ai">Character to perform move</param>
        /// <param name="target">Opponent of the ai</param>
        public void doDogfightMove(TeamInformation ti, DynamicObject ai, StaticObject target)
        {
            if (!this.isObjectRegistered(ai)) return;
            float radiusToGoBehindTarget = (target.getBoundingSphere().Radius + ai.getBoundingSphere().Radius) * RADIUS_MULTIPLIER_TO_GO_BEHIND_TARGET;
            Vector3 wpPosition = target.Position + Vector3.Normalize(Matrix.CreateFromQuaternion(target.rotation).Forward) * radiusToGoBehindTarget;
            Vector3 wpDPosition = target.Position + Vector3.Normalize(Matrix.CreateFromQuaternion(target.rotation).Up) * radiusToGoBehindTarget / RADIUS_FACTOR_TO_GO_ABOVE_TARGET;
            if (Vector3.Dot(Vector3.Normalize(wpPosition - target.Position), Vector3.Normalize(ai.Position - target.Position)) < DOT_ANGLE_TO_STOP_DOGFIGHT_MOVE ||
                (ai.Position - target.Position).Length() > CHASE_WHEN_FURTHER)
            {
                PathInformation fighterPath = objectPaths[ai];
                List<Node> waypointList = fighterPath.remainingPath;

                //we clear the waypoint list and add new waypoints:
                if (waypointList.Count > 0)
                {
                    bool shouldAddTopWaypt = (Vector3.Dot(Vector3.Normalize(Matrix.CreateFromQuaternion(target.rotation).Forward), Vector3.Normalize(target.Position - ai.Position)) > 0);
                    if ((wpPosition - waypointList.ElementAt(0).Position).Length() > TARGET_WP_BUFFER || shouldAddTopWaypt)
                    {
                        waypointList.Clear();
                        if (shouldAddTopWaypt)
                            waypointList.Insert(0, new Node(wpDPosition, -1));
                        waypointList.Insert(0, new Node(wpPosition, -1));
                    }
                }
                else
                {
                    if (Vector3.Dot(Vector3.Normalize(Matrix.CreateFromQuaternion(target.rotation).Forward), Vector3.Normalize(target.Position - ai.Position)) > 0)
                        waypointList.Insert(0, new Node(wpDPosition, -1));
                    waypointList.Insert(0, new Node(wpPosition, -1));
                }
                fighterPath.remainingPath = waypointList;
            }
            else //stop navigation (we are behind the target so we can start shooting it)
                getPath(ai).Clear();
        }
        /// <summary>
        /// Turns the ai to face towards the a waypoint
        /// </summary>
        /// <param name="vWantDir"></param>
        /// <param name="vLookDir"></param>
        /// <param name="objectToTurn"></param>
        /// <param name="currentWaypoint"></param>
        /// <param name="gt"></param>
        internal void turnAI(ref Vector3 vWantDir, ref Vector3 vLookDir, DynamicObject objectToTurn, Vector3 currentWaypoint, GameTime gt)
        {
            //Calculate yaw and pitch for view direction and target direction
            vWantDir = Vector3.Normalize(currentWaypoint - objectToTurn.Position);
            vLookDir = Vector3.Normalize(-Matrix.CreateFromQuaternion(objectToTurn.rotation).Forward);
            float distance = (float)Math.Sqrt(vWantDir.Z * vWantDir.Z + vWantDir.X * vWantDir.X);
            float tpitch = distance == 0 ? (float)Math.Sign(-vWantDir.Y) * (float)Math.PI / 2 : -(float)Math.Atan2(vWantDir.Y, distance);
            float tyaw = (float)Math.Atan2(vWantDir.X, vWantDir.Z);
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

            if (Math.Abs(diffp) > Math.Abs(objectToTurn.getpitchSpeed) * (float)(gt.ElapsedGameTime.TotalSeconds))
                diffp = Math.Sign(diffp) * Math.Abs(objectToTurn.getpitchSpeed) * (float)(gt.ElapsedGameTime.TotalSeconds);
            if (Math.Abs(diffy) > Math.Abs(objectToTurn.getYawSpeed) * (float)(gt.ElapsedGameTime.TotalSeconds))
                diffy = Math.Sign(diffy) * Math.Abs(objectToTurn.getYawSpeed) * (float)(gt.ElapsedGameTime.TotalSeconds);

            //special case: deal with the pitch if its PI/2 or -PI/2, because if its slightly off it causes problems:
            if (Math.Abs(Math.Abs(tpitch) - Math.PI / 2) <= EPSILON_DISTANCE && !(Math.Abs(diffy) <= EPSILON_DISTANCE))
                objectToTurn.rotation = Quaternion.CreateFromYawPitchRoll(tyaw, tpitch, 0);
            else
                objectToTurn.rotation = Quaternion.CreateFromYawPitchRoll(cyaw + diffy, cpitch + diffp, 0);

        }
        /// <summary>
        /// A* path finding algorithm.
        /// </summary>
        /// <param name="start">Start node</param>
        /// <param name="end">End node</param>
        /// <returns>Path to end node if one is found, otherwise null</returns>
        public List<Node> AStar(Node start, Node end, bool addRandomFactor)
        {
            if (start == end || start == null || end == null) return null;
            List<Node> openList = new List<Node>();
            List<Node> visitedList = new List<Node>();
            openList.Add(start);
            start.heuristic = (end.Position - start.Position).Length(); //calculate heuristic
            while (openList.Count > 0)  //while we have more nodes to explore
            {
                Node bestChoice = openList.ElementAt<Node>(0);
                //get the best node choice:
                foreach (Node node in openList)                       
                    if (bestChoice.pathCost + bestChoice.heuristic > node.pathCost + node.heuristic)
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
                    Random randomizer = new Random();
                    double distToNeighbour = bestChoice.pathCost + (addRandomFactor ?
                        neighbourEdge.distance + neighbourEdge.weight + randomizer.NextDouble() * ASTAR_RANDOM_FACTOR * neighbourEdge.distance : neighbourEdge.distance + neighbourEdge.weight);
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
                        neighbour.pathCost = distToNeighbour;
                    }
                }
            }
            return null; //Not found
        }
    }
}
