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
        //for error message boxs, ie debugging
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern uint MessageBox(IntPtr hWnd,String text,String caption,uint type);

        //instance variables
        private List<GridObjectInterface>[, ,] grid = null;
        private int GRID_BLOCK_SIZE = 64; //max grid block size
        private int grid_offset = 10; //because space is centered at (0,0,0)

        //constructor
        public GridStructure(int height,int width,int depth,int max_size)
        {
            GRID_BLOCK_SIZE = max_size;
            grid_offset = (height / GRID_BLOCK_SIZE) / 2;
            grid = new List<GridObjectInterface>[(height/GRID_BLOCK_SIZE) + 1, (width/GRID_BLOCK_SIZE) + 1, (depth/GRID_BLOCK_SIZE)+1];

            //initialise grid structure
            for (int x = 0; x < grid.GetLength(0); x++)
                for (int y = 0; y < grid.GetLength(1); y++)
                    for (int z = 0; z < grid.GetLength(2); z++)
                        grid[x, y, z] = new List<GridObjectInterface>();
        }

        //for debugging
        //public void displayGridContents()
        //{            
        //    List<GridObjectInterface> temp = new List<GridObjectInterface>();
        //    for (int x = 0; x < grid.GetLength(0); x++)
        //        for (int y = 0; y < grid.GetLength(1); y++)
        //            for (int z = 0; z < grid.GetLength(2); z++)
        //            {
        //                //Console.WriteLine(grid[x, y, z].Count);
        //                if (grid[x, y, z].Count > 0)
        //                    for (int i = 0; i < grid[x, y, z].Count; i++)
        //                        if (temp.Contains(grid[x, y, z][i]) != true)
        //                            temp.Add(grid[x, y, z][i]);
        //            }
        //    String errorMsg = "";
        //    for (int i = 0; i < temp.Count; i++)
        //        errorMsg = errorMsg + temp[i].Position + " \n";
        //    MessageBox(new IntPtr(0), errorMsg, "Contents of Grid:", 0);
        //}
          
        //insert object into grid and update pointers to grid-blocks
        public void registerObject(GridObjectInterface obj)
        {
            //remove object from grid first, if its already registered
            deregisterObject(obj);

            //get the width/diameter of object in terms of grid blocks
            int objectWidth = (int)Math.Ceiling(((obj.getBoundingSphere().Radius*2) / GRID_BLOCK_SIZE));
            int texX, texY, texZ;
            int objX, objY, objZ;

            for (int x = 0; x < objectWidth; x++)
                for (int y = 0; y < objectWidth; y++)
                    for (int z = 0; z < objectWidth; z++)
                    {
                        texX = (int)obj.Position.X;
                        texY = (int)obj.Position.Y;
                        texZ = (int)obj.Position.Z;

                        //convert objects coords to grid coords
                        objX = (int)Math.Round((double)((texX - x * GRID_BLOCK_SIZE) / GRID_BLOCK_SIZE)) + grid_offset;
                        objY = (int)Math.Round((double)((texY - y * GRID_BLOCK_SIZE) / GRID_BLOCK_SIZE)) + grid_offset;
                        objZ = (int)Math.Round((double)((texZ - z * GRID_BLOCK_SIZE) / GRID_BLOCK_SIZE)) + grid_offset;

                        //check that the object is still within the confines of the grid
                        if((objX >=0) && (objX < grid.GetLength(0)) && (objY >= 0) && (objY < grid.GetLength(1)) && (objZ >= 0) && (objZ < grid.GetLength(2)))
                        {
                            grid[objX, objY, objZ].Add(obj);
                            obj.setNewLocation(new Vector3(objX,objY,objZ));
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
            int blockX, blockY, blockZ;
            int gridX, gridY, gridZ;

            //loop though adjacent blocks containing objects to test for potential collisions
            for (int i = 0; i < obj.getCapacity(); i++)
            {
                gridBlock = obj.getLocation(i);
                blockX = (int)Math.Round(gridBlock.X);
                blockY = (int)Math.Round(gridBlock.Y);
                blockZ = (int)Math.Round(gridBlock.Z);

                //convert objects coords to grid coords
                gridX = (int)Math.Round((double)(blockX / GRID_BLOCK_SIZE)) + grid_offset;
                gridY = (int)Math.Round((double)(blockY / GRID_BLOCK_SIZE)) + grid_offset;
                gridZ = (int)Math.Round((double)(blockZ / GRID_BLOCK_SIZE)) + grid_offset;
                
                //check all 8 blocks surrounding object (as well as block object is in) for nearby objects
                for (int x = -1; x < 2;x++)
                    for(int y=-1;y<2;y++)
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
