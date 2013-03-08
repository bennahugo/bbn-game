using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region "XNA Using Statements"
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BBN_Game.Objects;
#endregion


namespace BBExperiment.Objects
{
    class MovingTarget : DynamicObject
    {
        float timer;
        float x, y, z;
        static Random rand = new Random();

        public MovingTarget(Game game, Team team, Vector3 position)
            : base(game, team, position)
        {
            timer = 10;
        }

        protected override void setData()
        {
            this.mass = 10;
            this.pitchSpeed = 40;
            this.maxSpeed = 1;
            this.minSpeed = -1;
            this.greatestLength = 10f;
            this.shipData.scale = 10f;

            numHudLines = 8;
            typeOfLine = PrimitiveType.LineStrip;

            Shield = 100;
            Health = 100;
            totalHealth = 100;

            x = (float)((float)(rand.Next(-3, 10)));
            y = (float)((float)(rand.Next(-10, 5)));
            z = (float)((float)(rand.Next(-5, 2)));
        }

        Boolean plus = true;
        public override void Update(GameTime gt)
        {
            float seconds = (float)gt.ElapsedGameTime.TotalSeconds;

            timer -= (float)gt.ElapsedGameTime.TotalSeconds;

            if (timer <= 0)
            {
                timer = 10;
                plus = !plus;
            }

            float speed = plus ? maxSpeed : minSpeed;
            //shipData.roll = roll;
            //shipData.yaw = yaw;
            //shipData.pitch = pitch;

            this.Position += new Vector3(x * speed * seconds, y * speed * seconds, z * speed * seconds);


            //setWorldMatrix();
            base.Update(gt);
        }

        protected override void resetModels()
        {
            model = Game.Content.Load<Model>("Models/Planets/mars2");

            base.resetModels();
        }

        protected override BoundingSphere createShpere()
        {
            BoundingSphere sphere = new BoundingSphere();

            sphere = new BoundingSphere();
            foreach (ModelMesh m in model.Meshes)
            {
                if (sphere.Radius == 0)
                    sphere = m.BoundingSphere;
                else
                    sphere = BoundingSphere.CreateMerged(sphere, m.BoundingSphere);
            }
            sphere.Radius *= this.shipData.scale;

            return sphere;
        }

        protected override void setVertexPosition(float screenX, float screenY, float radiusOfObject, Color col)
        {
            //Line 1
            targetBoxVertices[0].Position.X = screenX - radiusOfObject / 2;
            targetBoxVertices[0].Position.Y = screenY + radiusOfObject;
            targetBoxVertices[0].Color = col;

            //Line 2
            targetBoxVertices[1].Position.X = screenX - radiusOfObject;
            targetBoxVertices[1].Position.Y = screenY + radiusOfObject / 2;
            targetBoxVertices[1].Color = col;

            //Line 3
            targetBoxVertices[2].Position.X = screenX - radiusOfObject;
            targetBoxVertices[2].Position.Y = screenY - radiusOfObject / 2;
            targetBoxVertices[2].Color = col;

            //Line 4
            targetBoxVertices[3].Position.X = screenX - radiusOfObject / 2;
            targetBoxVertices[3].Position.Y = screenY - radiusOfObject;
            targetBoxVertices[3].Color = col;

            //Line 5
            targetBoxVertices[4].Position.X = screenX + radiusOfObject / 2;
            targetBoxVertices[4].Position.Y = screenY - radiusOfObject;
            targetBoxVertices[4].Color = col;

            //Line 6
            targetBoxVertices[5].Position.X = screenX + radiusOfObject;
            targetBoxVertices[5].Position.Y = screenY - radiusOfObject / 2;
            targetBoxVertices[5].Color = col;

            //Line 7
            targetBoxVertices[6].Position.X = screenX + radiusOfObject;
            targetBoxVertices[6].Position.Y = screenY + radiusOfObject / 2;
            targetBoxVertices[6].Color = col;

            //Line 8
            targetBoxVertices[7].Position.X = screenX + radiusOfObject / 2;
            targetBoxVertices[7].Position.Y = screenY + radiusOfObject;
            targetBoxVertices[7].Color = col;

            //Line 9
            targetBoxVertices[8].Position.X = screenX - radiusOfObject / 2;
            targetBoxVertices[8].Position.Y = screenY + radiusOfObject;
            targetBoxVertices[8].Color = col;
        }
    }
}
