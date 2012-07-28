using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.AI;
using BBN_Game.Objects;
using Microsoft.Xna.Framework;
using BBN_Game.Map;
using Microsoft.Xna.Framework.Graphics;
using BBN_Game.Utils;
using BBN_Game.Camera;
using BBN_Game.Controller;
using BBN_Game.Grid;

namespace BBN_Game
{
    static class AITest
    {
        public static AIController myAIController;
        public static GridStructure gridStructure;
        public static NavigationComputer navComputer;
        public static void update(GameTime gameTime,Game game)
        {
            navComputer.updateAIMovement(gameTime);
            myAIController.update(gameTime,game);
        }
        public static void drawPath(DynamicObject obj, Camera.CameraMatrices chasCam, BasicEffect bf, GraphicsDevice gd)
        {
            List<Node> path = navComputer.isObjectRegistered(obj) ? navComputer.getPath(obj) : new List<Node>();

            if (path.Count > 0)
            {
                Node nextWaypoint = path.Last();
                for (int i = 0; i < path.Count - 1; ++i)
                {
                    Algorithms.Draw3DLine(Color.Yellow, path.ElementAt(i).Position, path.ElementAt(i + 1).Position,
                        bf, gd, chasCam.Projection, chasCam.View, Matrix.Identity);
                }
                Algorithms.Draw3DLine(Color.Green, path.Last().Position, obj.Position,
                        bf, gd, chasCam.Projection, chasCam.View, Matrix.Identity);
            }
        }
        public static void draw(GameTime gameTime, Camera.CameraMatrices chasCam, BasicEffect bf, GraphicsDevice gd)
        {
            for (int team = 0; team < myAIController.getTeamCount(); ++team)
            {
                TeamInformation ti = myAIController.getTeam(team);

                drawPath(ti.teamPlayer, chasCam, bf, gd);
                foreach (Destroyer d in ti.teamDestroyers)
                    drawPath(d, chasCam, bf, gd);
                foreach (Fighter f in ti.teamFighters)
                    drawPath(f, chasCam, bf, gd);
            }
        }
    }
}
