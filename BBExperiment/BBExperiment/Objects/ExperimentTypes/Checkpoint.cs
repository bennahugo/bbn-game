using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BBN_Game.Controller;

/////
///
/// Author - Brandon James Talbot
/// 
/// This is the Checkpoint Object class
////

namespace BBN_Game.Objects
{
    class Checkpoint : StaticObject
    {
        float theta = 0;
        float originalScale = -1;
        Texture2D texCheckpoint = null;
        Texture2D texFinishline = null;
        #region "Constructors"
        protected override void setData()
        {
            this.rollSpeed = 5;
            this.pitchSpeed = 1;
            this.yawSpeed = 1;
            this.greatestLength = 6f;
            numHudLines = 8;
            typeOfLine = PrimitiveType.LineStrip;

            Shield = 100;
            Health = 200;
            totalHealth = 200;
        }


        public Checkpoint(Game game, Team team, Vector3 position)
            : base(game, team, position)
        {
        }
        #endregion

        #region "Update"
        protected override void resetModels()
        {
            model = Game.Content.Load<Model>("Models/Projectiles/Cube");
            texCheckpoint = Game.Content.Load<Texture2D>("progressCheck");
            texFinishline = Game.Content.Load<Texture2D>("finishCheck");
            base.resetModels();
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
            if (originalScale < 0)
                originalScale = shipData.scale;
            this.shipData.scale = (float)(originalScale*(1 + Math.Abs(Math.Sin(theta))*1.5));
            theta += (float)Math.PI / 64;
            if (theta > 2 * Math.PI)
                theta -= 2 * (float)Math.PI;
        }
        public override void Draw(GameTime gameTime, Camera.CameraMatrices cam)
        {
            if (!IsVisible(cam))
                return;

            if (((cam.Position - Position).Length() > 600) && !(this is Planets.Planet) && !(this is Asteroid)) // depth culling
                return;

            foreach (ModelMesh m in model.Meshes)
            {
                foreach (BasicEffect e in m.Effects)
                {
                    e.World = world;
                    e.View = cam.View;
                    e.Projection = cam.Projection;
                    e.LightingEnabled = false;
                    e.DiffuseColor = Vector3.One;
                    e.SpecularColor = Vector3.One;
                    e.EmissiveColor = Vector3.One;
                    e.AmbientLightColor = Vector3.One;
                    //e.Alpha = 0.15f;
                    //if (GameController.CheckPoints.Count == 0)
                    //    e.Texture = texFinishline;
                    //else
                    //    e.Texture = texCheckpoint;
                    e.TextureEnabled = true;
                }
                m.Draw();
            }
            base.Draw(gameTime);
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
        #endregion
    }
}
