using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBN_Game.Controller
{
    class GridDataCollection
    {
        public static void tryCaptureTower(Objects.playerObject player)
        {
            List<Grid.GridObjectInterface> list = GameController.Grid.checkNeighbouringBlocks(player);

            foreach (Grid.GridObjectInterface item in list)
            {
                if (item is Objects.Turret)
                {
                    Objects.Turret turret = ((Objects.Turret)item);
                    if (turret.Team.Equals(Objects.Team.nutral))
                    {
                        turret.changeTeam(player.Team);
                        break;
                    }
                }
            }
        }
    }
}
