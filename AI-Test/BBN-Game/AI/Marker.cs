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
    /// Basic map marker. Markers should not be visible on the map during game play and serve only as
    /// holders of position data.
    /// </summary>
    class Marker
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
    }
}
