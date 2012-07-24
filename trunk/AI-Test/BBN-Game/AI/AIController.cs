using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.Objects;
using Microsoft.Xna.Framework;

namespace BBN_Game.AI
{
    static class AIController
    {
        public const float PERCENT_OF_CREDITS_TO_SPEND_ON_FIGHTERS_WHEN_SHORT_ON_BOTH = 0.66f;
        public const float PERCENT_OF_CREDITS_TO_SPEND_ON_DESTROYERS_WHEN_SHORT_ON_BOTH = 0.33f;
        public const float DISTANCE_WHEN_TURRET_IS_CLOSE_TO_BASE = 200;
        public const int PRIORITY_FOR_ELIMINATING_PLAYER = 5;
        public const int PRIORITY_FOR_ELIMINATING_TURRET = 4;
        public const int PRIORITY_FOR_ELIMINATING_DESTROYER = 3;
        public const int PRIORITY_FOR_ELIMINATING_FIGHTER = 2;
        public const int PRIORITY_FOR_ELIMINATING_BASE = 1;
        public const int FIGHTERS_TO_SCRAMBLE_FOR_PLAYER = 6;
        public const int FIGHTERS_TO_SCRAMBLE_FOR_DESTROYER = 3;
        private static List<TeamInformation> infoOnTeams = new List<TeamInformation>();
        public static void registerTeam(TeamInformation ti)
        {
            if (!infoOnTeams.Contains(ti))
                infoOnTeams.Add(ti);
        }
        public static void registerHitOnBaseOrTurretOrFighters(StaticObject victim, StaticObject shooter)
        {
            TeamInformation tiV = getTeam(victim);
            TeamInformation tiS = getTeam(shooter);
            if (tiV == null || tiS == null) return;

            if (tiV != tiS)
                if (!isTargetMarkedForElimination(shooter, tiV))
                    if (!isTargetAlreadyBattled(tiV,shooter))
                    {
                        if (shooter is playerObject)
                        {
                            if (victim is Fighter || victim is Base)
                                tiV.scrambleQueue.Add(new KeyValuePair<int, StaticObject>(PRIORITY_FOR_ELIMINATING_PLAYER, shooter));
                            else if (victim is Turret)
                            {
                                float distanceFromTurretToHomeBase = Vector3.Distance(victim.Position, tiV.teamBase.Position);
                                if (distanceFromTurretToHomeBase <= DISTANCE_WHEN_TURRET_IS_CLOSE_TO_BASE)
                                    tiV.scrambleQueue.Add(new KeyValuePair<int, StaticObject>(PRIORITY_FOR_ELIMINATING_PLAYER, shooter));
                            }
                        }
                        else if (shooter is Destroyer)
                        {
                            if (victim is Fighter || victim is Base)
                                tiV.scrambleQueue.Add(new KeyValuePair<int, StaticObject>(PRIORITY_FOR_ELIMINATING_DESTROYER, shooter));
                            else if (victim is Turret)
                            {
                                float distanceFromTurretToHomeBase = Vector3.Distance(victim.Position, tiV.teamBase.Position);
                                if (distanceFromTurretToHomeBase <= DISTANCE_WHEN_TURRET_IS_CLOSE_TO_BASE)
                                    tiV.scrambleQueue.Add(new KeyValuePair<int, StaticObject>(PRIORITY_FOR_ELIMINATING_DESTROYER, shooter));
                            }
                        }
                    }
        }
        private static void enlistFightersToTargetDetectedEnemy()
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                Dictionary<StaticObject, int> numEngagedFightersPerEnemy = countFightersEngagedOnEnemy(ti);
                List<KeyValuePair<int,StaticObject>> enemyToReadd = new List<KeyValuePair<int,StaticObject>>();
                while (ti.scrambleQueue.Count > 0)
                {
                    //Calculate the number of fighters that need to be scrambled
                    StaticObject enemy = ti.scrambleQueue.PeekValue();
                    int numToScramble = 0;
                    if (enemy is playerObject)
                        numToScramble = FIGHTERS_TO_SCRAMBLE_FOR_PLAYER;
                    else if (enemy is Destroyer)
                        numToScramble = FIGHTERS_TO_SCRAMBLE_FOR_DESTROYER;
                    //if the enemy is already being faught then just top up the fighters when they die off
                    if (numEngagedFightersPerEnemy.Keys.Contains(enemy))
                        numToScramble -= numEngagedFightersPerEnemy[enemy];
                    //now get the healthiest fighters and scramble them:
                    if (numToScramble > 0)
                    {
                        PowerDataStructures.PriorityQueue<float, Fighter> healthiestInactiveFighters = getHealthiestFighters(ti, true);
                        int numIdleAvailable = healthiestInactiveFighters.Count;
                        //when we have enough fighters available just scramble them
                        if (numIdleAvailable >= numToScramble)
                        {
                            for (int i = 0; i < numToScramble; ++i)
                                ti.battleList.Add(healthiestInactiveFighters.DequeueValue(), enemy);
                        }
                        //when we have too few fighters reassign them to more important targets

                    }
                }
                foreach (KeyValuePair<int,StaticObject> enemy in enemyToReadd)
                    ti.scrambleQueue.Add(enemy);
            }
        }
        private static Dictionary<StaticObject, int> countFightersEngagedOnEnemy(TeamInformation ti)
        {
            Dictionary<StaticObject, int> result = new Dictionary<StaticObject, int>();
            foreach (StaticObject enemy in ti.battleList.Values)
                if (!result.Keys.Contains(enemy))
                    result.Add(enemy, 1);
                else
                    result[enemy]++;
            return result;
        }
        private static PowerDataStructures.PriorityQueue<float, Fighter> getHealthiestFighters(TeamInformation ti, bool fighterMustBeIdle)
        {
            PowerDataStructures.PriorityQueue<float, Fighter> healthiestFighters = new PowerDataStructures.PriorityQueue<float,Fighter>(true);
            foreach (Fighter fi in ti.teamFighters)
                if (fighterMustBeIdle)
                {
                    if (!isFighterEngagedInBattle(ti,fi))
                        healthiestFighters.Add(new KeyValuePair<float,Fighter>(fi.getHealth,fi));
                }
                else
                    healthiestFighters.Add(new KeyValuePair<float,Fighter>(fi.getHealth,fi));

            return healthiestFighters;
        }
        private static TeamInformation getTeam(StaticObject ai)
        {
            foreach (TeamInformation ti in infoOnTeams)
                if (ai.Team == ti.teamId)
                    return ti;
            return null;
        }
        private static Node getRandomPatrolNode(TeamInformation ti)
        {
            if (ti.teamOwnedNodes.Count > 0)
                return ti.teamOwnedNodes.ElementAt(new Random().Next(0, ti.teamOwnedNodes.Count - 1));
            else return null;
        }
        private static void returnVictoriousFightersToPatrol()
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                List<Fighter> tupplesMarkedForRemoval = new List<Fighter>();
                foreach (Fighter fi in ti.battleList.Keys)
                if (ti.battleList[fi].getHealth <= 0)
                {
                    tupplesMarkedForRemoval.Add(fi);
                    NavigationComputer.setNewPathForRegisteredObject(fi, getRandomPatrolNode(ti), getRandomPatrolNode(ti));
                }
                foreach (Fighter fi in tupplesMarkedForRemoval)
                    ti.battleList.Remove(fi);
            }
        }
        private static bool isFighterEngagedInBattle(TeamInformation ti, Fighter fi)
        {
            return ti.battleList.Keys.Contains(fi);
        }
        private static bool isTargetAlreadyBattled(TeamInformation ti, StaticObject target)
        {
            return ti.battleList.Values.Contains(target);
        }
        private static bool isTargetMarkedForElimination(StaticObject target, TeamInformation ti)
        {
            foreach (System.Collections.Generic.KeyValuePair<int, BBN_Game.Objects.StaticObject> pair in ti.scrambleQueue)
                if (pair.Value == target)
                    return true;
            return false;
        }
        private static void ensureIdlingFightersAreActivelyPatrolling()
        {
            foreach (TeamInformation ti in infoOnTeams)
                foreach (Fighter fi in ti.teamFighters)
                   if (!isFighterEngagedInBattle(ti,fi)) 
                   {
                       List<Node> path = NavigationComputer.getPath(fi);
                       if (path == null)
                           NavigationComputer.setNewPathForRegisteredObject(fi, getRandomPatrolNode(ti), getRandomPatrolNode(ti));
                       else if (path.Count == 0)
                           NavigationComputer.setNewPathForRegisteredObject(fi, getRandomPatrolNode(ti), getRandomPatrolNode(ti));
                   }
        }
        private static void replenishFightersAndDestroyers()
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                if (!ti.fullyAIControlled)
                    continue;
                int numFightersToBuy = 0;
                int numDestroyersToBuy = 0;
                if (ti.maxFighters > ti.teamFighters.Count)
                {
                    if (ti.maxDestroyers <= ti.teamDestroyers.Count)
                        numFightersToBuy = (int)Math.Min(ti.teamCredits / TradingInformation.fighterCost,ti.maxFighters);
                    else
                    {
                        numFightersToBuy = (int)Math.Min((int)(ti.teamCredits * PERCENT_OF_CREDITS_TO_SPEND_ON_FIGHTERS_WHEN_SHORT_ON_BOTH / 
                            TradingInformation.fighterCost), ti.maxFighters);
                        numDestroyersToBuy = (int)Math.Min((int)(ti.teamCredits * PERCENT_OF_CREDITS_TO_SPEND_ON_DESTROYERS_WHEN_SHORT_ON_BOTH /
                            TradingInformation.destroyerCost), ti.maxDestroyers);
                    }
                }
                else if (ti.maxDestroyers > ti.teamDestroyers.Count)
                    numDestroyersToBuy = (int)Math.Min(ti.teamCredits / TradingInformation.destroyerCost,ti.maxDestroyers);
            }
        }

    }
}
