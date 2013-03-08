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
    class Target : StaticObject
    {
        public Target(Game game, Team team, Vector3 position) : base(game, team, position)
        {

        }

        protected override void setData()
        {
            this.rollSpeed = 5;
            this.pitchSpeed = 1;
            this.yawSpeed = 1;
            this.greatestLength = 6f;
            this.shipData.scale = 6;
            numHudLines = 8;
            typeOfLine = PrimitiveType.LineStrip;

            Shield = 100;
            Health = 100;
            totalHealth = 100;
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

        public override void Update(GameTime gt)
        {
            this.shipData.roll = (float)(rollSpeed * gt.ElapsedGameTime.TotalSeconds);
            this.shipData.yaw = (float)(pitchSpeed * gt.ElapsedGameTime.TotalSeconds);

            base.Update(gt);
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

        protected override void resetModels()
        {
            model = Game.Content.Load<Model>("Models/Planets/CallistoModel");

            base.resetModels();
        }
    }
}
