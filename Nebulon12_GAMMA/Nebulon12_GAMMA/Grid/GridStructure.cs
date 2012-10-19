using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region "XNA using statements"
using System.Runtime.InteropServices; //for messageboxes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
#endregion

/**
 * Author: Nathan Floor
 * 
 * This class represents a 3 dimensional grid structure to be used for spatial lookups.
 * This class provides the functionality to perform spatial lookups for any given object,
 * as well as for any given point in space.
 * 
 */

namespace BBN_Game.Grid
{
    class GridStructure
    {
        //instance variables
        private List<GridObjectInterface>[, ,] grid = null;
        private int GRID_BLOCK_SIZE = 64; //max grid block size
        private int grid_offset = 10; //because space is centered at (0,0,0)
        private int gridLength;

        //constructor
        public GridStructure(int cubeLength, int max_size)
        {
            GRID_BLOCK_SIZE = max_size;
            grid_offset = (cubeLength / GRID_BLOCK_SIZE) / 2;
            gridLength = (cubeLength / GRID_BLOCK_SIZE) + 1;
            grid = new List<GridObjectInterface>[gridLength, gridLength, gridLength];
            
            //initialise grid structure
            for (int x = 0; x < grid.GetLength(0); x++)
                for (int y = 0; y < grid.GetLength(1); y++)
                    for (int z = 0; z < grid.GetLength(2); z++)
                        grid[x, y, z] = new List<GridObjectInterface>();
        }

