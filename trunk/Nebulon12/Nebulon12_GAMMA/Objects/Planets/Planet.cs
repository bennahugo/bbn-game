using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Objects.Planets
{
    class Planet : StaticObject
    {
        Model moon;
        Vector3 moonPos;
        Matrix moonWorld;
        float moonScale;
        float radiusOfPlanet;
        float radiusOfMoon;
        float roll;
        float moonRollSpeed;
        float yaw;
        float moonYawSpeed;
        float distanceAway;
        float startingYaw;
        BoundingSphere moonSphere;

        public Planet(Game game, Team team, Vector3 position)
            : base(game, team, position)
        {
            if (team.Equals(Team.Blue))
            {
                model = game.Content.Load<Model>("Models/Planets/Venus");
                moon = game.Content.Load<Model>("Models/Planets/mars2");
            }
            else
            {
                model = game.Content.Load<Model>("Models/Planets/CallistoModel");
                moon = game.Content.Load<Model>("Models/Planets/Saturn");
            }


            Bsphere = createShpere();
            radiusOfPlanet = Bsphere.Radius;

            BoundingSphere sphere = new BoundingSphere();

            sphere = new BoundingSphere();
            foreach (ModelMesh m in moon.Meshes)
            {
                if (sphere.Radius == 0)
                    sphere = m.BoundingSphere;
                else
                    sphere = BoundingSphere.CreateMerged(sphere, m.BoundingSphere);
            }
            sphere.Radius *= moonScale;
            moonSphere = sphere;
            radiusOfMoon = sphere.Radius;

            roll = 0;
            yaw = 0;

            moonPos = this.Position + Vector3.Transform(new Vector3(radiusOfMoon + radiusOfPlanet + distanceAway, 0, 0), Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(startingYaw, 0, roll)));
            moonWorld = Matrix.CreateScale(moonScale) * Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(yaw, pitchSpeed, roll)) * Matrix.CreateTranslation(moonPos);
        }

        protected Boolean isMoonVis(Camera.CameraMatrices camera)
        {
            BoundingSphere localSphere = moonSphere;

            localSphere.Center = moonPos;

            ContainmentType contains = camera.getBoundingFrustum.Contains(localSphere);
            if (contains == ContainmentType.Contains || contains == ContainmentType.Intersects)
                return true;

            return false;
        }

        public override void Update(GameTime gt)
        {

            this.shipData.roll = (float)(rollSpeed * gt.ElapsedGameTime.TotalSeconds);
            this.shipData.yaw = (float)(pitchSpeed * gt.ElapsedGameTime.TotalSeconds);

            roll += (float)(moonRollSpeed * gt.ElapsedGameTime.TotalSeconds);
            yaw += (float)(moonYawSpeed * gt.ElapsedGameTime.TotalSeconds);

            moonPos = this.Position + Vector3.Transform(new Vector3(radiusOfMoon + radiusOfPlanet + distanceAway, 0, 0), Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(startingYaw, 0, roll)));
            moonWorld = Matrix.CreateScale(moonScale) * Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(yaw, pitchSpeed, roll)) * Matrix.CreateTranslation(moonPos);

            base.Update(gt);
        }

        public override void Draw(GameTime gameTime, Camera.CameraMatrices cam)
        {
            if (isMoonVis(cam))
            {
                foreach (ModelMesh m in moon.Meshes)
                {
                    foreach (BasicEffect e in m.Effects)
                    {
                        e.EnableDefaultLighting();
                        e.PreferPerPixelLighting = true;
                        e.Parameters["World"].SetValue(moonWorld);
                        e.Parameters["View"].SetValue(cam.View);
                        e.Parameters["Projection"].SetValue(cam.Projection);
                    }
                    m.Draw();
                }

            }
            base.Draw(gameTime, cam);
        }

        protected override void setData()
        {
            Random rand = new Random();

            rollSpeed = (float)rand.NextDouble() * 6 + 3;
            yawSpeed = (float)rand.NextDouble() * 4 + 3;

            moonYawSpeed = (float)rand.NextDouble() / 10;
            moonRollSpeed = (float)rand.NextDouble() / 10;

            startingYaw = (float)(rand.NextDouble() * (MathHelper.Pi * 2));

            distanceAway = rand.Next(300) + 100; 

            this.shipData.scale = 300 + rand.Next(700);
            moonScale = this.shipData.scale * 0.2f;
            Health = 100;
        }
    }
}
