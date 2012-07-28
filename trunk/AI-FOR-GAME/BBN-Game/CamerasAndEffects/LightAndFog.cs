using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

/////
///
/// Author - Brandon James Talbot
/// 
/// This class contains Varibles for fog and lighting
/// 
////

namespace BBN_Game.CamerasAndEffects
{
    class LightAndFog
    {
        
        public Boolean Fog { get; set; }
        public Vector3 FogCol { get; set; }
        public Vector3 fogViewDistance { get; set; }
    }
}
