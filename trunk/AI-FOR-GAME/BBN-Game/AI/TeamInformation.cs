using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.Objects;

namespace BBN_Game.AI
{
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
        public PowerDataStructures.PriorityQueue<int, StaticObject> scrambleQueue { get; internal set; }
        public StaticObject PlayerTarget { get; internal set; }
        public List<DynamicObject> spawnQueue { get; internal set; }

        public TeamInformation(Team teamId, bool fullyAIControlled, List<Turret> ownedTurrets,
            int teamStartingCredits, playerObject teamPlayer, List<Node> ownedNodes, List<SpawnPoint> ownedSpawnPoints, 
            uint maxFighters, uint maxDestroyers, Base teamHomeBase)
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
            spawnQueue = new List<DynamicObject>(ownedSpawnPoints.Count);
        }
    }
}
