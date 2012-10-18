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
/// This is the Destroyer Object class
////

namespace BBN_Game.Objects
{
    class Fighter : DynamicObject
    {
        #region "Constructors"
        protected override void setData()
        {
            this.rollSpeed = 5;
            this.pitchSpeed = 1;
            this.yawSpeed = 1;
            this.maxSpeed = 40;
            this.minSpeed = -25;
            this.greatestLength = 6f;
            numHudLines = 8;
            typeOfLine = PrimitiveType.LineStrip;

            Shield = 100;
            Health = 200;
            totalHealth = 200;
        }


        public Fighter(Game game, Team team, Vector3 position)
            : base(game, team, position)
        {
        }
        #endregion

        #region "Update"
        protected override void resetModels()
        {
            if (this.Team == Team.Red)
                model = Game.Content.Load<Model>("Models/Ships/Fighter1");
            else
                model = Game.Content.Load<Model>("Models/Ships/Fighter2");

            base.resetModels();
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
