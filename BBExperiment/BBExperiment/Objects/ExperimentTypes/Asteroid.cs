using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region "XNA Using Statements"
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace BBN_Game.Objects
{
    class Asteroid : StaticObject
    {
        public Asteroid(Game game, Team team, Vector3 position) : base(game, team, position)
        {

        }

        public override void Update(GameTime gt)
        {
            this.shipData.roll = (float)(rollSpeed * gt.ElapsedGameTime.TotalSeconds);
            this.shipData.yaw = (float)(pitchSpeed * gt.ElapsedGameTime.TotalSeconds);

            base.Update(gt);
        }

        protected override void setData()
        {
            Random rand = new Random();

            rollSpeed = (float)rand.NextDouble() * 6 + 8;
            yawSpeed = (float)rand.NextDouble() * 4 + 6;

            this.shipData.scale = 1;

            this.mass = 10f;
            greatestLength = 10.0f;
            Shield = 100;
            Health = 100;
            totalHealth = 100;
            numHudLines = 4;
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

        protected override void resetModels()
        {
            model = Game.Content.Load<Model>("Models/Asteroids/AstrFBX");

            base.resetModels();
        }
    }
}
