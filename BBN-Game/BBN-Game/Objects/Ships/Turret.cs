using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Objects
{
    class Turret : StaticObject
    {
        private void setData()
        {
            this.rollSpeed = 5;
            this.pitchSpeed = 10;
            this.yawSpeed = 5;
            this.greatestLength = 6f;
        }


        public Turret(Game game, Team team, Vector3 position)
            : base(game, team, position)
        {
        }

        protected override void resetModels()
        {
            if (this.Team == Team.Red)
                model = Game.Content.Load<Model>("Models/Ships/FighterRed");
            else
                model = Game.Content.Load<Model>("Models/Ships/FighterBlue");

            base.resetModels();
        }
    }
}
