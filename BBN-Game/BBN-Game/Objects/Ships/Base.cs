﻿using System;
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
            numHudLines = 360 / 20;
            typeOfLine = PrimitiveType.LineStrip;
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
                model = Game.Content.Load<Model>("Models/Ships/FighterRed");
            else
                model = Game.Content.Load<Model>("Models/Ships/FighterBlue");

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
