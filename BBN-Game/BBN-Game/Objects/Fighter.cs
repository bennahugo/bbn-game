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
        private void setData()
        {
            this.rollSpeed = 5;
            this.pitchSpeed = 10;
            this.yawSpeed = 5;
            this.maxSpeed = 300;
            this.minSpeed = -25;
            this.greatestLength = 6f;
        }


        public Fighter(Game game)
            : base(game)
        {
            this.Position = new Vector3(0, 0, 10);
        }

        public void LoadContent()
        {
            this.model = Game.Content.Load<Model>("Models/Ships/FighterRed");
            base.LoadContent();
        }
    }
}
