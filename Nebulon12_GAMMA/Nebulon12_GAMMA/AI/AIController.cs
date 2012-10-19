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
        #region "Constants for unit"
        public const float PERCENT_OF_CREDITS_TO_SPEND_ON_FIGHTERS_WHEN_SHORT_ON_BOTH = 0.66f;
        public const float PERCENT_OF_CREDITS_TO_SPEND_ON_DESTROYERS_WHEN_SHORT_ON_BOTH = 0.33f;
        public const float DISTANCE_WHEN_TURRET_IS_CLOSE_TO_BASE = 200;
        public const int PRIORITY_FOR_ELIMINATING_PLAYER = 4;
        public const int PRIORITY_FOR_ELIMINATING_TURRET = 3;
        public const int PRIORITY_FOR_ELIMINATING_DESTROYER = 2;
        public const int PRIORITY_FOR_ELIMINATING_FIGHTER = 1;
        public const int PRIORITY_FOR_ELIMINATING_BASE = 5;
        public const int FIGHTERS_TO_SCRAMBLE_FOR_PLAYER = 6;
        public const int FIGHTERS_TO_SCRAMBLE_FOR_DESTROYER = 3;
        public const int FIGHTER_GUNS_COOLDOWN = 70;
        public const int DESTROYER_GUNS_COOLDOWN = 140;
        public const int PLAYER_GUNS_COOLDOWN = 50;
        public const int TURRET_GUNS_COOLDOWN = 240;
        public const float DETECTION_RADIUS = 250;
        public const float LINE_OF_SIGHT_CLOSE_DIST_MULTIPLYER = 2;
        #endregion
        #region "Variables for unit"
        private List<TeamInformation> infoOnTeams;
        private enum FighterState {FS_IDLE,FS_ENGAGED,FS_DONTCARE};
        private GridStructure spatialGrid;
        private NavigationComputer navComputer;
        private GameController gameController;
        private Random randomizer;
        private List<StaticObject> mapTurrets;
        private int instructionStep = 0;
        #endregion
        #region "Public methods"
        public AIController(GridStructure spatialGrid, NavigationComputer navComputer, GameController gameController, List<StaticObject> mapTurrets)
        {
            infoOnTeams = new List<TeamInformation>();
            this.spatialGrid = spatialGrid;
            this.navComputer = navComputer;
            this.gameController = gameController;
            this.mapTurrets = mapTurrets;
            randomizer = new Random();
        }
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
        public void registerTurretOnTeam(Turret turret, Team team)
        {
            if (team == Team.neutral)
                return;
            foreach (TeamInformation ti in infoOnTeams)
                if (ti.teamId == team)
                {
                    ti.ownedTurrets.Add(turret);
                    return;
                }
        }
        public void update(GameTime gameTime, Game game)
        {
            replenishFightersAndDestroyers(game);
            doSpawning();
            removeDestroyedTurretsFromTeamInfo();
            //Do a detection test to detect enemy, else make fighters and destroyers patrol
            if (instructionStep == 0)
            {
                this.looseEnemies();
                this.detectEnemyTargets();
            }
            //initiate or replenish battles:
            if (instructionStep == 5)
            {
                this.enlistFightersToTargetDetectedEnemy();
                this.topupAllBattles();
            }
            //make the fighters patrol or move to their targets:
            if (instructionStep == 10)
            {
                this.returnVictoriousFightersToPatrol();
                this.ensureIdlingFightersAreActivelyPatrolling();
                this.fightersEngageTargets();
                this.rotateFightersForAiming(gameTime);
            }
            //Make sure the destroyers are moving towards the enemy or sweeping round it at all times (return them from battle mode to idling if the won a battle)
            if (instructionStep == 5)
            {
                this.returnVictoriousDestroyersToDisengagedState();
                this.ensureIdlingDestroyersAreMovingTowardsEnemyBase();
            }
            //Make sure the player is either engaging or capturing turrets
            if (instructionStep == 0)
            {
                this.playerEngageTarget();
                this.returnVictoriousPlayerToDisengagedState();
                this.playersGoCaptureTurretsOrDestroyEnemyBase();
                this.rotatePlayerForAiming(gameTime);
            }
            if (instructionStep == 10)
            {
                //Return all victorius turrets to an inactive state:
                this.returnVictoriusTurretsToDisengagedState();
                //Do garbage collection:
                foreach (TeamInformation ti in infoOnTeams)
                    ti.garbageCollection();
            }
            //Execute shooting code:
            this.shootAtTargets();
            if (instructionStep == 10)
                instructionStep = 0;
            else
                instructionStep++;
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
        #endregion
        #region "Fighters battle control"
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
            }
        }
        private void topupAllBattles()
        {
            foreach (TeamInformation ti in infoOnTeams)
                for (int i = 0; i < ti.fighterBattleList.Values.Count; ++i )
                    topupAssignedFightersToBattle(ti, ti.fighterBattleList.Values.ElementAt(i));
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
                    if (ti.fighterBattleList[fi].getHealth <= 0 || (fi.Position - ti.fighterBattleList[fi].Position).Length() > DETECTION_RADIUS)
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
        private bool isTargetAlreadyBattledByFighters(TeamInformation ti, StaticObject target)
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
        #endregion
        #region "Detection Code For Fighters, Destroyers, Turrets & Players"
        public void registerHitOnBaseOrTurretOrFighters(StaticObject victim, StaticObject shooter)
        {
            TeamInformation tiV = getTeam(victim);
            TeamInformation tiS = getTeam(shooter);
            if (tiV == null || tiS == null) return;

            if (tiV != tiS)
                if (!isTargetMarkedForElimination(shooter, tiV))
                    if (!isTargetAlreadyBattledByFighters(tiV, shooter))
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
                                    targetQueue.Add(new KeyValuePair<float, DynamicObject>(this.getEliminationPriority(enemyDes) * 100 + dist, enemyDes));
                            }
                            float distToPlayer = (enemyTeam.teamPlayer.Position - homeFi.Position).Length();
                            if (distToPlayer <= DETECTION_RADIUS)
                                targetQueue.Add(new KeyValuePair<float, DynamicObject>(this.getEliminationPriority(enemyTeam.teamPlayer) * 100 + distToPlayer, enemyTeam.teamPlayer));
                            if (targetQueue.Count > 0)
                                if (!isTargetMarkedForElimination(targetQueue.PeekValue(), homeTeam))
                                    if (!isTargetAlreadyBattledByFighters(homeTeam, targetQueue.PeekValue()))
                                        homeTeam.scrambleQueue.Add(new KeyValuePair<int, StaticObject>(this.getEliminationPriority(targetQueue.PeekValue()), targetQueue.PeekValue()));

                        }
                        //Destroyer detects enemy? (player/turret/destroyer/fighter/base --- offensive measures):
                        foreach (Destroyer homeDs in homeTeam.teamDestroyers)
                        {
                            PowerDataStructures.PriorityQueue<float, StaticObject> targetQueue = new PowerDataStructures.PriorityQueue<float, StaticObject>(true);
                            foreach (Destroyer enemyDes in enemyTeam.teamDestroyers)
                            {
                                float dist = (enemyDes.Position - homeDs.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyDes) * 100 + dist, enemyDes));
                            }
                            float distToPlayer = (enemyTeam.teamPlayer.Position - homeDs.Position).Length();
                            if (distToPlayer <= DETECTION_RADIUS)
                                targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTeam.teamPlayer) * 100 + distToPlayer, enemyTeam.teamPlayer));
                            float distToBase = (enemyTeam.teamBase.Position - homeDs.Position).Length();
                            if (distToBase <= DETECTION_RADIUS)
                                targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTeam.teamBase) * 100 + distToPlayer, enemyTeam.teamBase));
                            foreach (Fighter enemyFi in enemyTeam.teamFighters)
                            {
                                float dist = (enemyFi.Position - homeDs.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyFi) * 100 + dist, enemyFi));
                            }
                            foreach (Turret enemyTurret in enemyTeam.ownedTurrets)
                            {
                                float dist = (enemyTurret.Position - homeDs.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTurret) * 100 + dist, enemyTurret));
                            }
                            if (targetQueue.Count > 0)
                                if (isDestroyerEngaged(homeTeam, homeDs))
                                    homeTeam.destroyerBattleList[homeDs] = targetQueue.PeekValue();
                                else
                                    homeTeam.destroyerBattleList.Add(homeDs, targetQueue.PeekValue());

                        }
                        //Turret detects enemy?
                        foreach (Turret homeTurret in homeTeam.ownedTurrets)
                        {
                            PowerDataStructures.PriorityQueue<float, StaticObject> targetQueue = new PowerDataStructures.PriorityQueue<float, StaticObject>(true);
                            foreach (Destroyer enemyDes in enemyTeam.teamDestroyers)
                            {
                                float dist = (enemyDes.Position - homeTurret.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyDes) * 100 + dist, enemyDes));
                            }
                            float distToPlayer = (enemyTeam.teamPlayer.Position - homeTurret.Position).Length();
                            if (distToPlayer <= DETECTION_RADIUS)
                                targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTeam.teamPlayer) * 100 + distToPlayer, enemyTeam.teamPlayer));
                            float distToBase = (enemyTeam.teamBase.Position - homeTurret.Position).Length();
                            if (distToBase <= DETECTION_RADIUS)
                                targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTeam.teamBase) * 100 + distToPlayer, enemyTeam.teamBase));
                            foreach (Fighter enemyFi in enemyTeam.teamFighters)
                            {
                                float dist = (enemyFi.Position - homeTurret.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyFi) * 100 + dist, enemyFi));
                            }
                            foreach (Turret enemyTurret in enemyTeam.ownedTurrets)
                            {
                                float dist = (enemyTurret.Position - homeTurret.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTurret) * 100 + dist, enemyTurret));
                            }
                            if (targetQueue.Count > 0)
                                if (homeTeam.turretBattleList.Keys.Contains(homeTurret))
                                    homeTeam.turretBattleList[homeTurret] = targetQueue.PeekValue();
                                else
                                    homeTeam.turretBattleList.Add(homeTurret, targetQueue.PeekValue());
                        }
                        //Player detects enemy?
                        {
                            PowerDataStructures.PriorityQueue<float, StaticObject> targetQueue = new PowerDataStructures.PriorityQueue<float, StaticObject>(true);
                            foreach (Destroyer enemyDes in enemyTeam.teamDestroyers)
                            {
                                float dist = (enemyDes.Position - homeTeam.teamPlayer.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyDes) * 100 + dist, enemyDes));
                            }
                            float distToPlayer = (enemyTeam.teamPlayer.Position - homeTeam.teamPlayer.Position).Length();
                            if (distToPlayer <= DETECTION_RADIUS)
                                targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTeam.teamPlayer) * 100 + distToPlayer, enemyTeam.teamPlayer));
                            float distToBase = (enemyTeam.teamBase.Position - homeTeam.teamPlayer.Position).Length();
                            if (distToBase <= DETECTION_RADIUS)
                                targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTeam.teamBase) * 100 + distToPlayer, enemyTeam.teamBase));
                            foreach (Fighter enemyFi in enemyTeam.teamFighters)
                            {
                                float dist = (enemyFi.Position - homeTeam.teamPlayer.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyFi) * 100 + dist, enemyFi));
                            }
                            foreach (Turret enemyTurret in enemyTeam.ownedTurrets)
                            {
                                float dist = (enemyTurret.Position - homeTeam.teamPlayer.Position).Length();
                                if (dist <= DETECTION_RADIUS)
                                    targetQueue.Add(new KeyValuePair<float, StaticObject>(this.getEliminationPriority(enemyTurret) * 100 + dist, enemyTurret));
                            }
                            if (targetQueue.Count > 0)
                                homeTeam.playerTarget = targetQueue.PeekValue();
                        }
                    }
        }
        private void looseEnemies()
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                List<StaticObject> removeQueue = new List<StaticObject>();
                foreach (Fighter fi in ti.fighterBattleList.Keys)
                    if ((fi.Position - ti.fighterBattleList[fi].Position).Length() > DETECTION_RADIUS)
                        removeQueue.Add(fi);
                foreach (StaticObject o in removeQueue)
                    ti.fighterBattleList.Remove(o as Fighter);
                removeQueue.Clear();
                foreach (Destroyer ds in ti.destroyerBattleList.Keys)
                    if ((ds.Position - ti.destroyerBattleList[ds].Position).Length() > DETECTION_RADIUS)
                        removeQueue.Add(ds);
                foreach (StaticObject o in removeQueue)
                    ti.destroyerBattleList.Remove(o as Destroyer);
                removeQueue.Clear();
                foreach (Turret tr in ti.turretBattleList.Keys)
                    if ((tr.Position - ti.turretBattleList[tr].Position).Length() > DETECTION_RADIUS)
                        removeQueue.Add(tr);
                foreach (StaticObject o in removeQueue)
                    ti.turretBattleList.Remove(o as Turret);
                removeQueue.Clear();
                if (ti.playerTarget != null)
                    if ((ti.teamPlayer.Position - ti.playerTarget.Position).Length() > DETECTION_RADIUS)
                        ti.playerTarget = null;
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
                            {
                                if (i + 1 < infoOnTeams.Count)
                                    eti = infoOnTeams.ElementAt(i + 1);
                                else if (i - 1 >= 0)
                                    eti = infoOnTeams.ElementAt(i - 1);
                            }
                            else eti = infoOnTeams.ElementAt(i);
                            if (eti != null)
                                randomEnemyNode = this.getRandomPatrolNode(eti);
                            navComputer.setNewPathForRegisteredObject(ds, closestNode, randomEnemyNode);
                        }
                    }
                }
        }
        #endregion
        #region "Destroyer Battle code"
        private bool isDestroyerEngaged(TeamInformation ti, Destroyer ds)
        {
            if (ti.destroyerBattleList.Keys.Contains(ds))
                return true;
            else
                return false;
        }
        private void returnVictoriousDestroyersToDisengagedState()
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                List<Destroyer> removalList = new List<Destroyer>();
                foreach (Destroyer ds in ti.destroyerBattleList.Keys)
                    if (ti.destroyerBattleList[ds].getHealth <= 0 || (ds.Position - ti.destroyerBattleList[ds].Position).Length() > DETECTION_RADIUS)
                        removalList.Add(ds);
                foreach (Destroyer ds in removalList)
                    ti.destroyerBattleList.Remove(ds);
            }           
        }
        #endregion
        #region "Helper code"
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
        #endregion
        #region "Fighters movement control"
        private void fightersEngageTargets()
        {
            foreach (TeamInformation ti in infoOnTeams)
                foreach (Fighter fi in ti.fighterBattleList.Keys)
                    navComputer.doDogfightMove(ti, fi, ti.fighterBattleList[fi]);
        }
        private void rotateFightersForAiming(GameTime gt)
        {
            foreach (TeamInformation ti in this.infoOnTeams)
                foreach (Fighter fi in ti.fighterBattleList.Keys)
                    if (this.isFighterEngagedInBattle(ti, fi))
                        if (navComputer.objectPaths[fi].currentWaypoint == null)
                        {
                            Vector3 vWantDir = Vector3.Zero; Vector3 vLookDir = Vector3.Zero;
                            navComputer.turnAI(ref vWantDir, ref vLookDir, fi, ti.fighterBattleList[fi].Position, gt);
                        }


        }
        private void ensureIdlingFightersAreActivelyPatrolling()
        {
            foreach (TeamInformation ti in infoOnTeams)
                foreach (Fighter fi in ti.teamFighters)
                    if (!isFighterEngagedInBattle(ti, fi))
                    {
                        List<Node> path = navComputer.getPath(fi);

                        Node randomStart = getRandomPatrolNode(ti);
                        Node randomEnd = getRandomPatrolNode(ti);
                        if (path == null)
                            navComputer.setNewPathForRegisteredObject(fi, randomStart, randomEnd);
                        else if (path.Count == 0)
                            navComputer.setNewPathForRegisteredObject(fi, randomStart, randomEnd);
                    }
        }
        #endregion
        #region "Player movement control"
        private void playerEngageTarget(){
            foreach (TeamInformation ti in infoOnTeams)
            {
                if (ti.fullyAIControlled)
                    if (ti.playerTarget != null)
                        navComputer.doDogfightMove(ti, ti.teamPlayer, ti.playerTarget);
            }
        }
        private void rotatePlayerForAiming(GameTime gt)
        {
            foreach (TeamInformation ti in this.infoOnTeams)
                if (ti.fullyAIControlled)
                    if (ti.playerTarget != null)
                        if (navComputer.objectPaths[ti.teamPlayer].currentWaypoint == null)
                        {
                            Vector3 vWantDir = Vector3.Zero; Vector3 vLookDir = Vector3.Zero;
                            navComputer.turnAI(ref vWantDir, ref vLookDir, ti.teamPlayer, ti.playerTarget.Position, gt);
                        }
        }
        #endregion
        #region "Player Battle / Objective Control"
        private void returnVictoriousPlayerToDisengagedState()
        {
            foreach (TeamInformation ti in infoOnTeams)
                if (ti.playerTarget != null)
                    if (ti.playerTarget.getHealth <= 0 || (ti.teamPlayer.Position - ti.playerTarget.Position).Length() > DETECTION_RADIUS)
                        ti.playerTarget = null;
        }
        private void playersGoCaptureTurretsOrDestroyEnemyBase()
        {
            foreach (TeamInformation ti in infoOnTeams)
                if (ti.fullyAIControlled)
                    if (ti.playerTarget == null)
                        if (ti.playerObjective == null)
                        {
                            if (this.mapTurrets.Count > 0)
                            {
                                Turret closestTurret = null;
                                float closestDistance = 0;
                                foreach (Turret tr in mapTurrets)
                                    if (tr.Team == Team.neutral && !tr.Repairing)
                                    {
                                        float dist = (tr.Position - ti.teamPlayer.Position).Length();
                                        if (closestTurret == null)
                                        {
                                            closestTurret = tr;
                                            closestDistance = dist;
                                        }
                                        else if (dist < closestDistance)
                                        {
                                            closestTurret = tr;
                                            closestDistance = dist;
                                        }
                                    }
                                if (closestTurret != null)
                                {
                                    routePlayerToTurret(ti,closestTurret);
                                }
                                else if (navComputer.objectPaths[ti.teamPlayer].currentWaypoint == null) //no turrets to capture at this time, go destroy the enemy base!
                                {
                                    int pickTeam = this.randomizer.Next(0, infoOnTeams.Count - 1);
                                    if (infoOnTeams.ElementAt(pickTeam) == ti)
                                    {
                                        if (pickTeam + 1 < infoOnTeams.Count)
                                            pickTeam++;
                                        else if (pickTeam - 1 >= 0)
                                            pickTeam--;
                                    }
                                    TeamInformation enemyTeam = infoOnTeams.ElementAt(pickTeam);
                                    List<GridObjectInterface> neighbouringObjects = this.spatialGrid.checkNeighbouringBlocks(enemyTeam.teamBase);
                                    Node closeWaypointToBase = null;
                                    foreach (GridObjectInterface obj in neighbouringObjects)
                                        if (obj is Node)
                                        {
                                            closeWaypointToBase = obj as Node;
                                            break;
                                        }
                                    Node closeWaypointToPlayer = null;
                                    foreach (GridObjectInterface obj in neighbouringObjects)
                                        if (obj is Node)
                                        {
                                            closeWaypointToPlayer = obj as Node;
                                            break;
                                        }
                                    if (!(closeWaypointToBase == null || closeWaypointToPlayer == null))
                                        navComputer.setNewPathForRegisteredObject(ti.teamPlayer, closeWaypointToPlayer, closeWaypointToBase);
                                    else
                                    {
                                        List<Node> path = new List<Node>();
                                        path.Add(new Node(enemyTeam.teamBase.Position + Vector3.Normalize(Matrix.CreateFromQuaternion(enemyTeam.teamBase.rotation).Forward) *
                                            (enemyTeam.teamBase.getGreatestLength + ti.teamPlayer.getGreatestLength) * LINE_OF_SIGHT_CLOSE_DIST_MULTIPLYER, -1));
                                        navComputer.objectPaths[ti.teamPlayer].remainingPath = path;
                                    }
                                }
                            }
                        }
                        else //we are enroute to capture the next turret
                        {
                            if (navComputer.objectPaths[ti.teamPlayer].currentWaypoint == null)
                                routePlayerToTurret(ti, ti.playerObjective);
                            if (ti.playerObjective.Team != Team.neutral)
                            {
                                ti.playerObjective = null;                //the other player captured our objective....... dang! NEXT!
                                continue;
                            }
                            else if ((ti.playerObjective.Position - ti.teamPlayer.Position).Length() <= GridDataCollection.MAX_CAPTURE_DISTANCE)
                            {
                                GridDataCollection.tryCaptureTower(ti.teamPlayer);
                                ti.playerObjective = null;
                            }
                        }
        }
        private void routePlayerToTurret(TeamInformation ti, Turret turret)
        {
            ti.playerObjective = turret;
            List<GridObjectInterface> neighbouringObjects = this.spatialGrid.checkNeighbouringBlocks(turret);
            Node closeWaypointToTurret = null;
            foreach (GridObjectInterface obj in neighbouringObjects)
                if (obj is Node && (obj.Position - ti.playerObjective.Position).Length() <= GridDataCollection.MAX_CAPTURE_DISTANCE)
                {
                    closeWaypointToTurret = obj as Node;
                    break;
                }
            neighbouringObjects = this.spatialGrid.checkNeighbouringBlocks(ti.teamPlayer);
            Node closeWaypointToPlayer = null;
            float minDist = 0;
            foreach (GridObjectInterface obj in neighbouringObjects)
                if (obj is Node)
                {
                    float dist = (obj.Position - ti.teamPlayer.Position).Length();
                    if (closeWaypointToPlayer == null || minDist > dist)
                    { 
                        closeWaypointToPlayer = obj as Node;
                        minDist = dist;
                    }
                }
            if (!(closeWaypointToTurret == null || closeWaypointToPlayer == null))
                navComputer.setNewPathForRegisteredObject(ti.teamPlayer, closeWaypointToPlayer, closeWaypointToTurret);
            else
            {
                List<Node> path = new List<Node>();
                path.Add(new Node(ti.playerObjective.Position + Vector3.Normalize(Matrix.CreateFromQuaternion(ti.playerObjective.rotation).Forward) *
                    (ti.playerObjective.getGreatestLength + ti.teamPlayer.getGreatestLength) * LINE_OF_SIGHT_CLOSE_DIST_MULTIPLYER, -1));
                navComputer.objectPaths[ti.teamPlayer].remainingPath = path;
            }
        }
        #endregion
        #region "Spawning Code"
        private void replenishFightersAndDestroyers(Game game)
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                if (!ti.fullyAIControlled)
                    continue;
                int numFightersToBuy = 0;
                int numDestroyersToBuy = 0;
                int numberOfFightersOnSpawnList = 0;
                int numberOfDestroyersOnSpawnList = 0;
                foreach (DynamicObject o in ti.spawnQueue)
                    if (o is Fighter)
                        numberOfFightersOnSpawnList++;
                    else if (o is Destroyer)
                        numberOfDestroyersOnSpawnList++;
                if (ti.maxFighters > ti.teamFighters.Count)
                {
                    if (ti.maxDestroyers <= ti.teamDestroyers.Count)
                        numFightersToBuy = (int)Math.Min(ti.teamCredits / TradingInformation.fighterCost, ti.maxFighters - ti.teamFighters.Count - numberOfFightersOnSpawnList);
                    else
                    {
                        numFightersToBuy = (int)Math.Min((int)(ti.teamCredits * PERCENT_OF_CREDITS_TO_SPEND_ON_FIGHTERS_WHEN_SHORT_ON_BOTH /
                            TradingInformation.fighterCost), ti.maxFighters - ti.teamFighters.Count - numberOfFightersOnSpawnList);
                        numDestroyersToBuy = (int)Math.Min((int)(ti.teamCredits * PERCENT_OF_CREDITS_TO_SPEND_ON_DESTROYERS_WHEN_SHORT_ON_BOTH /
                            TradingInformation.destroyerCost), ti.maxDestroyers - ti.teamDestroyers.Count - numberOfDestroyersOnSpawnList);
                    }
                }
                else if (ti.maxDestroyers > ti.teamDestroyers.Count)
                    numDestroyersToBuy = (int)Math.Min(ti.teamCredits / TradingInformation.destroyerCost,
                        ti.maxDestroyers - ti.teamDestroyers.Count - numberOfDestroyersOnSpawnList);
                for (int i = 0; i < numDestroyersToBuy; ++i)
                {
                    Destroyer d = new Destroyer(game, ti.teamId, Vector3.Zero);
                    d.Initialize();
                    d.LoadContent();
                    ti.spawnQueue.Add(d);
                    ti.teamCredits -= TradingInformation.destroyerCost;
                }
                for (int i = 0; i < numFightersToBuy; ++i)
                {
                    Fighter f = new Fighter(game, ti.teamId, Vector3.Zero);
                    f.Initialize();
                    f.LoadContent();
                    ti.spawnQueue.Add(f);
                    ti.teamCredits -= TradingInformation.fighterCost;
                }
            }
        }
        private void doSpawning()
        {
            foreach (TeamInformation ti in this.infoOnTeams)
            {
                //Spawn fighters and destroyers
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
                                if (obj is Fighter)
                                    ti.teamFighters.Add(obj as Fighter);
                                else if (obj is Destroyer)
                                    ti.teamDestroyers.Add(obj as Destroyer);
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
                //Spawn player if he is dead!
                if (ti.teamPlayer.getHealth <= 0)
                {
                    //ti.teamPlayer.killObject();
                    Controller.GameController.removeObject(ti.teamPlayer);
                    ti.teamPlayer = Controller.GameController.spawnPlayer(ti.teamId, ti.teamPlayer.Game);
                    ti.playerObjective = null;
                    ti.playerTarget = null;
                }
                
                if (ti.fullyAIControlled)
                    if (!navComputer.isObjectRegistered(ti.teamPlayer))
                        navComputer.registerObject(ti.teamPlayer);
            }
        }
        #endregion
        #region "turret code"
        private void removeDestroyedTurretsFromTeamInfo()
        {
            foreach (TeamInformation ti in this.infoOnTeams)
            {
                List<Turret> removalList = new List<Turret>();
                foreach (Turret t in ti.ownedTurrets)
                    if (t.getHealth <= 0 || t.Team != ti.teamId)
                        removalList.Add(t);
                foreach (Turret t in removalList)
                {
                    ti.ownedTurrets.Remove(t);
                    if (ti.turretBattleList.Keys.Contains(t))
                        ti.turretBattleList.Remove(t);
                }
            }
        }
        private void returnVictoriusTurretsToDisengagedState()
        {
            foreach (TeamInformation ti in infoOnTeams)
            {
                List<Turret> removalList = new List<Turret>();
                foreach (Turret t in ti.turretBattleList.Keys)
                    if (ti.turretBattleList[t].getHealth <= 0 || (t.Position - ti.turretBattleList[t].Position).Length() > DETECTION_RADIUS)
                        removalList.Add(t);
                foreach (Turret t in removalList)
                    ti.turretBattleList.Remove(t);
            }
        }

        #endregion
        #region "shooting code"
        private void shootAtTargets()
        {
            foreach (TeamInformation ti in this.infoOnTeams)
            {
                //Make the fighters fire bullets at their enemies:
                foreach (Fighter fi in ti.fighterBattleList.Keys)
                {   
                    if (ti.gunsCoolDown.Keys.Contains(fi))
                    {
                        if (ti.gunsCoolDown[fi]-- > 0)
                            continue;
                        else if (Vector3.Dot(Vector3.Normalize(Matrix.CreateFromQuaternion(fi.rotation).Forward), 
                            Vector3.Normalize(ti.fighterBattleList[fi].Position - fi.Position)) < -0.5f)
                        {
                            ti.gunsCoolDown[fi] = FIGHTER_GUNS_COOLDOWN;
                            Controller.GameController.addObject(new Objects.Bullet(fi.Game, ti.fighterBattleList[fi], fi));
                        }
                    }
                    else if (Vector3.Dot(Vector3.Normalize(Matrix.CreateFromQuaternion(fi.rotation).Forward),
                            Vector3.Normalize(ti.fighterBattleList[fi].Position - fi.Position)) < -0.5f)
                    {
                        Controller.GameController.addObject(new Objects.Bullet(fi.Game, ti.fighterBattleList[fi], fi));
                        ti.gunsCoolDown.Add(fi, FIGHTER_GUNS_COOLDOWN);
                    }
                }
                //Make the destroyers fire missiles at their enemies:
                foreach (Destroyer ds in ti.destroyerBattleList.Keys)
                {
                    if (ti.gunsCoolDown.Keys.Contains(ds))
                    {
                        if (ti.gunsCoolDown[ds]-- > 0)
                            continue;
                        else 
                        {
                            ti.gunsCoolDown[ds] = DESTROYER_GUNS_COOLDOWN;
                            Controller.GameController.addObject(new Objects.Missile(ds.Game, ti.destroyerBattleList[ds], ds));
                        }
                    }
                    else 
                    {
                        Controller.GameController.addObject(new Objects.Missile(ds.Game, ti.destroyerBattleList[ds], ds));
                        ti.gunsCoolDown.Add(ds, DESTROYER_GUNS_COOLDOWN);
                    }
                }
                //Make the turrets fire missiles at their enemies:
                foreach (Turret tr in ti.turretBattleList.Keys)
                {
                    Vector3 PYR = MathEuler.AngleTo(ti.turretBattleList[tr].Position, tr.Position);

                    tr.rotation = Quaternion.CreateFromYawPitchRoll(PYR.Y, PYR.X, PYR.Z);

                    if (ti.gunsCoolDown.Keys.Contains(tr))
                    {
                        if (ti.gunsCoolDown[tr]-- > 0)
                            continue;
                        else
                        {
                            ti.gunsCoolDown[tr] = TURRET_GUNS_COOLDOWN;
                            Controller.GameController.addObject(new Objects.Missile(tr.Game, ti.turretBattleList[tr], tr));
                        }
                    }
                    else
                    {
                        Controller.GameController.addObject(new Objects.Missile(tr.Game, ti.turretBattleList[tr], tr));
                        ti.gunsCoolDown.Add(tr, TURRET_GUNS_COOLDOWN);
                    }
                }
                //Make the player fire bullets at his enemy:
                if (ti.fullyAIControlled)
                {
                    if (ti.playerTarget != null)
                    {
                        if (ti.gunsCoolDown.Keys.Contains(ti.teamPlayer))
                        {
                            if (ti.gunsCoolDown[ti.teamPlayer]-- > 0)
                                continue;
                            else
                            {
                                ti.gunsCoolDown[ti.teamPlayer] = PLAYER_GUNS_COOLDOWN;
                                if (randomizer.NextDouble() < 0.3f)
                                    Controller.GameController.addObject(new Objects.Missile(ti.teamPlayer.Game, ti.playerTarget, ti.teamPlayer));
                                else
                                    Controller.GameController.addObject(new Objects.Bullet(ti.teamPlayer.Game, ti.playerTarget, ti.teamPlayer));
                            }
                        }
                        else
                        {
                            Controller.GameController.addObject(new Objects.Missile(ti.teamPlayer.Game, ti.playerTarget, ti.teamPlayer));
                            ti.gunsCoolDown.Add(ti.teamPlayer, PLAYER_GUNS_COOLDOWN);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
