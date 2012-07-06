using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


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

    class GameController
    {
        #region "Object holders"
        static List<Objects.StaticObject> AllObjects, Fighters, Destroyers, Towers, Asteroids, Projectiles;

        Objects.playerObject Player1, Player2;
        Objects.Base Team1Base, Team2Base;
        #endregion

        #region "Graphics Devices"
        Graphics.Skybox.Skybox SkyBox;
        #endregion

        #region "Game Controllers"
        GameState gameState, prevGameState;
        #endregion

        #region "Global Data Holders"
        Viewport Origional;
        BBN_Game.BBNGame game;
        static int i;
        #endregion

        #region "XNA Required"
        public GameController(BBN_Game.BBNGame game)
        {
            this.game = game;

            // Set up the Variables
            gameState = GameState.Playing;
            prevGameState = GameState.notLoaded;
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
            #endregion

            #region "Viewport setting"
            game.Graphics.PreferredBackBufferWidth = game.Graphics.GraphicsDevice.DisplayMode.Width;
            game.Graphics.PreferredBackBufferHeight = game.Graphics.GraphicsDevice.DisplayMode.Height;
            //game.Graphics.PreferredBackBufferWidth = 1440;
            //game.Graphics.PreferredBackBufferHeight = 900;
            //game.Graphics.IsFullScreen = true;
            game.Graphics.ApplyChanges();

            Origional = game.GraphicsDevice.Viewport;
            #endregion

        }

        public void loadContent()
        {
            // laod data if needed etc etc
            if (gameState.Equals(GameState.Playing))
            {
                if (!(prevGameState.Equals(GameState.Playing)))
                {
                    loadMap("temp not used yet");

                    SkyBox.Initialize();
                    SkyBox.loadContent();

                    foreach (Objects.StaticObject obj in AllObjects)
                        obj.LoadContent();

                    prevGameState = GameState.Playing;

                    Player2.Target = Player1;
                    Player1.Target = Team2Base;
                }
            }
        }

        public void unloadContent()
        {
            // issue here remember to talk to team (Note to self)...
        }

        public void Update(GameTime gameTime)
        {
            if (gameState.Equals(GameState.Playing))
            {
                for (i = 0; i < AllObjects.Count; ++i)
                    AllObjects.ElementAt(i).Update(gameTime);

                SkyBox.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime)
        {
            if (gameState.Equals(GameState.Playing))
            {

                #region "Player 1"
                drawObjects(gameTime, Player1);
                #endregion

                #region "Player 2"
                drawObjects(gameTime, Player2);
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
            // draw all other objects
            for (i = 0; i < AllObjects.Count; ++i)
                AllObjects.ElementAt(i).Draw(gameTime, cam);

            // we have to draw the huds afterward so that in third person camera the huds will draw above the player (as the dpth buffer is removed)
            for (i = 0; i < AllObjects.Count; ++i)
                    AllObjects.ElementAt(i).drawSuroundingBox(cam, player);

            //draw the players hud now (so that the target boxes wont obscure them)
            player.drawHud();
        }
        #endregion

        #region "Add Objects"

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
                Objects.Projectile p = (Objects.Projectile)Object;
                p.Initialize();
                p.LoadContent();
                Projectiles.Add(p);
            }
            
            // _____-----TODO----____ Add asteroids when class is made

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

            // _____-----TODO----____ Add asteroids when class is made

            AllObjects.Remove(Object);
            --i;
        }

        #endregion

        #region "Map loader"

        protected void loadMap(string mapName)
        {
            // hardcoded for now
            // players
            addObject(Player1 = new BBN_Game.Objects.playerObject(game, Objects.Team.Red, new Vector3(0, 0, -500), new Vector3(0,0,1)));
            addObject(Player2 = new BBN_Game.Objects.playerObject(game, Objects.Team.Blue, new Vector3(0, 0, 500), new Vector3(0,0,-1)));
          
            // Bases
            addObject(Team1Base = new BBN_Game.Objects.Base(game, Objects.Team.Red, new Vector3(10, 5, -510)));
            addObject(Team2Base = new BBN_Game.Objects.Base(game, Objects.Team.Blue, new Vector3(-10, 5, 500)));

            // add a few turrets
            addObject(new Objects.Turret(game, Objects.Team.Red, new Vector3(-10, 5, -500)));
            addObject(new Objects.Turret(game, Objects.Team.Blue, new Vector3(10, 5, 500)));

            // skybox
            SkyBox = new BBN_Game.Graphics.Skybox.Skybox(game, "Starfield", 100000, 10);
        }


        #endregion
    }
}
