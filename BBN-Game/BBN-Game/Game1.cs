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
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Objects.playerObject player1;
        Objects.Destroyer des, des2, des3;
        Graphics.Skybox.Skybox skyBox;

        //Camera
        Camera.ChaseCamera chasCam;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            player1 = new BBN_Game.Objects.playerObject(this);
            des = new BBN_Game.Objects.Destroyer(this);
            des2 = new BBN_Game.Objects.Destroyer(this);
            des3 = new BBN_Game.Objects.Destroyer(this);
            skyBox = new BBN_Game.Graphics.Skybox.Skybox(this, "Starfield");

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 1024;

            //graphics.IsFullScreen = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            player1.Initialize();
            des.Initialize();
            des2.Initialize();
            des2.Position = new Vector3(20, 20, 45);
            des3.Initialize(); 
            des3.Position = new Vector3(0, -15, 20);
            skyBox.Initialize();

            chasCam = new BBN_Game.Camera.ChaseCamera(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

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
            player1.LoadContent(1);
            des.LoadContent();
            des2.LoadContent();
            des3.LoadContent();
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
            des.Update(gameTime);
            des2.Update(gameTime);
            des3.Update(gameTime);


            chasCam.update(gameTime, player1.Position, Matrix.CreateFromQuaternion(player1.rotation));

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            Camera.CameraMatrices cam = new BBN_Game.Camera.CameraMatrices(chasCam.view, chasCam.proj);
            skyBox.Draw(gameTime, cam, player1.Position);
            des.Draw(gameTime, cam);
            des2.Draw(gameTime, cam);
            des3.Draw(gameTime, cam);
            player1.Draw(gameTime, cam, spriteBatch);


            des.drawSuroundingBox(cam);
            des2.drawSuroundingBox(cam);
            des3.isTarget = true;
            des3.drawSuroundingBox(cam);

            base.Draw(gameTime);
        }
    }
}
