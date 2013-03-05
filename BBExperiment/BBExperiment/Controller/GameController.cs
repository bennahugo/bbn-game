using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BBN_Game.Objects;

namespace BBN_Game.Controller
{

    enum GameState
    {
        Playing = 2
    }

    class GameController
    {
        #region "Constants"
        private string INITIAL_MAP = BBNGame.mode.Equals(BBNGame.ExperimentMode.RealThing) ? "Content/Maps/CheckPointRace.xml" : "Content/Maps/Practice.xml";
        private const int GRID_CUBE_SIZE = 60;
        public const int MAX_NUM_FIGHTERS_PER_TEAM = 4;
        public const int MAX_NUM_DESTROYERS_PER_TEAM = 6;
        private const float COLLISION_SPEED_PRESERVATION = 0.025f;
        private const float YAW_PITCH_ROLL_SPEED_FACTOR_FOR_AI_PLAYER = 0.0166f;
        //private const float DETAIL_CULL_DISTANCE = 600;
        //private const float HUD_DETAIL_CULL_DISTANCE = 1000;
        #endregion

        #region "Object holders"
        static List<Objects.StaticObject> AllObjects, DynamicObjs, Asteroids, Projectiles, Checkpoints;
        
        static Objects.playerObject Player1;

        public static List<Objects.StaticObject> getAllObjects
        {
            get { return AllObjects; }
        }
        public static List<Objects.StaticObject> DynamicObjects
        {
            get { return DynamicObjs; }
        }
        public static List<Objects.StaticObject> CheckPoints
        {
            get { return Checkpoints; }
        }
        #endregion

        #region "Graphics Devices"
        Graphics.Skybox.Skybox SkyBox;
        #endregion

        #region "Game Controllers"
        GameState gameState, prevGameState;
        public static string currentMap { get; private set; }
        public static float mapRadius { get; internal set; }
        public static float skyboxRepeat { get; internal set; }
        public static String skyboxTexture { get; internal set; }
        public GameState CurrentGameState
        {
            get { return gameState; }
            set { gameState = value; }
        }
        public GameState PreviousState
        {
            get { return prevGameState; }
            set { prevGameState = value; }
        }
        
        static Grid.GridStructure gameGrid;
        
        //Controller objects:
        
        //public static ParticleEngine.ParticleController particleController;

        public static Grid.GridStructure Grid
        {
            get { return gameGrid; }
        }
        #endregion

        #region "Global Data Holders"
        public static Viewport Origional;
        BBN_Game.BBNGame game;
        static int i;

        public static Boolean ObjectsLoaded;

        protected static Song imperial, beatit;
        protected static SoundEffect laugh1, laugh2, explosion;
        private Texture2D loadTex;
        private Texture2D loadNarative;
        private Texture2D btnA;
        private SpriteFont f;

        public static List<String> Team1Gold, Team2Gold;
        protected float stringCounterT1 = 2, stringCounterT2 = 2;
        #endregion

        #region "XNA Required"
        public GameController(BBN_Game.BBNGame game)
        {
            this.game = game;

            // Set up the Variables
            gameState = GameState.Playing;
            prevGameState = GameState.Playing;
            ObjectsLoaded = false;
        }

        public void Initialize()
        {
            #region "Lists"
            AllObjects = new List<BBN_Game.Objects.StaticObject>();
            Asteroids = new List<BBN_Game.Objects.StaticObject>();
            Projectiles = new List<BBN_Game.Objects.StaticObject>();
            DynamicObjs = new List<BBN_Game.Objects.StaticObject>();
            Checkpoints = new List<BBN_Game.Objects.StaticObject>();
            #endregion

            #region "Viewport setting"
            game.Graphics.PreferredBackBufferWidth = game.Graphics.GraphicsDevice.DisplayMode.Width;
            game.Graphics.PreferredBackBufferHeight = game.Graphics.GraphicsDevice.DisplayMode.Height;
            //game.Graphics.PreferredBackBufferWidth = 1920;
            //game.Graphics.PreferredBackBufferHeight = 1080;
            //game.Graphics.IsFullScreen = true;
            game.Graphics.ApplyChanges();

            Origional = game.GraphicsDevice.Viewport;
            #endregion

            #region "Controller initialization"
            
            #endregion
        }

