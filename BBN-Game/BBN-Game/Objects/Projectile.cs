using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Objects
{
    class Projectile : DynamicObject
    {
        public Projectile(Game game)
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
