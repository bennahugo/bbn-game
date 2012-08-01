using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.Objects;
using Microsoft.Xna.Framework;
using BBN_Game.Controller;
using BBN_Game.Grid;

namespace BBN_Game.AI
{
    /// <summary>
    /// Controller class for AI movement and battle coordination 
    /// Author: Benjamin Hugo
    /// </summary>
        
    class AIController
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
        public const float DETECTION_RADIUS = 70;
        private List<TeamInformation> infoOnTeams;
        private enum FighterState {FS_IDLE,FS_ENGAGED,FS_DONTCARE};
        private GridStructure spatialGrid;
        private NavigationComputer navComputer;
        private GameController gameController;
        private Random randomizer;
        #region "Public methods"
        public AIController(GridStructure spatialGrid, NavigationComputer navComputer, GameController gameController)
        {
            infoOnTeams = new List<TeamInformation>();
            this.spatialGrid = spatialGrid;
            this.navComputer = navComputer;
            this.gameController = gameController;
            randomizer = new Random();
        }
        #region "Team Information"
        public void registerTeam(TeamInformation ti)
        {
            if (!infoOnTeams.Contains(ti))
                infoOnTeams.Add(ti);
        }
        public int getTeamCount()
        {
            return infoOnTeams.Count;
        }
        public TeamInformation getTeam(int index)
        {
            return infoOnTeams.ElementAt(index);
        }
        #endregion
        
        public int getEliminationPriority(StaticObject target)
        {
            if (target is playerObject)
                return PRIORITY_FOR_ELIMINATING_PLAYER;
            else if (target is Turret)
                return PRIORITY_FOR_ELIMINATING_TURRET;
            else if (target is Destroyer)
                return PRIORITY_FOR_ELIMINATING_DESTROYER;
            else if (target is Fighter)
                return PRIORITY_FOR_ELIMINATING_FIGHTER;
            else if (target is Base)
                return PRIORITY_FOR_ELIMINATING_BASE;
            else return 0;
        }
        