        public void loadContent()
        {
            MediaPlayer.IsRepeating = true;

            imperial = game.Content.Load<Song>("Music/Imperial-March");
            beatit = game.Content.Load<Song>("Music/BeatIt");

            laugh1 = game.Content.Load<SoundEffect>("Music/deadLaugh");
            laugh2 = game.Content.Load<SoundEffect>("Music/deadLaugh2");
            explosion = game.Content.Load<SoundEffect>("Music/explosion");

            // laod data if needed etc etc
            if (gameState.Equals(GameState.Playing))
            {
                    Team1Gold = new List<string>();
                    Team2Gold = new List<string>();

                    //MediaPlayer.Play(beatit);

                    //game.Content.Unload();
                    if (SkyBox != null)
                    {
                        SkyBox.Dispose();
                        SkyBox = null;
                    }
                    loadMap(INITIAL_MAP);

                    SkyBox.Initialize();
                    SkyBox.loadContent();
                    prevGameState = GameState.Playing;

                    ObjectsLoaded = true;

                    // hard coded planet placement
                    Random rand = new Random();

                    Objects.Planets.Planet plan = new Objects.Planets.Planet(game, Team.Red, new Vector3(- rand.Next(500), rand.Next(500), -mapRadius * 1.2f));
                    AllObjects.Add(plan);
                    Objects.Planets.Planet plan2 = new Objects.Planets.Planet(game, Team.Blue, new Vector3(+ rand.Next(500), - rand.Next(500), mapRadius * 1.2f));
                    AllObjects.Add(plan2);
            }
            loadTex = game.Content.Load<Texture2D>("HudTextures/Loading");
            loadNarative = game.Content.Load<Texture2D>("HudTextures/Loading_narrative");
            btnA = game.Content.Load<Texture2D>("Menu/buttonA");
            f = game.Content.Load<SpriteFont>("Fonts/menuFont");
        }

        public void unloadContent()
        {
            // issue here remember to talk to team (Note to self)...
        }

