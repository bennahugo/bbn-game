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
        #region "Variables"
        private Boolean isRepairing = false;

        private int repairTimer = 0;
       
        public Boolean Repairing
        {
            get { return isRepairing; }
        }
        #endregion

        #region "Constructors"
        protected override void setData()
        {
            this.rollSpeed = 5;
            this.pitchSpeed = 10;
            this.yawSpeed = 5;
            this.greatestLength = 6f;
            numHudLines = 11;
            typeOfLine = PrimitiveType.TriangleList;

            Shield = 100;
            Health = 100;
            totalHealth = 100;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game class</param>
        /// <param name="team">Team for the object</param>
        /// <param name="position">Position</param>
        public Turret(Game game, Team team, Vector3 position)
            : base(game, team, position)
        {
        }

        #endregion

        #region "Update"

        public override void Update(GameTime gt)
        {
            if (this.Repairing)
            {
                if (repairTimer <= 0)
                    isRepairing = false;
                else
                    repairTimer--;
            }
            else
                this.doDamage(0.5f);

            base.Update(gt);
        }

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
            Vector2 topLeft = new Vector2(screenX - radiusOfObject, screenY + radiusOfObject);
            Vector2 topRight = new Vector2(screenX + radiusOfObject, screenY + radiusOfObject);
            Vector2 botLeft = new Vector2(screenX - radiusOfObject, screenY - radiusOfObject);
            Vector2 botRight = new Vector2(screenX + radiusOfObject, screenY - radiusOfObject);

            float amount = radiusOfObject * 0.25f;

             //top right triangle
            targetBoxVertices[0].Position.X = topRight.X - amount;
            targetBoxVertices[0].Position.Y = topRight.Y - amount;
            targetBoxVertices[0].Color = col;
            targetBoxVertices[1].Position.X = topRight.X + amount / 2;
            targetBoxVertices[1].Position.Y = topRight.Y - amount / 2;
            targetBoxVertices[1].Color = col;
            targetBoxVertices[2].Position.X = topRight.X - amount / 2;
            targetBoxVertices[2].Position.Y = topRight.Y + amount / 2;
            targetBoxVertices[2].Color = col;

             //top left
            targetBoxVertices[3].Position.X = topLeft.X + amount;
            targetBoxVertices[3].Position.Y = topLeft.Y - amount;
            targetBoxVertices[3].Color = col;
            targetBoxVertices[4].Position.X = topLeft.X + amount / 2;
            targetBoxVertices[4].Position.Y = topLeft.Y + amount / 2;
            targetBoxVertices[4].Color = col;
            targetBoxVertices[5].Position.X = topLeft.X - amount / 2;
            targetBoxVertices[5].Position.Y = topLeft.Y - amount / 2;
            targetBoxVertices[5].Color = col;

             //bot left
            targetBoxVertices[6].Position.X = botLeft.X + amount;
            targetBoxVertices[6].Position.Y = botLeft.Y + amount;
            targetBoxVertices[6].Color = col;
            targetBoxVertices[7].Position.X = botLeft.X - amount / 2;
            targetBoxVertices[7].Position.Y = botLeft.Y + amount / 2;
            targetBoxVertices[7].Color = col;
            targetBoxVertices[8].Position.X = botLeft.X + amount / 2;
            targetBoxVertices[8].Position.Y = botLeft.Y - amount / 2;
            targetBoxVertices[8].Color = col;

             //bot right
            targetBoxVertices[9].Position.X = botRight.X - amount;
            targetBoxVertices[9].Position.Y = botRight.Y + amount;
            targetBoxVertices[9].Color = col;
            targetBoxVertices[10].Position.X = botRight.X - amount / 2;
            targetBoxVertices[10].Position.Y = botRight.Y - amount / 2;
            targetBoxVertices[10].Color = col;
            targetBoxVertices[11].Position.X = botRight.X + amount / 2;
            targetBoxVertices[11].Position.Y = botRight.Y + amount / 2;
            targetBoxVertices[11].Color = col;
        }
        #endregion

        #region "Controller methods"
        public override void killObject()
        {
            isRepairing = true;

            repairTimer = 500;

            // dont call base as this object does not get removed
        }

        #endregion
    }
}
