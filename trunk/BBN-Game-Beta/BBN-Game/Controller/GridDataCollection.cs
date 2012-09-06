using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBN_Game.Controller
{
    class GridDataCollection
    {
        private static float MAX_CAPTURE_DISTANCE = 50;
        public static void tryCaptureTower(Objects.playerObject player)
        {
            List<Grid.GridObjectInterface> list = GameController.Grid.checkNeighbouringBlocks(player);

            foreach (Grid.GridObjectInterface item in list)
            {
                if (item is Objects.Turret && (item.Position - player.Position).Length() <= MAX_CAPTURE_DISTANCE)
                {
                    Objects.Turret turret = ((Objects.Turret)item);
                    if (turret.Team.Equals(Objects.Team.nutral))
                    {
                        turret.changeTeam(player.Team);
                        GameController.AIController.registerTurretOnTeam(turret, player.Team);
                        break;
                    }
                }
            }
        }
    }
}