        public void Update(GameTime gameTime)
        {
            if (gameState.Equals(GameState.Playing))
            {
                if (ObjectsLoaded)
                {
                    if ((CheckPoints.Count > 0 || Player1.Target != null) && ((BBNGame)game).totalElapsedTimeSeconds < BBNGame.MAX_TIME)
                    {
                        for (i = 0; i < AllObjects.Count; ++i)
                            AllObjects.ElementAt(i).Update(gameTime);
                        ((BBNGame)game).totalElapsedTimeSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;


                        checkCollision();
                        RemoveDeadObjects();
                        moveObjectsInGrid();
                    }
                }
                else
                {
                    loadContent();
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
                if (ObjectsLoaded)
                {
                    //reset graphics device state to draw 3D correctly (after spritebatch has drawn the system is in an invalid state)
                    
                    #region "Player 1"
                    drawObjects(gameTime, Player1);
                    #endregion

                    // set the graphics device back to normal
                    game.GraphicsDevice.Viewport = Origional;
                }
        }

        private void drawObjects(GameTime gameTime,  Objects.playerObject player)
        {
            Camera.CameraMatrices cam; // init variable

            // First off draw for player 1
            cam = player.Camera;
            game.GraphicsDevice.Viewport = player.getViewport;
            // draw skybox fist each time
            SkyBox.Draw(gameTime, cam);
            game.GraphicsDevice.BlendState = BlendState.Opaque;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // draw all other objects
            for (i = 0; i < AllObjects.Count; ++i)
            //    if ((AllObjects.ElementAt(i).Position - cam.Position).Length() <= DETAIL_CULL_DISTANCE)
                if (AllObjects.ElementAt(i) is Checkpoint)
                {
                    if (AllObjects.ElementAt(i) == Player1.Target)
                        AllObjects.ElementAt(i).Draw(gameTime, cam);
                }
                else
                    AllObjects.ElementAt(i).Draw(gameTime, cam);

            // we have to draw the huds afterward so that in third person camera the huds will draw above the player (as the dpth buffer is removed)
            for (i = 0; i < AllObjects.Count; ++i)
            //    if ((AllObjects.ElementAt(i).Position - cam.Position).Length() <= HUD_DETAIL_CULL_DISTANCE)
                if (AllObjects.ElementAt(i) is Checkpoint)
                {
                    if (AllObjects.ElementAt(i) == Player1.Target)
                        AllObjects.ElementAt(i).drawSuroundingBox(game.sb, cam, player);
                }
                else
                    AllObjects.ElementAt(i).drawSuroundingBox(game.sb, cam, player);

            //draw the players hud now (so that the target boxes wont obscure them)
            player.drawHud(game.sb, DynamicObjs, gameTime);
        }
        #endregion

        #region "Objects methods"

        public static List<Grid.GridObjectInterface> getTargets(Objects.playerObject player)
        {
            return Grid.getTargets(300, Matrix.CreateFromQuaternion(player.rotation), player);
        }

        private static void moveObjectsInGrid()
        {
            foreach (Objects.StaticObject obj in DynamicObjs)
                gameGrid.registerObject(obj);
        }

        /// <summary>
        /// Loops through all the objects deleting those that should not exist
        /// </summary>
        private static void RemoveDeadObjects()
        {
            for (i = 0; i < AllObjects.Count; i++)
            {
                if (AllObjects.ElementAt(i).getHealth <= 0)
                {
                    AllObjects.ElementAt(i).killObject();
                }
            }
        }

        /// <summary>
        /// Adds the object Specified to the correct matricies
        /// NOTE!:
        ///     These can only be of types:
        ///         Asteroid
        ///         Planet
        ///         Fighter
        ///         Projectile (of any type)
        ///         Destroyer.
        /// </summary>
        /// <param name="Object">The object to add</param>
        public static void addObject(Objects.StaticObject Object)
        {
            // initialise the object first
            Object.Initialize();
            Object.LoadContent();

            if (Object is Objects.Projectile)
            {
                Projectiles.Add(Object);
            }
            else if (Object is Objects.Checkpoint)
            {
                CheckPoints.Add(Object);
            }
            if (Object is Objects.DynamicObject)
                DynamicObjs.Add(Object);

            gameGrid.registerObject(Object);
            AllObjects.Add(Object);
        }

        public static void removeObject(Objects.StaticObject Object)
        {
            if (Object is Objects.Projectile)
            {
                Projectiles.Remove(Object);
            }
            if (Object is Objects.DynamicObject)
                DynamicObjs.Remove(Object);

            Vector3 velocity = Object.ShipMovementInfo.speed * Matrix.CreateFromQuaternion(Object.rotation).Forward;
        }
        /// <summary>
        /// Sets the system to use new instances of player objects
        /// </summary>
        /// <param name="playerIndex">Either Red or Blue</param>
        public static Objects.playerObject spawnPlayer(Objects.Team playerIndex, Game game)
        {
                addObject(Player1 = new BBN_Game.Objects.playerObject(game, Objects.Team.Red, Vector3.Zero, new Vector3(0, 0, -1), false));
                    return Player1;    
        }
        #endregion

        #region "Map loader"

        protected void loadMap(string mapName)
        {
            // clear all lists
            DynamicObjs.Clear();
            AllObjects.Clear();
            DynamicObjs.Clear();
            Asteroids.Clear();
            Projectiles.Clear();

            //First read in map:
            XmlReader reader = XmlReader.Create(mapName);
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element)
                    if (reader.Name == "Map")
                        readMapContent(reader.ReadSubtree());
                    else throw new Exception("Expected Token: Map");
            reader.Close();

            //Initially Spawn players:
            spawnPlayer(Objects.Team.Red, game);
        }
        /// <summary>
        /// Reads the Map subtree of the XML file
        /// </summary>
        /// <param name="reader">XML Reader @ Map</param>
        private void readMapContent(XmlReader reader)
        {
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element)
                    switch (reader.Name) //send for the correct subroutine to load the rest of the tree
                    {
                        case "Map":
                            mapRadius = Convert.ToSingle(reader.GetAttribute("mapRadius"));
                            gameGrid = new BBN_Game.Grid.GridStructure((int)mapRadius, GRID_CUBE_SIZE);
                            break;
                        case "Skybox":
                            readSkyboxData(reader);
                            break;
                        case "ContentItem":
                            readContentItemData(reader);
                            break;
                    }
        }
        /// <summary>
        /// Loads skybox subtree of Map tree
        /// </summary>
        /// <param name="reader">XML reader @ skybox</param>
        private void readSkyboxData(XmlReader reader)
        {
            skyboxTexture = reader.GetAttribute("texture");
            skyboxRepeat = Convert.ToSingle(reader.GetAttribute("repeat"));
            // set up skybox
            SkyBox = new BBN_Game.Graphics.Skybox.Skybox(game, skyboxTexture, mapRadius*2, (int)skyboxRepeat);
            game.Components.Add(SkyBox);
        }
        /// <summary>
        /// Loads content item subtree from Map tree
        /// </summary>
        /// <param name="reader">XML Reader @ contentItem</param>
        private void readContentItemData(XmlReader reader)
        {
            String id = reader.GetAttribute("id");
            String className = reader.GetAttribute("className");
            String type = reader.GetAttribute("type");
            int owningTeam = 0;
            float x = 0, y = 0, z = 0, yaw = 0, pitch = 0, roll = 0, scaleX = 0, scaleY = 0, scaleZ = 0;
            String modelName = "";
            XmlReader subtree = reader.ReadSubtree();
            while (subtree.Read())
                if (subtree.NodeType == XmlNodeType.Element)
                    switch (subtree.Name)
                    {
                        case "x":
                            x = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "y":
                            y = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "z":
                            z = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "yaw":
                            yaw = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "pitch":
                            pitch = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "roll":
                            roll = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "scaleX":
                            scaleX = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "scaleY":
                            scaleY = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "scaleZ":
                            scaleZ = Convert.ToSingle(subtree.ReadString());
                            break;
                        case "modelName":
                            modelName = subtree.ReadString();
                            break;
                        case "owningTeam":
                            owningTeam = Convert.ToInt32(subtree.ReadString());
                            break;
                    }
            //now just make them into objects:
            switch (className)
            {
                case "Astroid":
                    Asteroid a = new Asteroid(game,Team.neutral,new Vector3(x,y,z));
                    a.rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                    a.ShipMovementInfo.scale = Math.Max(Math.Max(scaleX, scaleY), scaleZ);
                    addObject(a);
                    break;
                case "Checkpoint":
                    Checkpoint cp = new Checkpoint(game, Team.neutral, new Vector3(x, y, z));
                    cp.ShipMovementInfo.scale = Math.Max(Math.Max(scaleX, scaleY), scaleZ);
                    addObject(cp);
                    break;
            }
        }
        /// <summary>
        /// Converts map team id to Objects.Team enumeration instance (0 is red, 1 is blue, otherwise neutral)
        /// </summary>
        /// <param name="team">integer indicating the team's id</param>
        /// <returns>Objects.Team instance</returns>
        private static Objects.Team getTeamFromMapTeamId(int team)
        {
            return team == -1 ? Objects.Team.neutral : (team == 0 ? Objects.Team.Red : (team == 1 ? Objects.Team.Blue : Objects.Team.neutral));
        }
        #endregion

