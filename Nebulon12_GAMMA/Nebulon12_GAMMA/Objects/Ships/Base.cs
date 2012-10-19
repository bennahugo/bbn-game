using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Objects
{
    class Base : StaticObject
    {
        #region "Constructors"
        protected override void setData()
        {
            this.rollSpeed = 5;
            this.pitchSpeed = 10;
            this.yawSpeed = 5;
            this.greatestLength = 6f;
            this.shipData.scale = 1;
            numHudLines = 360 / 20;
            typeOfLine = PrimitiveType.LineStrip;
            Shield = 100;
            Health = 2000;
            totalHealth = 2000;
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
            sphere.Radius *= this.shipData.scale * 0.7f;

            return sphere;
        }

        public Base(Game game, Team team, Vector3 position)
            : base(game, team, position)
        {
        }
        #endregion

        #region "Update"
        protected override void resetModels()
        {
            if (this.Team == Team.Red)
                model = Game.Content.Load<Model>("Models/Ships/spaceStation");
            else
                model = Game.Content.Load<Model>("Models/Ships/spaceStation");

                base.resetModels();
        }

        protected override void setVertexPosition(float screenX, float screenY, float radiusOfObject, Color col)
        {
            for (int i = 0; i <= 360; i += 20)
            {
                targetBoxVertices[i / 20].Position.X = screenX + (float)Math.Sin(MathHelper.ToRadians(i)) * radiusOfObject;
                targetBoxVertices[i / 20].Position.Y = screenY + (float)Math.Cos(MathHelper.ToRadians(i)) * radiusOfObject;
                targetBoxVertices[i / 20].Color = col;
            }
        }
        #endregion

    }
}
