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
    class Destroyer : DynamicObject
    {
        #region "Constructors"
        protected override void setData()
        {
            this.rollSpeed = 5;
            this.pitchSpeed = 10;
            this.yawSpeed = 5;
            this.maxSpeed = 300;
            this.minSpeed = -25;
            this.greatestLength = 6f;
            numHudLines = 4;
            typeOfLine = PrimitiveType.LineStrip;
            Shield = 100;
            Health = 100;
            totalHealth = 100;
        }


        public Destroyer(Game game, Team team, Vector3 position)
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
        #endregion
    }
}
