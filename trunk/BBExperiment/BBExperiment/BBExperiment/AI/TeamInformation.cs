using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.Objects;
namespace BBN_Game.AI
{
    /// <summary>
    /// Team information class
    /// Author: Benjamin Hugo
    /// </summary>
    class TeamInformation
    {
        public Team teamId { get; internal set; }
        public bool fullyAIControlled { get; internal set; }
        public int teamCredits { get; internal set; }
        public List<Turret> ownedTurrets { get; internal set; }
        public List<Fighter> teamFighters { get; internal set; }
        public List<Destroyer> teamDestroyers { get; internal set; }
        public Base teamBase { get; internal set; }
        public uint maxFighters { get; internal set; }
        public uint maxDestroyers { get; internal set; }
        public List<Node> teamOwnedNodes { get; internal set; }
        public List<SpawnPoint> teamSpawnPoints { get; internal set; }
        public playerObject teamPlayer { get; internal set; }
        public Dictionary<Fighter,StaticObject> fighterBattleList { get; internal set; }
        public Dictionary<Destroyer, StaticObject> destroyerBattleList { get; internal set; }
        public Dictionary<Turret, StaticObject> turretBattleList { get; internal set; }
        public StaticObject playerTarget { get; internal set; }
        public Turret playerObjective { get; internal set; }
        public PowerDataStructures.PriorityQueue<int, StaticObject> scrambleQueue { get; internal set; }
        public List<DynamicObject> spawnQueue { get; internal set; }
        internal Dictionary<StaticObject, int> gunsCoolDown { get; set; }
        /// <summary>
        /// Constructor for Team Information
        /// </summary>
        /// <param name="teamId">Instance of Team class, indicating the identifier for the TeamInformation instance (red/blue)</param>
        /// <param name="fullyAIControlled">Indicates whether the team is human controlled or not</param>
        /// <param name="ownedTurrets">Owned turrets at the start of the game</param>
        /// <param name="teamStartingCredits">Initial credits</param>
        /// <param name="teamPlayer">Team player object</param>
        /// <param name="ownedNodes">Team's patrol nodes</param>
        /// <param name="ownedSpawnPoints">Team's owned spawn points</param>
        /// <param name="maxFighters">Maximum number of fighters</param>
        /// <param name="maxDestroyers">Maximum number of destroyers</param>
        /// <param name="teamHomeBase">Team base instance</param>
        /// <param name="playerSpawnPt">Team's player's spawn point</param>
        public TeamInformation(Team teamId, bool fullyAIControlled, List<Turret> ownedTurrets,
            int teamStartingCredits, playerObject teamPlayer, List<Node> ownedNodes, List<SpawnPoint> ownedSpawnPoints, 
            uint maxFighters, uint maxDestroyers, Base teamHomeBase, PlayerSpawnPoint playerSpawnPt)
        {
            this.teamId = teamId;
            this.fullyAIControlled = fullyAIControlled;
            this.ownedTurrets = ownedTurrets;
            this.teamCredits = teamStartingCredits;
            this.teamPlayer = teamPlayer;
            this.teamOwnedNodes = ownedNodes;
            this.teamSpawnPoints = ownedSpawnPoints;
            this.maxDestroyers = maxDestroyers;
            this.maxFighters = maxFighters;
            this.teamFighters = new List<Fighter>((int)maxFighters);
            this.teamDestroyers = new List<Destroyer>((int)maxDestroyers);
            this.teamBase = teamHomeBase;
            
            scrambleQueue = new PowerDataStructures.PriorityQueue<int, StaticObject>(true);
            fighterBattleList = new Dictionary<Fighter, StaticObject>();
            destroyerBattleList = new Dictionary<Destroyer, StaticObject>();
            turretBattleList = new Dictionary<Turret, StaticObject>();
            gunsCoolDown = new Dictionary<StaticObject, int>();
            spawnQueue = new List<DynamicObject>(ownedSpawnPoints.Count);
            playerTarget = null;
            playerObjective = null;
        }
        /// <summary>
        /// Method to do garbage collection
        /// </summary>
        internal void garbageCollection()
        {
            for (int i = 0; i < teamFighters.Count; ++i)
                if (teamFighters.ElementAt(i).getHealth <= 0)
                    teamFighters.RemoveAt(i--);
            for (int i = 0; i < teamDestroyers.Count; ++i)
                if (teamDestroyers.ElementAt(i).getHealth <= 0)
                    teamDestroyers.RemoveAt(i--);
            for (int i = 0; i < ownedTurrets.Count; ++i)
                if (ownedTurrets.ElementAt(i).getHealth <= 0)
                    ownedTurrets.RemoveAt(i--);
            for (int i = 0; i < fighterBattleList.Keys.Count; ++i)
                if (fighterBattleList.Keys.ElementAt(i).getHealth <= 0)
                    fighterBattleList.Remove(fighterBattleList.Keys.ElementAt(i--));
            for (int i = 0; i < destroyerBattleList.Keys.Count; ++i)
                if (destroyerBattleList.Keys.ElementAt(i).getHealth <= 0)
                    destroyerBattleList.Remove(destroyerBattleList.Keys.ElementAt(i--));
            for (int i = 0; i < turretBattleList.Keys.Count; ++i)
                if (turretBattleList.Keys.ElementAt(i).getHealth <= 0)
                    turretBattleList.Remove(turretBattleList.Keys.ElementAt(i--));
            for (int i = 0; i < gunsCoolDown.Keys.Count; ++i)
                if (gunsCoolDown.Keys.ElementAt(i).getHealth <= 0)
                    gunsCoolDown.Remove(gunsCoolDown.Keys.ElementAt(i--));
        }
        /// <summary>
        /// Adds a new destroyer to the spawn queue (call this when a asset aquisition is made)
        /// </summary>
        /// <param name="ds">Instance of instantiated, registered destroyer</param>
        public void addNewDestroyerToTeam(Destroyer ds)
        {
            this.spawnQueue.Add(ds);
        }
        /// <summary>
        /// Adds a new fighter to the spawn queue (call this when a asset aquisition is made)
        /// </summary>
        /// <param name="fi">Instance of instantiated, registered fighter</param>
        public void addNewFighterToTeam(Fighter fi)
        {
            this.spawnQueue.Add(fi);
        }
    }
    
}
