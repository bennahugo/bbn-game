using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BBN_Game.AI
{
    class SpawnPoint:Marker
    {
        /// <summary>
        /// Constructors to accomodate inheritance from Marker class
        /// </summary>
        public SpawnPoint():
            base() { }
        public SpawnPoint(Vector3 pos, int OwningTeam) :
            base(pos,OwningTeam) { }
    }
}
