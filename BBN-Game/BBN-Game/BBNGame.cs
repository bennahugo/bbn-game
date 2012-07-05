using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace BBN_Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BBNGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Objects.playerObject player1;
        Objects.Destroyer des;
        Objects.Fighter fig;
        Objects.Base bse;
        Objects.Missile proj;
        Objects.Turret turret;
        Objects.playerObject plyTmp;

        Viewport orig;

        Graphics.Skybox.Skybox skyBox;

        public BBNGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            player1 = new BBN_Game.Objects.playerObject(this, PlayerIndex.One);

            // temps
            des = new BBN_Game.Objects.Destroyer(this);
            fig = new BBN_Game.Objects.Fighter(this);
            bse = new BBN_Game.Objects.Base(this);
            turret = new BBN_Game.Objects.Turret(this);
            plyTmp = new BBN_Game.Objects.playerObject(this, PlayerIndex.Two);


            skyBox = new BBN_Game.Graphics.Skybox.Skybox(this, "Starfield", 100000, 10);

            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.ApplyChanges();
            //graphics.IsFullScreen = true;
            // TODO: Add your initialization logic here
            player1.Initialize();


            des.Initialize();
            des.Position = new Vector3(0, 0, 0 + 100);
            fig.Initialize();
            fig.Position = new Vector3(0, 0, 50 + 100);
            bse.Initialize();
            bse.Position = new Vector3(50, 0, 0 + 100);
            bse.ShipMovementInfo.scale = 3;
            turret.Initialize();
            turret.Position = new Vector3(50, 50, 0 + 100);
            plyTmp.Initialize();
            plyTmp.Position = new Vector3(0, 50, 50 + 100);
            proj = new BBN_Game.Objects.Missile(this, player1, plyTmp);
            proj.Initialize();


            skyBox.Initialize();


            orig = GraphicsDevice.Viewport;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            player1.LoadContent();

            // temp
            des.LoadContent();
            fig.LoadContent();
            bse.LoadContent();
            proj.LoadContent();
            turret.LoadContent();
            plyTmp.LoadContent();


            skyBox.loadContent();

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here
            player1.Update(gameTime);

            // temp
            des.Update(gameTime);
            fig.Update(gameTime);
            bse.Update(gameTime);
            proj.Update(gameTime);
            turret.Update(gameTime);
            plyTmp.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Khaki);

            GraphicsDevice.Viewport = player1.getViewport;

            // TODO: Add your drawing code here
            Camera.CameraMatrices cam = player1.Camera;
            skyBox.Draw(gameTime, cam);

            // temp
            des.Draw(gameTime, cam);
            fig.Draw(gameTime, cam);
            bse.Draw(gameTime, cam);
            proj.Draw(gameTime, cam);
            turret.Draw(gameTime, cam);
            plyTmp.Draw(gameTime, cam);


            player1.Draw(gameTime, cam);

            des.drawSuroundingBox(cam, player1);
            fig.drawSuroundingBox(cam, player1);
            bse.drawSuroundingBox(cam, player1);
            proj.drawSuroundingBox(cam, player1);
            turret.drawSuroundingBox(cam, player1);
            plyTmp.drawSuroundingBox(cam, player1);

            GraphicsDevice.Viewport = plyTmp.getViewport;
            cam = plyTmp.Camera;
            skyBox.Draw(gameTime, cam);

            // temp
            des.Draw(gameTime, cam);
            fig.Draw(gameTime, cam);
            bse.Draw(gameTime, cam);
            proj.Draw(gameTime, cam);
            turret.Draw(gameTime, cam);
            plyTmp.Draw(gameTime, cam);


            player1.Draw(gameTime, cam);

            des.drawSuroundingBox(cam, plyTmp);
            fig.drawSuroundingBox(cam, plyTmp);
            bse.drawSuroundingBox(cam, plyTmp);
            proj.drawSuroundingBox(cam, plyTmp);
            turret.drawSuroundingBox(cam, plyTmp);
            player1.drawSuroundingBox(cam, plyTmp);

            GraphicsDevice.Viewport = orig;            

            base.Draw(gameTime);
        }
    }
}
