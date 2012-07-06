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
        protected float lifeSpan;

        protected override void setData()
        {
            this.rollSpeed = 10;
            this.yawSpeed = 2.5f;
            this.pitchSpeed = 2.5f;
            this.maxSpeed = 58;
            this.minSpeed = 0;
            this.mass = 0;
            this.greatestLength = 2f;
            this.shipData.scale = 0.1f;
            this.lifeSpan = 15;
        }

        public Projectile(Game game, StaticObject parent)
            : base(game, Objects.Team.nutral, parent.Position + Vector3.Transform(new Vector3(0, -parent.getGreatestLength / 4, parent.getGreatestLength / 4), Matrix.CreateFromQuaternion(parent.rotation)))
        {
            this.rotation = parent.rotation;

            this.shipData.speed = parent.ShipMovementInfo.speed;
        }

        public override void controller(GameTime gt)
        {

            base.controller(gt);
        }
    }
}