using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/////
///
/// Author - Benjamin Hugo
/// 
/// This class is used to store Triangle data (used in the collision detection)
////

namespace BBN_Game.Collision_Detection
{
    /// <summary>
    /// Basic triangle ADT
    /// </summary>
    public struct Triangle
    {
        public Vector3 v1;
        public Vector3 v2;
        public Vector3 v3;
    }
}