        public void update(GameTime gameTime, Game game)
        {
            replenishFightersAndDestroyers(game);
            doSpawning();
            this.ensureIdlingFightersAreActivelyPatrolling();
            this.ensureIdlingDestroyersAreMovingTowardsEnemyBase();
        }
        #endregion
        #region "Fighters update methods"
        private void enlistFightersToTargetDetectedEnemy()
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                List<KeyValuePair<int,StaticObject>> enemyToReadd = new List<KeyValuePair<int,StaticObject>>();
                while (ti.scrambleQueue.Count > 0)
                {
                    //Calculate the number of fighters that need to be scrambled
                    StaticObject enemy = ti.scrambleQueue.PeekValue();
                    if (topupAssignedFightersToBattle(ti, enemy) == 0)
                        enemyToReadd.Add(ti.scrambleQueue.Dequeue());
                    else
                        ti.scrambleQueue.Dequeue();
                }
                foreach (KeyValuePair<int,StaticObject> enemy in enemyToReadd)
                    ti.scrambleQueue.Add(enemy);
                Node n = new Node();
            }
        }
        private void topupAllBattles()
        {
            foreach (TeamInformation ti in infoOnTeams)
                foreach (StaticObject enemy in ti.fighterBattleList.Values)
                    topupAssignedFightersToBattle(ti, enemy);
        }
        private int topupAssignedFightersToBattle(TeamInformation ti, StaticObject enemy)
        {
                    Dictionary<StaticObject, int> numEngagedFightersPerEnemy = countFightersEngagedOnEnemy(ti);
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
                        PowerDataStructures.PriorityQueue<float, Fighter> healthiestInactiveFighters = getHealthiestFighters(ti, FighterState.FS_IDLE);
                        int numIdleAvailable = healthiestInactiveFighters.Count;
                        //when we have enough fighters available just scramble them
                        if (numIdleAvailable >= numToScramble)
                        {
                            for (int i = 0; i < numToScramble; ++i)
                                ti.fighterBattleList.Add(healthiestInactiveFighters.DequeueValue(), enemy);
                            return numIdleAvailable;
                        }
                        //when we have too few fighters reassign them to more important targets
                        else
                        {
                            //add what we do have:
                            for (int i = 0; i < numIdleAvailable; ++i)
                                ti.fighterBattleList.Add(healthiestInactiveFighters.DequeueValue(), enemy);
                            //now find more fighters and reassign:
                            PowerDataStructures.PriorityQueue<float, Fighter> healthiestActiveFighters = getHealthiestFighters(ti, FighterState.FS_ENGAGED);
                            int numReassigned = 0;
                            foreach (KeyValuePair<float,Fighter> fighter in healthiestActiveFighters)
                                if (getEliminationPriority(ti.fighterBattleList[fighter.Value]) < getEliminationPriority(enemy))
                                {
                                    ti.fighterBattleList.Remove(fighter.Value);
                                    ti.fighterBattleList.Add(fighter.Value, enemy);
                                    if (numIdleAvailable+(++numReassigned) == numToScramble)
                                        break;
                                }
                            return numReassigned+numIdleAvailable;
                        }
                    }
                    else return 0;
        }
        private Dictionary<StaticObject, int> countFightersEngagedOnEnemy(TeamInformation ti)
        {
            Dictionary<StaticObject, int> result = new Dictionary<StaticObject, int>();
            foreach (StaticObject enemy in ti.fighterBattleList.Values)
                if (!result.Keys.Contains(enemy))
                    result.Add(enemy, 1);
                else
                    result[enemy]++;
            return result;
        }
        private PowerDataStructures.PriorityQueue<float, Fighter> getHealthiestFighters(TeamInformation ti, FighterState requiredFighterState)
        {
            PowerDataStructures.PriorityQueue<float, Fighter> healthiestFighters = new PowerDataStructures.PriorityQueue<float,Fighter>(true);
            foreach (Fighter fi in ti.teamFighters)
                if (requiredFighterState == FighterState.FS_IDLE)
                {
                    if (!isFighterEngagedInBattle(ti,fi))
                        healthiestFighters.Add(new KeyValuePair<float,Fighter>(fi.getHealth,fi));
                }
                else if (requiredFighterState == FighterState.FS_ENGAGED)
                {
                    if (!isFighterEngagedInBattle(ti, fi))
                        healthiestFighters.Add(new KeyValuePair<float, Fighter>(fi.getHealth, fi));
                }
                else if (requiredFighterState == FighterState.FS_DONTCARE)
                    healthiestFighters.Add(new KeyValuePair<float, Fighter>(fi.getHealth, fi));

            return healthiestFighters;
        }
        private void returnVictoriousFightersToPatrol()
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                List<Fighter> tupplesMarkedForRemoval = new List<Fighter>();
                foreach (Fighter fi in ti.fighterBattleList.Keys)
                    if (ti.fighterBattleList[fi].getHealth <= 0)
                    {
                        tupplesMarkedForRemoval.Add(fi);
                        navComputer.setNewPathForRegisteredObject(fi, getRandomPatrolNode(ti), getRandomPatrolNode(ti));
                    }
                foreach (Fighter fi in tupplesMarkedForRemoval)
                    ti.fighterBattleList.Remove(fi);
            }
        }
        private bool isFighterEngagedInBattle(TeamInformation ti, Fighter fi)
        {
            return ti.fighterBattleList.Keys.Contains(fi);
        }
        private bool isTargetAlreadyBattled(TeamInformation ti, StaticObject target)
        {
            return ti.fighterBattleList.Values.Contains(target);
        }
        private bool isTargetMarkedForElimination(StaticObject target, TeamInformation ti)
        {
            foreach (System.Collections.Generic.KeyValuePair<int, BBN_Game.Objects.StaticObject> pair in ti.scrambleQueue)
                if (pair.Value == target)
                    return true;
            return false;
        }
        private void ensureIdlingFightersAreActivelyPatrolling()
        {
            foreach (TeamInformation ti in infoOnTeams)
                foreach (Fighter fi in ti.teamFighters)
                    if (!isFighterEngagedInBattle(ti, fi))
                    {
                        List<Node> path = navComputer.getPath(fi);
                        spatialGrid.checkNeighbouringBlocks(fi);

                        Node randomStart = getRandomPatrolNode(ti);
                        Node randomEnd = getRandomPatrolNode(ti);
                        if (path == null)
                            navComputer.setNewPathForRegisteredObject(fi, randomStart, randomEnd);
                        else if (path.Count == 0)
                            navComputer.setNewPathForRegisteredObject(fi, randomStart, randomEnd);
                    }
        }
        #endregion
        #region "Detection Code For Fighters & Destroyers"
        public void registerHitOnBaseOrTurretOrFighters(StaticObject victim, StaticObject shooter)
        {
            TeamInformation tiV = getTeam(victim);
            TeamInformation tiS = getTeam(shooter);
            if (tiV == null || tiS == null) return;

            if (tiV != tiS)
                if (!isTargetMarkedForElimination(shooter, tiV))
                    if (!isTargetAlreadyBattled(tiV, shooter))
                    {
                        if (shooter is playerObject)
                        {
                            if (victim is Fighter || victim is Base)
                                tiV.scrambleQueue.Add(new KeyValuePair<int, StaticObject>(PRIORITY_FOR_ELIMINATING_PLAYER, shooter));
                            else if (victim is Turret)
                            {
                                float distanceFromTurretToHomeBase = (victim.Position - tiV.teamBase.Position).Length();
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
                                float distanceFromTurretToHomeBase = (victim.Position - tiV.teamBase.Position).Length();
                                if (distanceFromTurretToHomeBase <= DISTANCE_WHEN_TURRET_IS_CLOSE_TO_BASE)
                                    tiV.scrambleQueue.Add(new KeyValuePair<int, StaticObject>(PRIORITY_FOR_ELIMINATING_DESTROYER, shooter));
                            }
                        }
                    }
        }

        private void detectEnemyTargets()
        {
            foreach (TeamInformation homeTeam in infoOnTeams)
                foreach (TeamInformation enemyTeam in infoOnTeams)
                    if (homeTeam != enemyTeam)
                    {
                        //Fighter detects enemy? (Fighters only concerned with enemy player and destroyers --- defensive measures):
                        foreach (Fighter homeFi in homeTeam.teamFighters)
                        {
                            PowerDataStructures.PriorityQueue<float, DynamicObject> targetQueue = new PowerDataStructures.PriorityQueue<float, DynamicObject>(true);
                            foreach (Destroyer enemyDes in enemyTeam.teamDestroyers)
                            {
                                float dist = (enemyDes.Position - homeFi.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, DynamicObject>(this.getEliminationPriority(enemyDes)*100+dist, enemyDes));
                            }
                            float distToPlayer = (enemyTeam.teamPlayer.Position - homeFi.Position).Length();
                            if (distToPlayer <= DETECTION_RADIUS)
                                targetQueue.Add(new KeyValuePair<float, DynamicObject>(this.getEliminationPriority(enemyTeam.teamPlayer) * 100 + distToPlayer, enemyTeam.teamPlayer));
                            if (targetQueue.Count > 0)
                                if (!isTargetMarkedForElimination(targetQueue.PeekValue(), homeTeam))
                                    if (!isTargetAlreadyBattled(homeTeam,targetQueue.PeekValue()))
                                        homeTeam.scrambleQueue.Add(new KeyValuePair<int, StaticObject>(this.getEliminationPriority(targetQueue.PeekValue()), targetQueue.PeekValue()));

                        }
                        //Destroyer detects enemy? (player/turret/destroyer/fighter/base --- offensive measures):

                    }
        }
        #endregion
        #region "Destroyer navigation code"
        private void ensureIdlingDestroyersAreMovingTowardsEnemyBase()
        {
            foreach (TeamInformation ti in infoOnTeams)
                foreach (Destroyer ds in ti.teamDestroyers)
                {
                    if (navComputer.getPath(ds) != null)
                        if (navComputer.getPath(ds).Count != 0) continue;
                    if (isDestroyerEngaged(ti, ds)) continue;
                    List<GridObjectInterface> vacinity = spatialGrid.checkNeighbouringBlocks(ds);
                    if (vacinity != null)
                    {
                        Node closestNode = null;
                        Node randomEnemyNode = null;
                        foreach (GridObjectInterface goi in vacinity)
                            if (goi is Node)
                                closestNode = (closestNode != null) ? 
                                                ((goi.Position-ds.Position).Length() < (closestNode.Position-ds.Position).Length()) ? 
                                                    goi as Node : closestNode 
                                              : goi as Node;
                        if (closestNode != null)
                        {                            
                            int i = randomizer.Next(0,infoOnTeams.Count() - 1);
                            TeamInformation eti = null;
                            if (infoOnTeams.ElementAt(i) == ti)
                                if (i + 1 < infoOnTeams.Count)
                                    eti = infoOnTeams.ElementAt(i + 1);
                                else if (i - 1 >= 0)
                                    eti = infoOnTeams.ElementAt(i - 1);
                            if (eti != null)
                                randomEnemyNode = this.getRandomPatrolNode(eti);
                            navComputer.setNewPathForRegisteredObject(ds, closestNode, randomEnemyNode);
                        }
                    }
                }
        }
        private bool isDestroyerEngaged(TeamInformation ti, Destroyer ds)
        {
            if (ti.destroyerBattleList.Keys.Contains(ds))
                return true;
            else
                return false;
        }
        #endregion
        private TeamInformation getTeam(StaticObject ai)
        {
            foreach (TeamInformation ti in infoOnTeams)
                if (ai.Team == ti.teamId)
                    return ti;
            return null;
        }
        private Node getRandomPatrolNode(TeamInformation ti)
        {
            if (ti.teamOwnedNodes.Count > 0)
                return ti.teamOwnedNodes.ElementAt(randomizer.Next(0, ti.teamOwnedNodes.Count - 1));
            else return null;
        }
        #region "Spawning Code"
        private void replenishFightersAndDestroyers(Game game)
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
                for (int i = 0; i < numDestroyersToBuy; ++i)
                {
                    Destroyer d = new Destroyer(game, ti.teamId, Vector3.Zero);
                    d.Initialize();
                    d.LoadContent();
                    ti.spawnQueue.Add(d);
                    ti.teamDestroyers.Add(d);
                    ti.teamCredits -= TradingInformation.destroyerCost;
                }
                for (int i = 0; i < numFightersToBuy; ++i)
                {
                    Fighter f = new Fighter(game, ti.teamId, Vector3.Zero);
                    f.Initialize();
                    f.LoadContent();
                    ti.spawnQueue.Add(f);
                    ti.teamFighters.Add(f);
                    ti.teamCredits -= TradingInformation.fighterCost;
                }
            }
        }
        private void doSpawning()
        {
            foreach (TeamInformation ti in this.infoOnTeams)
            {
                List<DynamicObject> spawnedList = new List<DynamicObject>();
                foreach (DynamicObject obj in ti.spawnQueue)                
                    foreach (SpawnPoint sp in ti.teamSpawnPoints)
                    {
                        List < GridObjectInterface > vacinity = this.spatialGrid.checkNeighbouringBlocks(sp.Position);
                        bool bCanSpawn = true;
                        obj.Position = sp.Position;
                        navComputer.registerObject(obj);
                        if (vacinity != null)
                        {
                            foreach (GridObjectInterface nearbyObject in vacinity)
                                if (!(nearbyObject is Marker))
                                {
                                    BoundingSphere sphere1 = nearbyObject.getBoundingSphere();
                                    BoundingSphere sphere2 = obj.getBoundingSphere();
                                    if ((nearbyObject.Position - obj.Position).Length() < (sphere1.Radius + sphere2.Radius) * 2)
                                    {
                                        bCanSpawn = false;
                                        break;
                                    }
                                }
                            if (bCanSpawn)
                            {
                                spawnedList.Add(obj);
                                GameController.addObject(obj);
                                obj.Position = sp.Position;
                                spatialGrid.registerObject(obj);
                                break;
                            }
                        }
                    }
                foreach (DynamicObject removal in spawnedList)
                    ti.spawnQueue.Remove(removal);
            }
        }
        #endregion
    }
}
