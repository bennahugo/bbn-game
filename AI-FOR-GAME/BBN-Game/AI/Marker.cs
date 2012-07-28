using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using BBN_Game.Grid;
namespace BBN_Game.AI
{
    /// <summary>
    /// Basic map marker. Markers should not be visible on the map during game play and serve only as
    /// holders of position data.
    /// </summary>
    class Marker:GridObjectInterface
    {
        public Vector3 Position { get; set; }
        public String id { get; set; }
        public String className { get; set; }
        public String type { get; set; }
        /// <summary>
        /// OwningTeam must be -1 or the team number
        /// </summary>
        public int OwningTeam { get; set; }
        /// <summary>
        /// Construtors
        /// </summary>
        public Marker()
        {
            Position = Vector3.Zero;
            OwningTeam = -1;
        }
        public Marker(Vector3 vPosition, int OwningTeam)
        {
            Position = vPosition;
            this.OwningTeam = OwningTeam;
        }
        #region GridObjectInterface Members
        private List<Vector3> locations = new List<Vector3>();
        void GridObjectInterface.setNewLocation(Vector3 newPosition)
        {
            locations.Add(newPosition);
        }

        int GridObjectInterface.getCapacity()
        {
            if (locations == null)
                return 0;
            else
                return locations.Count();
        }

        Vector3 GridObjectInterface.getLocation(int index)
        {
            if (locations != null)
                return locations[index];
            else
                return Vector3.Zero;
        }

        void GridObjectInterface.removeAllLocations()
        {
            if (locations != null)
                locations.Clear();
        }

        BoundingSphere GridObjectInterface.getBoundingSphere()
        {
            return new BoundingSphere(this.Position, 0.00001f);
        }

        Vector3 GridObjectInterface.Position
        {
            get
            {
                return Position;
            }
            set
            {
                Position = value;
            }
        }

        #endregion
    }
}