        #region "Collision Detection"
        public void checkCollision()
        {
           foreach (Objects.DynamicObject obj in DynamicObjs)
                if (!obj.Position.Equals(obj.getPreviousPosition))
            {
                List<Grid.GridObjectInterface> list = gameGrid.checkNeighbouringBlocks(obj);
                foreach (Grid.GridObjectInterface other in list)
                    if (other is Objects.StaticObject)
                    {
                        if (!other.Equals(obj))
                        {
                            if (obj is Objects.Projectile && other is Objects.Projectile)
                                continue;
                            Objects.StaticObject o1 = obj as Objects.StaticObject;
                            Objects.StaticObject o2 = other as Objects.StaticObject;                        
                            //if (Collision_Detection.CollisionDetectionHelper.isObjectsCollidingOnMeshPartLevel(o1.shipModel, o2.shipModel,
                              //  o1.getWorld,o2.getWorld,
                                //o1 is Objects.Projectile || o2 is Objects.Projectile))
                            if (obj.getBoundingSphere().Intersects(other.getBoundingSphere()))
                            {
                                // Collision occured call on the checker
                                checkTwoObjects(obj, ((Objects.StaticObject)other));
                            }
                        }
                    }
            }
        }
        private void checkTwoObjects(Objects.StaticObject obj1, Objects.StaticObject obj2)
        {
                // add object collision settings
            if (obj1 is Checkpoint || obj2 is Checkpoint) return;

                if (obj1 is Objects.DynamicObject && obj2 is Objects.DynamicObject)
                {
                    ((BBNGame)game).numberOfHits++;
                    Objects.DynamicObject d1 = obj1 as Objects.DynamicObject;
                    Objects.DynamicObject d2 = obj2 as Objects.DynamicObject;
                    d1.bumpVelocity += Vector3.Normalize(Matrix.CreateFromQuaternion(d2.rotation).Forward) * d2.Mass * d2.ShipMovementInfo.speed / d1.Mass * COLLISION_SPEED_PRESERVATION;
                    d2.bumpVelocity += Vector3.Normalize(Matrix.CreateFromQuaternion(d1.rotation).Forward) * d1.Mass * d1.ShipMovementInfo.speed / d2.Mass * COLLISION_SPEED_PRESERVATION;
                }
                else if (obj1 is Objects.DynamicObject)
                {
                    ((BBNGame)game).numberOfHits++;
                    Objects.DynamicObject d = obj1 as Objects.DynamicObject;
                    d.bumpVelocity += Vector3.Normalize(Matrix.CreateFromQuaternion(d.rotation).Backward) * d.ShipMovementInfo.speed * COLLISION_SPEED_PRESERVATION;
                }
                else if (obj2 is Objects.DynamicObject)
                {
                    ((BBNGame)game).numberOfHits++;
                    Objects.DynamicObject d = obj2 as Objects.DynamicObject;
                    d.bumpVelocity += Vector3.Normalize(Matrix.CreateFromQuaternion(d.rotation).Backward) * d.ShipMovementInfo.speed * COLLISION_SPEED_PRESERVATION;
                }
                obj1.ShipMovementInfo.speed = 0;
                obj2.ShipMovementInfo.speed = 0;
        }

        #endregion

        // debug
        public static int getNumberAround(Objects.StaticObject obj)
        {
            return gameGrid.checkNeighbouringBlocks(obj).Count;
        }
    }
}
