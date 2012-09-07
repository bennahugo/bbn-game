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
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace BBN_Game.Controller
{

    enum GameState
    {
        MainMenu = 0,
        OptionsMenu = 1,
        Playing = 2,
        Paused = 3,
        notLoaded = 4
    }

    enum Players
    {
        single = 0,
        two = 1
    }

    class GameController
    {
        #region "Constants"
        private const string INITIAL_MAP = "Content/Maps/Death Zone.xml";
        private const int GRID_CUBE_SIZE = 200;
        private const int MAX_NUM_FIGHTERS_PER_TEAM = 10;
        private const int MAX_NUM_DESTROYERS_PER_TEAM = 10;
        private const float MAP_SCALE_VALUE = 2;
        private const float COLLISION_SPEED_PRESERVATION = 0.025f;
        private const float YAW_PITCH_ROLL_SPEED_FACTOR_FOR_AI_PLAYER = 0.0166f;
        private const float DETAIL_CULL_DISTANCE = 650;
        private const float HUD_DETAIL_CULL_DISTANCE = 1050;
        #endregion

        #region "Object holders"
        static List<Objects.StaticObject> AllObjects, DynamicObjs, Fighters, Destroyers, Towers, Asteroids, Projectiles;
        static List<AI.Marker> spawnPoints = new List<AI.Marker>();
        static Dictionary<String, AI.Node> pathNodes = new Dictionary<string,BBN_Game.AI.Node>(); 
        static Objects.playerObject Player1, Player2;
        static Objects.Base Team1Base, Team2Base;

        static AI.PlayerSpawnPoint Team1SpawnPoint;
        static AI.PlayerSpawnPoint Team2SpawnPoint;

        public static AI.TeamInformation team1 { get; internal set; }
        public static AI.TeamInformation team2 { get; internal set; }
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
        static Players numPlayers;
        
        static Grid.GridStructure gameGrid;
        
        //Controler objects:
        static AI.NavigationComputer navComputer;
        static AI.AIController aiController;
        static Menu.MenuController menuController;

        public static Players NumberOfPlayers
        {
            get { return numPlayers; }
            set { numPlayers = value; }
        }
        public static Grid.GridStructure Grid
        {
            get { return gameGrid; }
        }
        public static AI.NavigationComputer navigationComputer
        {
            get { return navComputer; }
        }
        public static AI.AIController AIController
        {
            get { return aiController; }
        }
        #endregion

        #region "Global Data Holders"
        Viewport Origional;
        BBN_Game.BBNGame game;
        static int i;

        static Boolean ObjectsLoaded;
        #endregion

        #region "XNA Required"
        public GameController(BBN_Game.BBNGame game)
        {
            this.game = game;

            // Set up the Variables
            gameState = GameState.MainMenu;
            prevGameState = GameState.notLoaded;
            numPlayers = Players.single;
            ObjectsLoaded = false;
        }

        public void Initialize()
        {
            #region "Lists"
            AllObjects = new List<BBN_Game.Objects.StaticObject>();
            Fighters = new List<BBN_Game.Objects.StaticObject>();
            Destroyers = new List<BBN_Game.Objects.StaticObject>();
            Towers = new List<BBN_Game.Objects.StaticObject>();
            Asteroids = new List<BBN_Game.Objects.StaticObject>();
            Projectiles = new List<BBN_Game.Objects.StaticObject>();
            DynamicObjs = new List<BBN_Game.Objects.StaticObject>();
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

            #region "Menu initialization"
            menuController = new BBN_Game.Menu.MenuController(this, this.game);
            #endregion
        }

        public void loadContent()
        {
            // laod data if needed etc etc
            if (gameState.Equals(GameState.Playing))
            {
                if (!(prevGameState.Equals(GameState.Playing)))
                {
                    game.Content.Unload();

                    loadMap(INITIAL_MAP);

                    SkyBox.Initialize();
                    SkyBox.loadContent();

                    prevGameState = GameState.Playing;

                    Player2.Target = Player1;
                    Player1.Target = Player2;

                    ObjectsLoaded = true;
                }
            }
            else
            {
                game.Content.Unload();
                                
                ObjectsLoaded = false;
            }
            menuController.loadContent();
        }

        public void unloadContent()
        {
            // issue here remember to talk to team (Note to self)...
        }

        //handle keyboard controls for pause and trade menu interaction
        KeyboardState prevKeyState = Keyboard.GetState();
        Boolean tradePanelUp1 = false;
        Boolean tradePanelUp2 = false;
        public void handleKeyControls()
        {
            KeyboardState keyState = Keyboard.GetState();

            if (gameState.Equals(GameState.Playing))
            {
                if (keyState.IsKeyDown(Keys.P) && prevKeyState.IsKeyUp(Keys.P))//pause game
                {
                    gameState = GameState.Paused;
                    menuController.updateState();
                }

                #region Player 1
                //player 1 trade menu
                if (keyState.IsKeyDown(Keys.Z) && prevKeyState.IsKeyUp(Keys.Z) && Player1.TradeMenuOption > 1)
                    Player1.TradeMenuOption--;
                else if(keyState.IsKeyDown(Keys.Z) && prevKeyState.IsKeyUp(Keys.Z) && Player1.TradeMenuOption == 1)
                    Player1.TradeMenuOption = 3;
                if (keyState.IsKeyDown(Keys.X) && prevKeyState.IsKeyUp(Keys.X) && Player1.TradeMenuOption < 3)
                    Player1.TradeMenuOption++;
                else if (keyState.IsKeyDown(Keys.X) && prevKeyState.IsKeyUp(Keys.X) && Player1.TradeMenuOption == 3)
                    Player1.TradeMenuOption = 1;
                //trade menu selection
                if (keyState.IsKeyDown(Keys.C) && prevKeyState.IsKeyUp(Keys.C))
                    makePurchase(Player1, team1);

                if (keyState.IsKeyDown(Keys.Q) && prevKeyState.IsKeyUp(Keys.Q))
                {
                    if (tradePanelUp1)
                    {
                        tradePanelUp1 = false;
                        Player1.GoingUp = false;
                    }
                    else
                    {
                        tradePanelUp1 = true;
                        Player1.GoingUp = true;
                        if(Player1.UpFactor >= 150)
                            Player1.TradeMenuOption = 1;
                    }
                }

                #endregion

                #region Player 2
                if (numPlayers.Equals(Players.two))
                {                    
                    //player 2 trade menu
                    if (keyState.IsKeyDown(Keys.NumPad2) && prevKeyState.IsKeyUp(Keys.NumPad2) && Player2.TradeMenuOption > 1)
                        Player2.TradeMenuOption--;
                    else if (keyState.IsKeyDown(Keys.NumPad2) && prevKeyState.IsKeyUp(Keys.NumPad2) && Player2.TradeMenuOption == 1)
                        Player2.TradeMenuOption = 3;
                    if (keyState.IsKeyDown(Keys.NumPad3) && prevKeyState.IsKeyUp(Keys.NumPad3) && Player2.TradeMenuOption < 3)
                        Player2.TradeMenuOption++;
                    else if (keyState.IsKeyDown(Keys.NumPad3) && prevKeyState.IsKeyUp(Keys.NumPad3) && Player2.TradeMenuOption == 3)
                        Player2.TradeMenuOption = 1;
                    if (keyState.IsKeyDown(Keys.M) && prevKeyState.IsKeyUp(Keys.M))
                    {
                        if (tradePanelUp2)
                        {
                            tradePanelUp2 = false;
                            Player2.GoingUp = false;
                        }
                        else
                        {
                            tradePanelUp2 = true;
                            Player2.GoingUp = true;
                            if (Player2.UpFactor >= 150)
                                Player2.TradeMenuOption = 1;
                        }
                    }

                    if (keyState.IsKeyDown(Keys.N) && prevKeyState.IsKeyUp(Keys.N))
                        makePurchase(Player2, team2);
                }
                #endregion

                prevKeyState = keyState;
            }
        }
        /// <summary>
        /// Handles a purchasing command from the trade menu
        /// </summary>
        /// <param name="player">player who has made selection</param>
        /// <param name="team">team of the player who has made the selection</param>
        private void makePurchase(Objects.playerObject player, AI.TeamInformation team)
        {
            if (player.TradeMenuOption == 1)
            {
                //buy destroyer
                if (team.teamCredits > TradingInformation.destroyerCost)
                {
                    Objects.Destroyer ds = new Objects.Destroyer(game, Objects.Team.Red, Vector3.Zero);
                    team.spawnQueue.Add(ds);
                    team.teamCredits -= TradingInformation.destroyerCost;
                }
            }
            else if (player.TradeMenuOption == 2)
            {
                //buy fighter
                if (team.teamCredits > TradingInformation.fighterCost)
                {
                    Objects.Fighter fi = new Objects.Fighter(game, Objects.Team.Red, Vector3.Zero);
                    team.spawnQueue.Add(fi);
                    team.teamCredits -= TradingInformation.fighterCost;
                }
            }
            else if (player.TradeMenuOption == 3)
            {
                //buy missiles for player
                if (team.teamCredits > TradingInformation.missileCost)
                {
                    player.Missiles++;
                    team.teamCredits -= TradingInformation.missileCost;
                }
            }
        }
        //XBox controls for pause and trade menu interaction
        GamePadState prevPadState1 = GamePad.GetState(PlayerIndex.One);
        GamePadState prevPadState2 = GamePad.GetState(PlayerIndex.Two);
        public void handleXboxControls()
        {
            GamePadState pad1State = GamePad.GetState(PlayerIndex.One);
            GamePadState pad2State = GamePad.GetState(PlayerIndex.Two);

            #region Player 1
            //bring up pause menu
            if (pad1State.Buttons.Back == ButtonState.Pressed && prevPadState1.Buttons.Back == ButtonState.Released)//Player 1 pauses game
            {
                prevGameState = gameState;
                gameState = GameState.Paused;
                menuController.updateState();
            }
           
            //bring up trade menus
            if (pad1State.Buttons.RightShoulder == ButtonState.Pressed && prevPadState1.Buttons.RightShoulder != ButtonState.Pressed)//player1
            {
                if (tradePanelUp1)
                {
                    tradePanelUp1 = false;
                    Player1.GoingUp = false;
                }
                else
                {
                    tradePanelUp1 = true;
                    Player1.GoingUp = true;
                    if (Player1.UpFactor >= 150)
                        Player1.TradeMenuOption = 1;
                }
            }

            if (tradePanelUp1)
            {
                //traverse trade menu
                if (pad1State.DPad.Down == ButtonState.Pressed && prevPadState1.DPad.Down == ButtonState.Released && Player1.TradeMenuOption < 3)
                    Player1.TradeMenuOption++;
                if (pad1State.DPad.Up == ButtonState.Pressed && prevPadState1.DPad.Up == ButtonState.Released && Player1.TradeMenuOption > 1)
                    Player1.TradeMenuOption--;
                //menu option selection
                if (pad1State.Buttons.A == ButtonState.Pressed && prevPadState1.Buttons.A == ButtonState.Released)
                    makePurchase(Player1, team1);
            }
            #endregion

            #region Player 2
            if (numPlayers.Equals(Players.two) && GamePad.GetState(PlayerIndex.Two).IsConnected)
            {
                //bring up pause menu
                if (pad2State.Buttons.Back == ButtonState.Pressed && prevPadState2.Buttons.Back == ButtonState.Released)//Player 2 pauses game
                {
                    prevGameState = gameState;
                    gameState = GameState.Paused;
                    menuController.updateState();
                }
                //bring up trade menus
                if (pad2State.Buttons.RightShoulder == ButtonState.Pressed && prevPadState2.Buttons.RightShoulder == ButtonState.Released)//player2
                {
                    if (tradePanelUp2)
                    {
                        tradePanelUp2 = false;
                        Player2.GoingUp = false;
                    }
                    else
                    {
                        tradePanelUp2 = true;
                        Player2.GoingUp = true;
                        if (Player2.UpFactor >= 150)
                            Player2.TradeMenuOption = 1;
                    }
                }

                if (tradePanelUp2)
                {
                    //traverse trade menu
                    if (pad2State.DPad.Down == ButtonState.Pressed && prevPadState2.DPad.Down == ButtonState.Released && Player2.TradeMenuOption < 3)
                        Player2.TradeMenuOption++;
                    if (pad2State.DPad.Up == ButtonState.Pressed && prevPadState2.DPad.Up == ButtonState.Released && Player2.TradeMenuOption > 1)
                        Player2.TradeMenuOption--;
                    //menu option selection
                    if (pad2State.Buttons.A == ButtonState.Pressed && prevPadState2.Buttons.A == ButtonState.Released)
                        makePurchase(Player2, team2);
                }
            }
            #endregion            
            
            prevPadState1 = pad1State;
            prevPadState2 = pad2State;
        }

        public void Update(GameTime gameTime)
        {
            if (gameState.Equals(GameState.Playing))
            {
                if (ObjectsLoaded)
                {
                    for (i = 0; i < AllObjects.Count; ++i)
                        AllObjects.ElementAt(i).Update(gameTime);

                    SkyBox.Update(gameTime);

                    if (GamePad.GetState(PlayerIndex.One).IsConnected)
                        handleXboxControls();
                    else
                        handleKeyControls();

                    checkCollision();
                    RemoveDeadObjects();
                    moveObjectsInGrid();

                    //Update AI and navigation:
                    aiController.update(gameTime, game);
                    navComputer.updateAIMovement(gameTime);
                }
                else
                {
                    loadContent();
                }
            }
            else
            {
                menuController.updateMenu(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (gameState.Equals(GameState.MainMenu) || gameState.Equals(GameState.OptionsMenu))
            {
                menuController.drawMenu(gameTime);
            }
            else 
            {
                if (ObjectsLoaded)
                {
                    //reset graphics device state to draw 3D correctly (after spritebatch has drawn the system is in an invalid state)
                    game.GraphicsDevice.RenderState.AlphaBlendEnable = false;
                    game.GraphicsDevice.RenderState.AlphaTestEnable = false;
                    game.GraphicsDevice.RenderState.DepthBufferEnable = true;
                    game.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
                    #region "Player 1"
                    drawObjects(gameTime, Player1);
                    if (tradePanelUp1)//handle trade panel poping up                    
                        menuController.drawTradeMenu(Player1);
                    else if(Player1.UpFactor < 150)
                        menuController.drawTradeMenu(Player1);
                    else
                        menuController.drawTradeStats(Player1);
                    #endregion

                    #region "Player 2"
                    if (numPlayers.Equals(Players.two))
                    {
                        drawObjects(gameTime, Player2);
                        if (tradePanelUp2)
                            menuController.drawTradeMenu(Player2);
                        else if (Player2.UpFactor < 150)
                            menuController.drawTradeMenu(Player2);
                        else
                            menuController.drawTradeStats(Player2);
                    }
                    #endregion

                    // set the graphics device back to normal
                    game.GraphicsDevice.Viewport = Origional;                    
                }

                if(gameState.Equals(GameState.Paused))
                    menuController.drawMenu(gameTime);
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
            // draw all other objects
            for (i = 0; i < AllObjects.Count; ++i)
                if ((AllObjects.ElementAt(i).Position - cam.Position).Length() < DETAIL_CULL_DISTANCE)
                    AllObjects.ElementAt(i).Draw(gameTime, cam);

            // we have to draw the huds afterward so that in third person camera the huds will draw above the player (as the dpth buffer is removed)
            for (i = 0; i < AllObjects.Count; ++i)
                if ((AllObjects.ElementAt(i).Position - cam.Position).Length() < HUD_DETAIL_CULL_DISTANCE)
                    AllObjects.ElementAt(i).drawSuroundingBox(cam, player);

            //draw the players hud now (so that the target boxes wont obscure them)
            player.drawHud(DynamicObjs);
            
            //TODO DEBUG: draw the paths of the AI
            drawPaths(gameTime, player.Camera, new BasicEffect(game.GraphicsDevice, null), game.GraphicsDevice);
        }
        #endregion

        #region "Objects methods"

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

            if (Object is Objects.Fighter)
            {
                Fighters.Add(Object);
            }
            else if (Object is Objects.Destroyer)
            {
                Destroyers.Add(Object);
            }
            else if (Object is Objects.Turret)
            {
                Towers.Add(Object);
            }
            else if (Object is Objects.Projectile)
            {
                Projectiles.Add(Object);
            }

            if (Object is Objects.DynamicObject)
                DynamicObjs.Add(Object);

            // _____-----TODO----____ Add asteroids when class is made

            gameGrid.registerObject(Object);
            AllObjects.Add(Object);
        }

        public static void removeObject(Objects.StaticObject Object)
        {
            if (Object is Objects.Fighter)
            {
                Fighters.Remove(Object);
            }
            else if (Object is Objects.Destroyer)
            {
                Destroyers.Remove(Object);
            }
            else if (Object is Objects.Turret)
            {
                Towers.Remove(Object);
            }
            else if (Object is Objects.Projectile)
            {
                Projectiles.Remove(Object);
            }

            if (Object is Objects.DynamicObject)
                DynamicObjs.Remove(Object);

            // _____-----TODO----____ Add asteroids when class is made


            gameGrid.deregisterObject(Object);
            AllObjects.Remove(Object);
            --i;
        }
        /// <summary>
        /// Sets the system to use new instances of player objects
        /// </summary>
        /// <param name="playerIndex">Either Red or Blue</param>
        public static Objects.playerObject spawnPlayer(Objects.Team playerIndex, Game game)
        {
            switch (playerIndex)
            {
                case Objects.Team.Red:
                    addObject(Player1 = new BBN_Game.Objects.playerObject(game, Objects.Team.Red, Team1SpawnPoint.Position, new Vector3(0, 0, -1), numPlayers.Equals(Players.single) ? false : true));
                    return Player1;
                case Objects.Team.Blue:
                    addObject(Player2 = new BBN_Game.Objects.playerObject(game, Objects.Team.Blue, Team2SpawnPoint.Position, new Vector3(0, 0, 1), numPlayers.Equals(Players.single) ? false : true));
                    if (numPlayers.Equals(Players.single))
                    {
                        Player2.setYawSpeed = Player2.getYawSpeed * YAW_PITCH_ROLL_SPEED_FACTOR_FOR_AI_PLAYER;
                        Player2.setpitchSpeed = Player2.getpitchSpeed * YAW_PITCH_ROLL_SPEED_FACTOR_FOR_AI_PLAYER;
                        Player2.setRollSpeed = Player2.getRollSpeed * YAW_PITCH_ROLL_SPEED_FACTOR_FOR_AI_PLAYER;
                    }
                    return Player2;
                default:
                    throw new Exception("System only supports index 1 or 2");
            }
                
        }
        #endregion

        #region "Map loader"

        protected void loadMap(string mapName)
        {
            //First read in map:
            XmlReader reader = XmlReader.Create(mapName);
            while (reader.Read())
                if (reader.NodeType == XmlNodeType.Element)
                    if (reader.Name == "Map")
                        readMapContent(reader.ReadSubtree());
                    else throw new Exception("Expected Token: Map");
            reader.Close();

            //Setup controllers:
            navComputer = new AI.NavigationComputer(gameGrid);
            aiController = new AI.AIController(gameGrid, navComputer, this, Towers);

            //Initially Spawn players:
            spawnPlayer(Objects.Team.Red, game);
            spawnPlayer(Objects.Team.Blue, game);

            //Setup teams:
            List<Objects.Turret> team1InitialTurrets = new List<Objects.Turret>();
            List<Objects.Turret> team2InitialTurrets = new List<Objects.Turret>();
            foreach (Objects.Turret tr in Towers)
                if (tr.Team == Objects.Team.Red)
                    team1InitialTurrets.Add(tr);
                else if (tr.Team == Objects.Team.Blue)
                    team2InitialTurrets.Add(tr);
            List<AI.Node> team1OwnedNodes = new List<AI.Node>();
            List<AI.Node> team2OwnedNodes = new List<AI.Node>();
            foreach (AI.Node n in pathNodes.Values)
                if (n.OwningTeam == 0)
                    team1OwnedNodes.Add(n);
                else if (n.OwningTeam == 1)
                    team2OwnedNodes.Add(n);
            List<AI.SpawnPoint> team1OwnedSpawnPoints = new List<AI.SpawnPoint>();
            List<AI.SpawnPoint> team2OwnedSpawnPoints = new List<AI.SpawnPoint>();
            foreach (AI.SpawnPoint n in spawnPoints)
                if (n.OwningTeam == 0)
                    team1OwnedSpawnPoints.Add(n);
                else if (n.OwningTeam == 1)
                    team2OwnedSpawnPoints.Add(n);
            team1 = new AI.TeamInformation(Objects.Team.Red, false, team1InitialTurrets, TradingInformation.startingCreditsPerTeam, Player1, team1OwnedNodes,
                team1OwnedSpawnPoints, MAX_NUM_FIGHTERS_PER_TEAM, MAX_NUM_DESTROYERS_PER_TEAM, Team1Base, Team1SpawnPoint);
            team2 = new AI.TeamInformation(Objects.Team.Blue, numPlayers.Equals(Players.single) ? true : false, team2InitialTurrets, 
                TradingInformation.startingCreditsPerTeam, Player2, team2OwnedNodes,
                team2OwnedSpawnPoints, MAX_NUM_FIGHTERS_PER_TEAM, MAX_NUM_DESTROYERS_PER_TEAM, Team2Base, Team2SpawnPoint);
            aiController.registerTeam(team1);
            aiController.registerTeam(team2);
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
                        case "Marker":
                            readMarkerData(reader);
                            break;
                        case "ContentItem":
                            readContentItemData(reader);
                            break;
                        case "PathEdge":
                            readEdgeData(reader);
                            break;
                        default:
                            throw new Exception("Error in Map file. Unknown Token");
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
        /// Loads marker subtree of Map tree
        /// </summary>
        /// <param name="reader">XML reader @ marker</param>
        private void readMarkerData(XmlReader reader)
        {
            String id = reader.GetAttribute("id");
            String className = reader.GetAttribute("className");
            int owningTeam = Convert.ToInt32(reader.GetAttribute("owningTeam"));
            float x = 0, y = 0, z = 0;
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
                    }
            //Now write them into the correct lists:
            switch (className)
            {
                case "PathNode":
                    AI.Node n = new AI.Node(new Vector3(x, y, z), owningTeam);
                    n.id = id;
                    pathNodes.Add(id,n);
                    gameGrid.registerObject(n);
                    break;
                case "SpawnPoint":
                    AI.SpawnPoint sp = new AI.SpawnPoint(new Vector3(x, y, z), owningTeam);
                    sp.id = id;
                    spawnPoints.Add(sp);
                    gameGrid.registerObject(sp);
                    break;
                case "PlayerSpawnPoint":
                    AI.PlayerSpawnPoint psp = new AI.PlayerSpawnPoint(new Vector3(x, y, z), owningTeam);
                    psp.id = id;
                    if (owningTeam == 0)
                        Team1SpawnPoint = psp;
                    else if (owningTeam == 1)
                        Team2SpawnPoint = psp;
                    gameGrid.registerObject(psp);
                    break;
            }
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
                case "Base":
                    Objects.Base b = new Objects.Base(game, getTeamFromMapTeamId(owningTeam), new Vector3(x, y, z));
                    b.rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                    addObject(b);
                    if (b.Team == BBN_Game.Objects.Team.Red)
                        Team1Base = b;
                    else if (b.Team == BBN_Game.Objects.Team.Blue)
                        Team2Base = b;
                    break;
                case "Tower":
                    Objects.Turret t = new Objects.Turret(game, getTeamFromMapTeamId(owningTeam), new Vector3(x, y, z));
                    t.rotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
                    addObject(t);
                    break;
            }
        }
        /// <summary>
        /// Loads edge subtree from map tree
        /// </summary>
        /// <param name="reader">XML Reader @ edge</param>
        private void readEdgeData(XmlReader reader)
        {
            float weight = Convert.ToSingle(reader.GetAttribute("weight"));
            float distance = Convert.ToSingle(reader.GetAttribute("distance"));

            String firstNode = "-1", secondNode = "-1";
            XmlReader subtree = reader.ReadSubtree();
            while (subtree.Read())
                if (subtree.NodeType == XmlNodeType.Element)
                    switch (subtree.Name)
                    {
                        case "firstNodeId":
                            firstNode = subtree.ReadString();
                            break;
                        case "secondNodeId":
                            secondNode = subtree.ReadString();
                            break;
                    }
            //Now connect the two nodes:
            if (!(pathNodes.Keys.Contains(firstNode) && pathNodes.Keys.Contains(secondNode)))
                throw new Exception("Map is corrupt: invalid link between pathnodes");
            AI.Node n1 = pathNodes[firstNode];
            AI.Node n2 = pathNodes[secondNode];
            AI.Edge e = new AI.Edge(n1,n2,weight);
            e.distance = distance;
            pathNodes[firstNode].connectedEdges.Add(e);
            pathNodes[secondNode].connectedEdges.Add(e);
        }
        /// <summary>
        /// Converts map team id to Objects.Team enumeration instance (0 is red, 1 is blue, otherwise neutral)
        /// </summary>
        /// <param name="team">integer indicating the team's id</param>
        /// <returns>Objects.Team instance</returns>
        private static Objects.Team getTeamFromMapTeamId(int team)
        {
            return team == -1 ? Objects.Team.nutral : (team == 0 ? Objects.Team.Red : (team == 1 ? Objects.Team.Blue : Objects.Team.nutral));
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
                                if (Collision_Detection.CollisionDetectionHelper.isObjectsCollidingOnMeshPartLevel(obj.shipModel, ((Objects.StaticObject)other).shipModel, obj.getWorld, ((Objects.StaticObject)other).getWorld,
                                    obj is Objects.DynamicObject ? navComputer.isObjectRegistered(obj) : false || 
                                    other is Objects.DynamicObject ? navComputer.isObjectRegistered(other as Objects.DynamicObject) : false || 
                                    obj is Objects.Bullet || obj is Objects.Missile || other is Objects.Bullet || other is Objects.Missile))
                                {
                                    // Collision occured call on the checker
                                    checkTwoObjects(obj, ((Objects.StaticObject)other));
                                }
                    }
            }
        }

        private void checkTwoObjects(Objects.StaticObject obj1, Objects.StaticObject obj2)
        {
            if (obj1 is Objects.Projectile || obj2 is Objects.Projectile)
            {
                if (obj1 is Objects.Projectile)
                {
                    obj1.doDamage(10000);
                    obj2.doDamage(((Objects.Projectile)obj1).damage);
                }
                else
                {
                    obj2.doDamage(10000);
                    obj1.doDamage(((Objects.Projectile)obj2).damage);
                }
            }
            else
            {
                // add object collision settings
                if (obj1 is Objects.DynamicObject && obj2 is Objects.DynamicObject)
                {
                    Objects.DynamicObject d1 = obj1 as Objects.DynamicObject;
                    Objects.DynamicObject d2 = obj2 as Objects.DynamicObject;
                    d1.bumpVelocity += Vector3.Normalize(Matrix.CreateFromQuaternion(d2.rotation).Forward) * d2.Mass * d2.ShipMovementInfo.speed / d1.Mass * COLLISION_SPEED_PRESERVATION;
                    d2.bumpVelocity += Vector3.Normalize(Matrix.CreateFromQuaternion(d1.rotation).Forward) * d1.Mass * d1.ShipMovementInfo.speed / d2.Mass * COLLISION_SPEED_PRESERVATION;
                }
                else if (obj1 is Objects.DynamicObject)
                {
                    Objects.DynamicObject d = obj1 as Objects.DynamicObject;
                    d.bumpVelocity += Vector3.Normalize(Matrix.CreateFromQuaternion(d.rotation).Backward) * d.ShipMovementInfo.speed * COLLISION_SPEED_PRESERVATION;
                }
                else if (obj2 is Objects.DynamicObject)
                {
                    Objects.DynamicObject d = obj2 as Objects.DynamicObject;
                    d.bumpVelocity += Vector3.Normalize(Matrix.CreateFromQuaternion(d.rotation).Backward) * d.ShipMovementInfo.speed * COLLISION_SPEED_PRESERVATION;
                }
                obj1.ShipMovementInfo.speed = 0;
                obj2.ShipMovementInfo.speed = 0;
            }
        }

        #endregion

        // debug
        public static int getNumberAround(Objects.StaticObject obj)
        {
            return gameGrid.checkNeighbouringBlocks(obj).Count;
        }

        public static void drawPath(Objects.DynamicObject obj, Camera.CameraMatrices chasCam, BasicEffect bf, GraphicsDevice gd)
        {
            List<AI.Node> path = navComputer.isObjectRegistered(obj) ? navComputer.getPath(obj) : new List<AI.Node>();

            if (path.Count > 0)
            {
                AI.Node nextWaypoint = path.Last();
                for (int i = 0; i < path.Count - 1; ++i)
                {
                    Utils.Algorithms.Draw3DLine(Color.Yellow, path.ElementAt(i).Position, path.ElementAt(i + 1).Position,
                        bf, gd, chasCam.Projection, chasCam.View, Matrix.Identity);
                }
                Utils.Algorithms.Draw3DLine(Color.Green, path.Last().Position, obj.Position,
                    bf, gd, chasCam.Projection, chasCam.View, Matrix.Identity);
            }
        }
        public static void drawPaths(GameTime gameTime, Camera.CameraMatrices chasCam, BasicEffect bf, GraphicsDevice gd)
        {
            for (int team = 0; team < aiController.getTeamCount(); ++team)
            {
                AI.TeamInformation ti = aiController.getTeam(team);

                drawPath(ti.teamPlayer, chasCam, bf, gd);
                foreach (Objects.Destroyer d in ti.teamDestroyers)
                    drawPath(d, chasCam, bf, gd);
                foreach (Objects.Fighter f in ti.teamFighters)
                    drawPath(f, chasCam, bf, gd);
            }
        }
    }
}
