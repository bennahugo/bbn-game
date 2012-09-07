using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BBN_Game.Controller
{
    class GridDataCollection
    {
        public static float MAX_CAPTURE_DISTANCE = 50;
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
                        if (player.Team == Objects.Team.Red) 
                            Controller.GameController.team1.teamCredits += TradingInformation.creditsForCapturingTower;
                        else
                            Controller.GameController.team2.teamCredits += TradingInformation.creditsForCapturingTower;
                        break;
                    }
                }
            }
        }
    }
}
