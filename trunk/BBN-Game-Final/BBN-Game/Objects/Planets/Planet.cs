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
        public Planet(Game game, Team team, Vector3 position) : base(game, team, position)
        {
            Random rand = new Random();

            int r = rand.Next(3);
            
            if (r == 0)
                this.model = Game.Content.Load<Model>("Models/Planets/CallistoModel");
            else if (r == 1) 
                this.model = Game.Content.Load<Model>("Models/Planets/Saturn");
            else
                this.model = Game.Content.Load<Model>("Models/Planets/Venus");
        }

        public Planet(Game game, Team team, Vector3 position, Planet p)
            : base(game, team, position)
        {
            Random rand = new Random();

            int r = rand.Next(3);


            if (r == 0)
                this.model = Game.Content.Load<Model>("Models/Planets/CallistoModel");
            else if (r == 1)
                this.model = Game.Content.Load<Model>("Models/Planets/Saturn");
            else
                this.model = Game.Content.Load<Model>("Models/Planets/Venus");

            while (this.model.Equals(p.model))
            {
                r = rand.Next(4);


                if (r == 0)
                    this.model = Game.Content.Load<Model>("Models/Planets/CallistoModel");
                else if (r == 1)
                    this.model = Game.Content.Load<Model>("Models/Planets/Saturn");
                else if (r == 2)
                    this.model = Game.Content.Load<Model>("Models/Planets/Venus");
                else
                    this.model = Game.Content.Load<Model>("Models/Planets/mars2");
            }
        }

        public Planet(Game game, Team team, Vector3 position, Planet p, Planet p2)
            : base(game, team, position)
        {
            Random rand = new Random();

            int r = rand.Next(3);


            if (r == 0)
                this.model = Game.Content.Load<Model>("Models/Planets/CallistoModel");
            else if (r == 1)
                this.model = Game.Content.Load<Model>("Models/Planets/Saturn");
            else
                this.model = Game.Content.Load<Model>("Models/Planets/Venus");

            while (this.model.Equals(p.model) || this.model.Equals(p2.model))
            {
                r = rand.Next(4);


                if (r == 0)
                    this.model = Game.Content.Load<Model>("Models/Planets/CallistoModel");
                else if (r == 1)
                    this.model = Game.Content.Load<Model>("Models/Planets/Saturn");
                else if (r == 2)
                    this.model = Game.Content.Load<Model>("Models/Planets/Venus");
                else
                    this.model = Game.Content.Load<Model>("Models/Planets/mars2");
            }
        }

        public override void Update(GameTime gt)
        {

            this.shipData.roll = (float)(rollSpeed * gt.ElapsedGameTime.TotalSeconds);
            this.shipData.yaw = (float)(pitchSpeed * gt.ElapsedGameTime.TotalSeconds);


            base.Update(gt);
        }

        protected override void setData()
        {
            Random rand = new Random();

            rollSpeed = (float)rand.NextDouble() * 6 + 3;
            yawSpeed = (float)rand.NextDouble() * 4 + 3;

            this.shipData.scale = 300 + rand.Next(700);
            Health = 100;
        }
    }
}
