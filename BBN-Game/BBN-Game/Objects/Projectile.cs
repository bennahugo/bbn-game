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
        StaticObject target;

        protected float acceleration;
        protected double EPSILON_DISTANCE = 0.0001f;
        protected const float TURNING_SPEED_COEF = 1.05f;
        private const float DISTANCE_TO_TARGET_IN_SECONDS_WHEN_VERY_CLOSE = 0.01f;
        private const float DISTANCE_TO_TARGET_IN_SECONDS_WHEN_CLOSE = 0.1f;
        protected float lifeSpan;

        Boolean hit = false;

        protected override void setData()
        {
            this.rollSpeed = 10;
            this.yawSpeed = 3.5f;
            this.pitchSpeed = 3.5f;
            this.maxSpeed = 70;
            this.minSpeed = 0;
            this.mass = 0;
            this.greatestLength = 2f;
            this.shipData.scale = 0.2f;
            this.acceleration = 50f;
            this.lifeSpan = 20;
        }

        public Projectile(Game game, StaticObject target, StaticObject parent)
            : base(game)
        {
            this.target = target;
            Vector3 move = parent.Position + Vector3.Transform(new Vector3(0, 0, parent.getGreatestLength / 2), Matrix.CreateFromQuaternion(parent.rotation));
            this.Position = move;
            this.rotation = parent.rotation;
            shipData.speed = parent.ShipMovementInfo.speed;
        }

        public override void controller(GameTime gt)
        {
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

                Matrix m = Matrix.CreateFromQuaternion(rotation);
                Quaternion pitch = Quaternion.CreateFromAxisAngle(m.Right, diffp);
                Quaternion yaw = Quaternion.CreateFromAxisAngle(m.Up, diffy);
                //special case: deal with the pitch if its PI/2 or -PI/2, because if its slightly off it causes problems:
                if (Math.Abs(Math.Abs(tpitch) - Math.PI / 2) <= EPSILON_DISTANCE && !(Math.Abs(diffy) <= EPSILON_DISTANCE))
                    rotation = Quaternion.CreateFromYawPitchRoll(tyaw, tpitch, 0);
                else
                    rotation = yaw * pitch * rotation;
                #endregion

                #region "Speed"
                float compLookOntoWant = Vector3.Dot(vLookDir, vWantDir);
                if (Math.Abs(compLookOntoWant) > 1)
                    compLookOntoWant = 1;
                shipData.speed = this.maxSpeed * (float)(Math.Pow(TURNING_SPEED_COEF, -Math.Abs(Math.Acos(compLookOntoWant) * 180 / Math.PI)));
                #endregion

                this.lifeSpan -= 1 * time;
            }
            else // if the bullet is very close
            {
                shipData.speed = 0;

                //BULLET IS ON TARGET MOERSE BANG, PARTS FLYING... BLOOD... GORE...
                hit = true;
            }
            base.controller(gt);
        }

        public void LoadContent()
        {
            this.model = Game.Content.Load<Model>("Models/Projectiles/projectile1");
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime, BBN_Game.Camera.CameraMatrices cam)
        {
            SpriteBatch b = new SpriteBatch(Game.GraphicsDevice);

            SpriteFont f = Game.Content.Load<SpriteFont>("SpriteFont1");

            b.Begin();
            if (hit)
                b.DrawString(f, "DEAD", new Vector2(10, 10), Color.Red);
            b.DrawString(f, shipData.speed.ToString("0000") + " " + (target.Position - Position).Length().ToString("0000"), new Vector2(0, 0), Color.Yellow);
            b.End();

            base.Draw(gameTime, cam);
        }
    }
}