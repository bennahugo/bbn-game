using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region "XNA Using Statements"
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion

/////
///
/// Author - Brandon James Talbot
/// 
/// This class contains Varibles that the Game Objects require for rotation etc
////

namespace BBN_Game.Objects.ObjectData
{
    class ObjectData
    {
        public float pitch;
        public float yaw;
        public float roll;
        public float speed;
        public float scale;
        public Vector3 position;

        public ObjectData()
        {
            reset();
        }


        public ObjectData(Vector3 Pos, float pitch, float yaw, float roll, float scale)
        {
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
            this.position = Pos;
            this.scale = scale;
        }

        public void reset()
        {
            pitch = yaw = roll = speed = 0.0f;
            scale = 1;
            position = Vector3.Zero;
        }

        public void resetAngles()
        {
            pitch = yaw = roll = 0.0f;
        }
    }
}
