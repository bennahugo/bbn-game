using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region "XNA Using Statements"
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BBN_Game.Objects;
#endregion


namespace BBExperiment.Objects
{
    class MovingTarget : DynamicObject
    {
        public MovingTarget(Game game, Team team, Vector3 position)
            : base(game, team, position)
        {
        }
    }
}
