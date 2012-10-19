using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region "XNA using statements"
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
#endregion

/**
 * Author: Nathan Floor(FLRNAT001)
 * 
 * Interface for game objects to make the objects compatable with the grid spatial structure.
 * One instance variable is needed for full functionality.
 * 
 * Just add the following to your list of instance variables:
 * private List<Vector3> locations = new List<Vector3>();
 * 
 */

namespace BBN_Game.Grid
{
    interface GridObjectInterface
    {
        void setNewLocation(Vector3 newPosition);//update the object's list of grid locations
        int getCapacity();//return the size of the list of grid locations cotaining the object
        Vector3 getLocation(int index);//return the location vector for specified index
        void removeAllLocations();//clear the list of grid locations containing the object

        //can be found in Brandon's StaticObject class
        BoundingSphere getBoundingSphere();
        Vector3 Position
        {
            get;
            set;
        }
    }
}
