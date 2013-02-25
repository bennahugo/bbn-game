using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Objects
{
    class Bullet : Projectile
    {
        #region "Globals"
        protected double EPSILON_DISTANCE = 0.0001f;
        protected const float TURNING_SPEED_COEF = 0.8f;
        private const float DISTANCE_TO_TARGET_IN_SECONDS_WHEN_VERY_CLOSE = 0.01f;
        private const float DISTANCE_TO_TARGET_IN_SECONDS_WHEN_CLOSE = 0.1f;

        StaticObject target;
        #endregion

        #region "Constructors - Data settors"
        protected override void setData()
        {
            this.rollSpeed = 0.9f;
            this.yawSpeed = 0.9f;
            this.pitchSpeed = 1f;
            this.maxSpeed = 60;
            this.minSpeed = 0;
            this.mass = 0;
            this.greatestLength = 2f;
            this.shipData.scale = 0.2f;
            this.lifeSpan = 5.68f;

            this.Damage = 10f;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="parent">The parent of the bullet</param>
        /// <param name="target">The target for the object</param>
        public Bullet(Game game, StaticObject target, StaticObject parent)
            : base(game, parent)
        {
            this.target = target;
        }

        #endregion

        protected override void resetModels()
        {
            model = Game.Content.Load<Model>("Models/Projectiles/Cube");

            base.resetModels();
        }

        public override void controller(GameTime gt)
        {
            //shipData.speed = maxSpeed;
            //float veryCloseToTarget = this.getMaxSpeed * DISTANCE_TO_TARGET_IN_SECONDS_WHEN_VERY_CLOSE;
            //float closeToTarget = this.getMaxSpeed * DISTANCE_TO_TARGET_IN_SECONDS_WHEN_CLOSE;
            //float distanceFromTarget = (target.Position - this.Position).Length();
            //if ((target.Position - this.Position).Length() <= veryCloseToTarget)
            //    this.destroy = true;

            if (this.target != null)
                chaseTarget(gt);
            else
                this.shipData.speed = maxSpeed;

            //Vector3 newRotation = (this.Position - target.Position);

            //Matrix rotate = Matrix.CreateFromQuaternion(rotation);
            //rotate.Forward = newRotation;
            //rotate.Translation = Position;

            //this.rotation = Quaternion.CreateFromRotationMatrix(rotate);

            //shipData.speed = maxSpeed;

            base.controller(gt);
        }

        public void chaseTarget(GameTime gt)
        {
            if (target == null) return;
            float veryCloseToTarget = this.getMaxSpeed * DISTANCE_TO_TARGET_IN_SECONDS_WHEN_VERY_CLOSE;
            float closeToTarget = this.getMaxSpeed * DISTANCE_TO_TARGET_IN_SECONDS_WHEN_CLOSE;
            float distanceFromTarget = (target.Position - this.Position).Length();
            if ((target.Position - this.Position).Length() > veryCloseToTarget)
            {
                float time = (float)gt.ElapsedGameTime.TotalSeconds;

                #region "Rotations"

                Vector3 vWantDir = Vector3.Normalize(target.Position - Position);
                float distance = (float)Math.Sqrt(vWantDir.Z * vWantDir.Z + vWantDir.X * vWantDir.X);
                float tpitch = distance == 0 ? (float)Math.Sign(-vWantDir.Y) * (float)Math.PI / 2 : -(float)Math.Atan2(vWantDir.Y, distance);
                float tyaw = (float)Math.Atan2(vWantDir.X, vWantDir.Z);
                Vector3 vLookDir = Vector3.Normalize(-Matrix.CreateFromQuaternion(rotation).Forward);
                distance = (float)Math.Sqrt(vLookDir.Z * vLookDir.Z + vLookDir.X * vLookDir.X);
                float cyaw = (float)Math.Atan2(vLookDir.X, vLookDir.Z);
                float cpitch = distance == 0 ? (float)Math.Sign(-vLookDir.Y) * (float)Math.PI / 2 : -(float)Math.Atan2(vLookDir.Y, distance);

                //now rotate towards the target yaw and pitch
                float diffy = tyaw - cyaw;
                float diffp = tpitch - cpitch;

                //get the direction we need to rotate in:
                if (Math.Abs(diffy) > Math.PI)
                    if (tyaw > cyaw)
                        diffy = -(float)(Math.PI * 2 - Math.Abs(diffy));
                    else
                        diffy = (float)(Math.PI * 2 - Math.Abs(diffy));

                if (Math.Abs(diffp) > Math.PI)
                    if (tpitch > cpitch)
                        diffp = -(float)(Math.PI * 2 - Math.Abs(diffp));
                    else
                        diffp = (float)(Math.PI * 2 - Math.Abs(diffp));

                if (Math.Abs(diffp) > Math.Abs(pitchSpeed) * (float)(gt.ElapsedGameTime.TotalSeconds))
                    diffp = Math.Sign(diffp) * Math.Abs(pitchSpeed) * (float)(gt.ElapsedGameTime.TotalSeconds);
                if (Math.Abs(diffy) > Math.Abs(yawSpeed) * (float)(gt.ElapsedGameTime.TotalSeconds))
                    diffy = Math.Sign(diffy) * Math.Abs(yawSpeed) * (float)(gt.ElapsedGameTime.TotalSeconds);

                //special case: deal with the pitch if its PI/2 or -PI/2, because if its slightly off it causes problems:
                if (Math.Abs(Math.Abs(tpitch) - Math.PI / 2) <= EPSILON_DISTANCE && !(Math.Abs(diffy) <= EPSILON_DISTANCE))
                    rotation = Quaternion.CreateFromYawPitchRoll(tyaw, tpitch, 0);
                else
                    rotation = Quaternion.CreateFromYawPitchRoll(cyaw + diffy, cpitch + diffp, 0);
                #endregion

                #region "Speed"
                float compLookOntoWant = Vector3.Dot(vLookDir, vWantDir);
                if (Math.Abs(compLookOntoWant) > 1)
                    compLookOntoWant = 1;
                shipData.speed += (this.maxSpeed * (float)(Math.Pow(TURNING_SPEED_COEF, -Math.Abs(Math.Acos(compLookOntoWant) * 180 / Math.PI)))) * (float)(gt.ElapsedGameTime.TotalSeconds);
                #endregion
            }
            else // if the bullet is very close
            {
                shipData.speed = 0;

                //BULLET IS ON TARGET MOERSE BANG, PARTS FLYING... BLOOD... GORE...
            }
        }
    }
}
