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
/// A Base class for Objects that move in the game
////

namespace BBN_Game.Objects
{
    class DynamicObject : StaticObject
    {
        #region "Globals"
        /// <summary>
        /// Extra variables that the static class did not require
        /// </summary>
        protected float maxSpeed, minSpeed;
        protected Vector3 prevPos;
        public Vector3 bumpVelocity = Vector3.Zero;
        /// <summary>
        /// Getters and setters
        /// </summary>
        public float getMaxSpeed
        {
            get { return maxSpeed; }
        }
        public float getMinSpeed
        {
            get { return minSpeed; }
        }
        public float getMass
        {
            get { return mass; }
        }
        public Vector3 getPreviousPosition
        {
            get { return prevPos; }
        }
        #endregion

        #region "Constructors"
        /// <summary>
        /// Constructor
        /// This adds the setData() method to the default constructor
        /// </summary>
        /// <param name="game">The Game</param>
        public DynamicObject (Game game, Team team, Vector3 position) : base(game, team, position)
        {
            prevPos = position;
        }

        /// <summary>
        /// Sets the data for the object (these are defaults)
        /// </summary>
        protected override void setData()
        {
            this.maxSpeed = 50;
            this.minSpeed = -10;
            this.mass = 1000;
            base.setData();
        }
        #endregion

        #region "Update methods"
        /// <summary>
        /// Update method overide
        /// This adds the Controller method to the list of duties
        /// </summary>
        /// <param name="gt">Game time</param>
        public override void Update(GameTime gt)
        {
            controller(gt);

            base.Update(gt);
        }

        /// <summary>
        /// A virtual Controller method
        /// This will be used to call on AI and player controls
        /// </summary>
        /// <param name="gt"></param>
        public virtual void controller(GameTime gt)
        {
            // check speeds
            if (!(shipData.speed == 0))
            {
                if (shipData.speed < minSpeed)
                    shipData.speed = minSpeed;

                if (shipData.speed > maxSpeed)
                    shipData.speed = maxSpeed;
            }
        }

        /// <summary>
        /// Sets the World Matrix for the object
        /// </summary>
        /// <param name="time">The Game time</param>
        /// <param name="m">The matrix rotation that has occured</param>
        public override void  setWorldMatrix(float time, Matrix m)
        {
            prevPos = Position;
            Position -= m.Forward * shipData.speed * time + bumpVelocity;
            bumpVelocity = bumpVelocity * 0.85f;
            base.setWorldMatrix(time, m);
        }
        #endregion
    }
}