        //return all the objects in-front of the player for targetting
        public List<GridObjectInterface> getTargets(int distance, Microsoft.Xna.Framework.Matrix rotation, GridObjectInterface player)
        {
            List<GridObjectInterface> targets = new List<GridObjectInterface>();
            PowerDataStructures.PriorityQueue<Double,GridObjectInterface> queue = new PowerDataStructures.PriorityQueue<Double,GridObjectInterface>(true);

            Vector3[] boxPoints = new Vector3[7];
            //we want to take the length of the entire ship (if it is at the edge of a cell) into account (add the tip and the tail to the list):
            Vector3 frontPoint = Vector3.Normalize(rotation.Forward) * player.getBoundingSphere().Radius + player.Position;
            Vector3 backPoint = Vector3.Normalize(rotation.Backward) * player.getBoundingSphere().Radius + player.Position;
            //now compute the left and right wing tip points:
            Vector3 leftPoint = Vector3.Normalize(rotation.Left) * player.getBoundingSphere().Radius + player.Position;
            Vector3 rightPoint = Vector3.Normalize(rotation.Right) * player.getBoundingSphere().Radius + player.Position;
            //also compute the tail fin and the belly points
            Vector3 topPoint = Vector3.Normalize(rotation.Up) * player.getBoundingSphere().Radius + player.Position;
            Vector3 bottomPoint = Vector3.Normalize(rotation.Down) * player.getBoundingSphere().Radius + player.Position;
            
            //convert the front and back points to grid coords:
            Vector3 objPosFront = new Vector3((int)Math.Round((double)((frontPoint.X) / GRID_BLOCK_SIZE)) + grid_offset,
            (int)Math.Round((double)((frontPoint.Y) / GRID_BLOCK_SIZE)) + grid_offset,
            (int)Math.Round((double)((frontPoint.Z) / GRID_BLOCK_SIZE)) + grid_offset); //players own position
            Vector3 objPosBack = new Vector3((int)Math.Round((double)((backPoint.X) / GRID_BLOCK_SIZE)) + grid_offset,
            (int)Math.Round((double)((backPoint.Y) / GRID_BLOCK_SIZE)) + grid_offset,
            (int)Math.Round((double)((backPoint.Z) / GRID_BLOCK_SIZE)) + grid_offset); //players own position
            
            //Construct the radar area (front, left and right, above and below):
            //now add the front and the back points to the list
            boxPoints[0] = objPosFront;
            boxPoints[1] = objPosBack;
            //add a distance to the left and right wingtips, as well as the tail and belly points
            boxPoints[2] = gridPtAlongDirection(rotation.Backward,backPoint, distance);
            boxPoints[3] = gridPtAlongDirection(rotation.Left, leftPoint, distance);
            boxPoints[4] = gridPtAlongDirection(rotation.Right, rightPoint, distance);
            boxPoints[5] = gridPtAlongDirection(rotation.Up, topPoint, distance);
            boxPoints[6] = gridPtAlongDirection(rotation.Down, bottomPoint, distance);
            //Now compute lower and upper bounds
            Vector3 lb = Vector3.Zero;
            Vector3 ub = Vector3.Zero;
            getMinimumsAndMaximums(boxPoints, out lb, out ub);
            //System.Diagnostics.Debug.WriteLine(lb + "  ,  " + ub);
            lb = clampCellCoordToGridLimits(lb);
            ub = clampCellCoordToGridLimits(ub);
            //Now get all the objects in front of the player:
            for (int x = (int)lb.X; x <= (int)ub.X; ++x)
                for (int y = (int)lb.Y; y <= (int)ub.Y; ++y)
                    for (int z = (int)lb.Z; z <= (int)ub.Z; ++z)
                        for (int i = 0; i < grid[x, y, z].Count; i++)   
                        {
                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);
                            foreach(KeyValuePair<double,GridObjectInterface> value in queue)
                                if (value.Value == tempObj)
                                    goto dontAdd;
                            //check if object is within player's view cone +- 45 degrees
                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Backward), Vector3.Normalize(tempObj.Position - player.Position));
                            if (theta >= 0.707)
                            {
                                double dist = (tempObj.Position - player.Position).Length();
                                KeyValuePair<double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                queue.Add(temp);
                            }
                        dontAdd: { /*DO NOTHING IF THE ITEM IS ALREADY ON THE QUEUE*/ }
                        }
            #region old-code
            /*
            //convert real-world coords to grid coords
            int gridX_pos = (int)Math.Round((double)((playerPosition.X) / GRID_BLOCK_SIZE)) + grid_offset;//for player position
            int gridY_pos = (int)Math.Round((double)((playerPosition.Y) / GRID_BLOCK_SIZE)) + grid_offset;
            int gridZ_pos = (int)Math.Round((double)((playerPosition.Z) / GRID_BLOCK_SIZE)) + grid_offset;

            int gridX_forward = (int)Math.Round((double)((rotation.Forward.X*distance) / GRID_BLOCK_SIZE)) + grid_offset;//forward position
            int gridY_forward = (int)Math.Round((double)((rotation.Forward.Y*distance) / GRID_BLOCK_SIZE)) + grid_offset;
            int gridZ_forward = (int)Math.Round((double)((rotation.Forward.Z * distance) / GRID_BLOCK_SIZE)) + grid_offset;*/

            //loop through blocks in-front of player            
            
            /*
            #region along x-axis
            if (gridX_pos < gridX_forward)
            {
                for (int x = gridX_pos; x < gridX_forward; x++)
                {
                    #region along y-axis
                    if (gridY_pos < gridY_forward)
                    {
                        for (int y = gridY_pos; y < gridY_forward; y++)
                        {
                            #region along z-axis
                            if (gridZ_pos < gridZ_forward)
                            {
                                for (int z = gridZ_pos; z < gridZ_forward; z++)
                                {
                                    if (grid[x, y, z].Count > 0)
                                    {
                                        for (int i = 0; i < grid[x, y, z].Count; i++)
                                        {                                            
                                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);

                                            //check if object is within player's view cone +- 45 degrees
                                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Forward), Vector3.Normalize(tempObj.Position-playerPosition));
                                            //if (theta >= 0.75)
                                            {
                                                double dist = (tempObj.Position - playerPosition).Length();
                                                KeyValuePair<double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                                queue.Add(temp);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int z = gridZ_pos; z < gridZ_forward; z--)
                                {
                                    if (grid[x, y, z].Count > 0)
                                    {
                                        for (int i = 0; i < grid[x, y, z].Count; i++)
                                        {
                                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);
                                            //check if object is within player's view cone +- 45 degrees
                                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Forward), Vector3.Normalize(tempObj.Position-playerPosition));
                                            //if (theta >= 0.75)
                                            {
                                                double dist = (tempObj.Position - playerPosition).Length();
                                                KeyValuePair<Double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                                queue.Add(temp);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        for (int y = gridY_pos; y < gridY_forward; y--)
                        {
                            #region along z-axis
                            if (gridZ_pos < gridZ_forward)
                            {
                                for (int z = gridZ_pos; z < gridZ_forward; z++)
                                {
                                    if (grid[x, y, z].Count > 0)
                                    {
                                        for (int i = 0; i < grid[x, y, z].Count; i++)
                                        {
                                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);
                                            //check if object is within player's view cone +- 45 degrees
                                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Forward), Vector3.Normalize(tempObj.Position-playerPosition));
                                            //if (theta >= 0.75)
                                            {
                                                double dist = (tempObj.Position - playerPosition).Length();
                                                KeyValuePair<double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                                queue.Add(temp);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int z = gridZ_pos; z < gridZ_forward; z--)
                                {
                                    if (grid[x, y, z].Count > 0)
                                    {
                                        for (int i = 0; i < grid[x, y, z].Count; i++)
                                        {
                                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);
                                            //check if object is within player's view cone +- 45 degrees
                                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Forward), Vector3.Normalize(tempObj.Position-playerPosition));
                                            //if (theta >= 0.75)
                                            {
                                                double dist = (tempObj.Position - playerPosition).Length();
                                                KeyValuePair<Double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                                queue.Add(temp);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
            else
            {
                for (int x = gridX_pos; x < gridX_forward; x--)
                {
                    #region along y-axis
                    if (gridY_pos < gridY_forward)
                    {
                        for (int y = gridY_pos; y < gridY_forward; y++)
                        {
                            #region along z-axis
                            if (gridZ_pos < gridZ_forward)
                            {
                                for (int z = gridZ_pos; z < gridZ_forward; z++)
                                {
                                    if (grid[x, y, z].Count > 0)
                                    {
                                        for (int i = 0; i < grid[x, y, z].Count; i++)
                                        {
                                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);
                                            //check if object is within player's view cone +- 45 degrees
                                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Forward), Vector3.Normalize(tempObj.Position-playerPosition));
                                            //if (theta >= 0.75)
                                            {
                                                double dist = (tempObj.Position - playerPosition).Length();
                                                KeyValuePair<double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                                queue.Add(temp);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int z = gridZ_pos; z < gridZ_forward; z--)
                                {
                                    if (grid[x, y, z].Count > 0)
                                    {
                                        for (int i = 0; i < grid[x, y, z].Count; i++)
                                        {
                                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);
                                            //check if object is within player's view cone +- 45 degrees
                                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Forward), Vector3.Normalize(tempObj.Position-playerPosition));
                                            //if (theta >= 0.75)
                                            {
                                                double dist = (tempObj.Position - playerPosition).Length();
                                                KeyValuePair<Double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                                queue.Add(temp);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    else
                    {
                        for (int y = gridY_pos; y < gridY_forward; y--)
                        {
                            #region along z-axis
                            if (gridZ_pos < gridZ_forward)
                            {
                                for (int z = gridZ_pos; z < gridZ_forward; z++)
                                {
                                    if (grid[x, y, z].Count > 0)
                                    {
                                        for (int i = 0; i < grid[x, y, z].Count; i++)
                                        {
                                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);
                                            //check if object is within player's view cone +- 45 degrees
                                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Forward), Vector3.Normalize(tempObj.Position-playerPosition));
                                            //if (theta >= 0.75)
                                            {
                                                double dist = (tempObj.Position - playerPosition).Length();
                                                KeyValuePair<double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                                queue.Add(temp);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int z = gridZ_pos; z < gridZ_forward; z--)
                                {
                                    if (grid[x, y, z].Count > 0)
                                    {
                                        for (int i = 0; i < grid[x, y, z].Count; i++)
                                        {
                                            GridObjectInterface tempObj = grid[x, y, z].ElementAt(i);
                                            //check if object is within player's view cone +- 45 degrees
                                            float theta = Vector3.Dot(Vector3.Normalize(rotation.Forward), Vector3.Normalize(tempObj.Position-playerPosition));
                                            //if (theta >= 0.75)
                                            {
                                                double dist = (tempObj.Position - playerPosition).Length();
                                                KeyValuePair<Double, GridObjectInterface> temp = new KeyValuePair<double, GridObjectInterface>(dist, tempObj);
                                                queue.Add(temp);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
            #endregion
            */
            #endregion
            for (int i = 0; i < queue.Count; ++i)
            {
                targets.Add(queue.ElementAt(i).Value);
            }

            //return queue.Count;

            return targets;
        }
        //Get the bounding box minimums and maximums
        private void getMinimumsAndMaximums(Vector3[] pointList, out Vector3 minimums, out Vector3 maximums)
        {
            minimums = Vector3.Zero;
            maximums = Vector3.Zero;
            minimums.X = pointList[0].X;
            minimums.Y = pointList[0].Y;
            minimums.Z = pointList[0].Z;
            maximums.X = pointList[0].X;
            maximums.Y = pointList[0].Y;
            maximums.Z = pointList[0].Z;
            for (int i = 1; i < pointList.Length; ++i)
            {
                minimums.X = Math.Min(minimums.X, pointList[i].X);
                minimums.Y = Math.Min(minimums.Y, pointList[i].Y);
                minimums.Z = Math.Min(minimums.Z, pointList[i].Z);
                maximums.X = Math.Max(maximums.X, pointList[i].X);
                maximums.Y = Math.Max(maximums.Y, pointList[i].Y);
                maximums.Z = Math.Max(maximums.Z, pointList[i].Z);
            }
        }
        //Clamps a cell coordinate to the bounds of the grid
        private Vector3 clampCellCoordToGridLimits(Vector3 cellCoord)
        {
            Vector3 result = cellCoord;
            result.X = ((int)cellCoord.X < 0 ? 0 : ((int)cellCoord.X >= gridLength ? gridLength - 1 : (int)cellCoord.X));
            result.Y = ((int)cellCoord.Y < 0 ? 0 : ((int)cellCoord.Y >= gridLength ? gridLength - 1 : (int)cellCoord.Y));
            result.Z = ((int)cellCoord.Z < 0 ? 0 : ((int)cellCoord.Z >= gridLength ? gridLength - 1 : (int)cellCoord.Z));
            return result;
        }
        //Calculate the grid coordinates of a point along a ray (using the line equation p' = p + d*t, where d is the 
        //direction of the ray and p is the starting point
        private Vector3 gridPtAlongDirection(Vector3 direction, Vector3 startpos, float distance)
        {
            Vector3 normDirection = Vector3.Normalize(direction);
            return new Vector3((int)Math.Round((double)((normDirection.X * distance + startpos.X) / GRID_BLOCK_SIZE)) + grid_offset,
                (int)Math.Round((double)((normDirection.Y * distance + startpos.Y) / GRID_BLOCK_SIZE)) + grid_offset,
                (int)Math.Round((double)((normDirection.Z * distance + startpos.Z) / GRID_BLOCK_SIZE)) + grid_offset);
        }
        //insert object into grid and update pointers to grid-blocks
        public void registerObject(GridObjectInterface obj)
        {
            Boolean hasMoved = false;
            //get the width/diameter of object in terms of grid blocks
            int objectWidth = (int)Math.Ceiling(((obj.getBoundingSphere().Radius * 2) / GRID_BLOCK_SIZE));

            Vector3 oldPos = new Vector3();
            if(obj is Objects.DynamicObject)
                oldPos = ((Objects.DynamicObject)obj).getPreviousPosition;

            //convert position coords to grid coords
            int objX_prev = (int)Math.Round((double)((oldPos.X) / GRID_BLOCK_SIZE)) + grid_offset;
            int objY_prev = (int)Math.Round((double)((oldPos.Y) / GRID_BLOCK_SIZE)) + grid_offset;
            int objZ_prev = (int)Math.Round((double)((oldPos.Z) / GRID_BLOCK_SIZE)) + grid_offset;
            //get object's present coords
            int texX, texY, texZ;
            texX = (int)obj.Position.X;
            texY = (int)obj.Position.Y;
            texZ = (int)obj.Position.Z;
            //convert position coords to grid coords
            int objX = (int)Math.Round((double)(texX / GRID_BLOCK_SIZE)) + grid_offset;
            int objY = (int)Math.Round((double)(texY / GRID_BLOCK_SIZE)) + grid_offset;
            int objZ = (int)Math.Round((double)(texZ / GRID_BLOCK_SIZE)) + grid_offset;

            //check if object has moved out of grid block
            if (obj.getCapacity() == 0)
                hasMoved = true;
            else if (((objX + objectWidth / 2) < (objX_prev + objectWidth / 2)) || ((objX - objectWidth / 2) > (objX_prev - objectWidth / 2)))
            {
                hasMoved = false;
            }
            else if (((objY + objectWidth / 2) < (objY_prev + objectWidth / 2)) || ((objY + objectWidth / 2) > (objY_prev + objectWidth / 2)))
            {
                hasMoved = false;
            }
            else if (((objZ + objectWidth / 2) < (objZ_prev + objectWidth / 2)) || ((objZ + objectWidth / 2) > (objZ_prev + objectWidth / 2)))
            {
                hasMoved = false;
            }
            else
                hasMoved = true;

            hasMoved = true;
            if (hasMoved)
            {
                //remove object from grid first, if its already registered
                deregisterObject(obj);
                
                //register object in grid blocks
                for (int x = 0; x < objectWidth; x++)
                    for (int y = 0; y < objectWidth; y++)
                        for (int z = 0; z < objectWidth; z++)
                        {
                            //convert objects coords to grid coords
                            objX = (int)Math.Round((double)((texX - x * GRID_BLOCK_SIZE) / GRID_BLOCK_SIZE)) + grid_offset;
                            objY = (int)Math.Round((double)((texY - y * GRID_BLOCK_SIZE) / GRID_BLOCK_SIZE)) + grid_offset;
                            objZ = (int)Math.Round((double)((texZ - z * GRID_BLOCK_SIZE) / GRID_BLOCK_SIZE)) + grid_offset;

                            //check that the object is still within the confines of the grid
                            if ((objX >= 0) && (objX < grid.GetLength(0)) && (objY >= 0) && (objY < grid.GetLength(1)) && (objZ >= 0) && (objZ < grid.GetLength(2)))
                            {
                                grid[objX, objY, objZ].Add(obj);
                                obj.setNewLocation(new Vector3(objX, objY, objZ));
                            }
                        }
            }
        }

        //clear pointers to grid and remove object from grid
        public void deregisterObject(GridObjectInterface obj)
        {
            for (int i = 0; i < obj.getCapacity(); i++)
            {
                Vector3 gridBlock = obj.getLocation(i);
                grid[(int)Math.Round(gridBlock.X), (int)Math.Round(gridBlock.Y), (int)Math.Round(gridBlock.Z)].Remove(obj);
            }
            obj.removeAllLocations();
        }

        //find list of objects near/adjacent to current object
        public List<GridObjectInterface> checkNeighbouringBlocks(GridObjectInterface obj)
        {
            List<GridObjectInterface> neighbours = new List<GridObjectInterface>();
            Vector3 gridBlock;
            int gridX, gridY, gridZ;

            //loop though adjacent blocks containing objects to test for potential collisions
            for (int i = 0; i < obj.getCapacity(); i++)
            {
                gridBlock = obj.getLocation(i);
                gridX = (int)(gridBlock.X);
                gridY = (int)(gridBlock.Y);
                gridZ = (int)(gridBlock.Z);

                //check all 8 blocks surrounding object (as well as block object is in) for nearby objects
                for (int x = -1; x < 2; x++)
                    for (int y = -1; y < 2; y++)
                        for (int z = -1; z < 2; z++)
                            checkForDuplicates(neighbours, obj, gridX + x, gridY + y, gridZ + z);
            }
            return neighbours;
        }

        //find list of objects near/adjacent to current point in space
        public List<GridObjectInterface> checkNeighbouringBlocks(Vector3 pointInSpace)
        {
            List<GridObjectInterface> neighbours = new List<GridObjectInterface>();
            int gridX, gridY, gridZ;

            //convert objects coords to grid coords
            gridX = (int)Math.Round((double)(Math.Round(pointInSpace.X) / GRID_BLOCK_SIZE)) + grid_offset;
            gridY = (int)Math.Round((double)(Math.Round(pointInSpace.Y) / GRID_BLOCK_SIZE)) + grid_offset;
            gridZ = (int)Math.Round((double)(Math.Round(pointInSpace.Z) / GRID_BLOCK_SIZE)) + grid_offset;

            //check all 8 blocks surrounding object (as well as block object is in) for nearby objects
            for (int x = -1; x < 2; x++)
                for (int y = -1; y < 2; y++)
                    for (int z = -1; z < 2; z++)
                        checkForDuplicates(neighbours, gridX + x, gridY + y, gridZ + z);

            return neighbours;
        }

        //find list of objects within supplied radius to current point in space NEW
        public List<GridObjectInterface> checkWithInRadius(Vector3 pointInSpace, int radius)
        {
            List<GridObjectInterface> neighbours = new List<GridObjectInterface>();
            int gridX, gridY, gridZ;

            //convert objects coords to grid coords
            gridX = (int)Math.Round((double)(Math.Round(pointInSpace.X) / GRID_BLOCK_SIZE)) + grid_offset;
            gridY = (int)Math.Round((double)(Math.Round(pointInSpace.Y) / GRID_BLOCK_SIZE)) + grid_offset;
            gridZ = (int)Math.Round((double)(Math.Round(pointInSpace.Z) / GRID_BLOCK_SIZE)) + grid_offset;

            //check all 8 blocks surrounding object (as well as block object is in) for nearby objects
            int dist = (int)Math.Ceiling((double)radius / GRID_BLOCK_SIZE);
            for (int x = (gridX - dist); x < (gridX + dist); x++)
                for (int y = (gridY - dist); y < (gridY + dist); y++)
                    for (int z = (gridZ - dist); z < (gridZ + dist); z++)
                        checkForDuplicates(neighbours, gridX + x, gridY + y, gridZ + z);

            return neighbours;
        }

        //check if supplied object is within the grid NEW
        public bool isInGrid(GridObjectInterface obj)
        {
            int blockX = (int)Math.Round(obj.Position.X);
            int blockY = (int)Math.Round(obj.Position.Y);
            int blockZ = (int)Math.Round(obj.Position.Z);

            //convert objects coords to grid coords
            int gridX = (int)Math.Round((double)(blockX / GRID_BLOCK_SIZE)) + grid_offset;
            int gridY = (int)Math.Round((double)(blockY / GRID_BLOCK_SIZE)) + grid_offset;
            int gridZ = (int)Math.Round((double)(blockZ / GRID_BLOCK_SIZE)) + grid_offset;

            if ((gridX < 0) && (gridX >= grid.GetLength(0)) && (gridY < 0) && (gridY >= grid.GetLength(1)) && (gridZ < 0) && (gridZ >= grid.GetLength(2)))
                return false;
            else
                return true;
        }


        //check list of neighbours to current object for duplicate entries
        private void checkForDuplicates(List<GridObjectInterface> nearByObjs, GridObjectInterface obj, int xcoord, int ycoord, int zcoord)
        {
            if ((xcoord >= 0) && (xcoord < grid.GetLength(0)) && (ycoord >= 0) && (ycoord < grid.GetLength(1)) && (zcoord >= 0) && (zcoord < grid.GetLength(2)))
                for (int j = 0; j < grid[xcoord, ycoord, zcoord].Count; j++)
                    if (!nearByObjs.Contains(grid[xcoord, ycoord, zcoord].ElementAt(j)) && (obj != grid[xcoord, ycoord, zcoord].ElementAt(j)))
                        nearByObjs.Add(grid[xcoord, ycoord, zcoord].ElementAt(j));
        }

        //check list of neighbours to point in space for duplicate entries
        private void checkForDuplicates(List<GridObjectInterface> nearByObjs, int xcoord, int ycoord, int zcoord)
        {
            if ((xcoord >= 0) && (xcoord < grid.GetLength(0)) && (ycoord >= 0) && (ycoord < grid.GetLength(1)) && (zcoord >= 0) && (zcoord < grid.GetLength(2)))
                for (int j = 0; j < grid[xcoord, ycoord, zcoord].Count; j++)
                    if (!nearByObjs.Contains(grid[xcoord, ycoord, zcoord].ElementAt(j)))
                        nearByObjs.Add(grid[xcoord, ycoord, zcoord].ElementAt(j));
        }
    }
}
