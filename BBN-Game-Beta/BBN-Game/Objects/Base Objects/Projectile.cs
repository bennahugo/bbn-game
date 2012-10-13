using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Objects
{
    class Projectile : DynamicObject
    {
        #region "Globals"
        protected float lifeSpan; // how long the bullet lasts
        protected float Damage;

        public float damage
        {
            get { return Damage; }
        }

        public StaticObject parent;
        #endregion

        #region "Constructors"
        protected override void setData()
        {
            this.rollSpeed = 10;
            this.yawSpeed = 2.5f;
            this.pitchSpeed = 2.5f;
            this.maxSpeed = 58;
            this.minSpeed = 0;
            this.mass = 0;
            this.greatestLength = 2f;
            this.shipData.scale = 1f;
            this.lifeSpan = 15;

            this.Damage = 10;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="parent">The parent of the bullet</param>
        public Projectile(Game game, StaticObject parent)
            : base(game, Objects.Team.neutral, parent.Position + Vector3.Transform(new Vector3(0, -parent.getGreatestLength / 2, parent.getGreatestLength / 2), Matrix.CreateFromQuaternion(parent.rotation)))
        {
            this.Health = 10;
            numHudLines = 3;
            typeOfLine = PrimitiveType.LineStrip;

            Vector3 foreward = parent.Position + Vector3.Transform(new Vector3(0, 0, 10), Matrix.CreateFromQuaternion(parent.rotation));
            Vector3 PYR = MathEuler.AngleTo(foreward, parent.Position);

            rotation = Quaternion.CreateFromYawPitchRoll(PYR.Y, PYR.X, PYR.Z);

            this.shipData.speed = parent.ShipMovementInfo.speed;

            this.parent = parent;
        }
        #endregion

        #region "Update"
        public override void Update(GameTime gt)
        {

            this.lifeSpan -= 1 * (float)gt.ElapsedGameTime.TotalSeconds;

            if (lifeSpan <= 0)
                this.Health = 0;

            base.Update(gt);
        }

        protected override void resetModels()
        {
            base.resetModels();
        }        

        public override void controller(GameTime gt)
        {

            base.controller(gt);
        }

        protected override void setVertexPosition(float screenX, float screenY, float radiusOfObject, Color col)
        {
            radiusOfObject = 2;

            //Line 1
            targetBoxVertices[0].Position.X = screenX;
            targetBoxVertices[0].Position.Y = screenY + radiusOfObject * 1.8f;
            targetBoxVertices[0].Color = col;

        //    Line 2
            targetBoxVertices[1].Position.X = screenX - radiusOfObject * 1.8f;
            targetBoxVertices[1].Position.Y = screenY - radiusOfObject;
            targetBoxVertices[1].Color = col;

        //    Line 3
            targetBoxVertices[2].Position.X = screenX + radiusOfObject * 1.8f;
            targetBoxVertices[2].Position.Y = screenY - radiusOfObject;
            targetBoxVertices[2].Color = col;

        //    Line 4
            targetBoxVertices[3].Position.X = screenX;
            targetBoxVertices[3].Position.Y = screenY + radiusOfObject * 1.8f;
            targetBoxVertices[3].Color = col;
        }
        #endregion
    }
}