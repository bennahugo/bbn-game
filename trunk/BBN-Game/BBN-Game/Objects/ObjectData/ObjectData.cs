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
        #region "Globals"
        public float pitch;
        public float yaw;
        public float roll;
        public float speed;
        public float scale;
        public Vector3 position;
        #endregion

        /// <summary>
        /// Resets the variables
        /// </summary>
        public ObjectData()
        {
            reset();
        }

        /// <summary>
        /// Sets the variables with the input
        /// </summary>
        /// <param name="Pos">Position</param>
        /// <param name="pitch">Pitch</param>
        /// <param name="yaw">Yaw</param>
        /// <param name="roll">Roll</param>
        /// <param name="scale">Scale</param>
        public ObjectData(Vector3 Pos, float pitch, float yaw, float roll, float scale)
        {
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
            this.position = Pos;
            this.scale = scale;
        }

        /// <summary>
        /// resets variables to defaults
        /// </summary>
        public void reset()
        {
            // reset
            pitch = yaw = roll = speed = 0.0f;
            scale = 1;
            position = Vector3.Zero;
        }

        /// <summary>
        /// Zeroes all variables needed
        /// </summary>
        public void resetAngles()
        {
            pitch = yaw = roll = 0.0f;
        }
    }
}
