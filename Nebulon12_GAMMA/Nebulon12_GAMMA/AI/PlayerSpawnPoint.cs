using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BBN_Game.AI
{
    class PlayerSpawnPoint:SpawnPoint
    {
        /// <summary>
        /// Constructors to accomodate inheritance from Marker class
        /// </summary>
        public PlayerSpawnPoint():
            base() { }
        public PlayerSpawnPoint(Vector3 pos, int OwningTeam) :
            base(pos,OwningTeam) { }
    }
}
