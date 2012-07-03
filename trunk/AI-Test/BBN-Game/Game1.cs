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
using BBN_Game.AI;
using BBN_Game.Utils;

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
        Objects.Destroyer des;
        Graphics.Skybox.Skybox skyBox;
        BasicEffect bf;
        //Camera
        Camera.ChaseCamera chasCam;
        SpriteFont spf;
        AI.NavigationComputer navComputer;
        #region node data
        Node n1 = new Node(new Vector3(0, 0, 0), -1);
        Node n2 = new Node(new Vector3(100, 500, 600), -1);
        Node n3 = new Node(new Vector3(-100, 50, -600), -1);
        Node n4 = new Node(new Vector3(-100, 700, 1500), -1);
        Node n5 = new Node(new Vector3(-300, -2500, 4500), -1);
        #endregion
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            player1 = new BBN_Game.Objects.playerObject(this);
            des = new BBN_Game.Objects.Destroyer(this);

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

            chasCam = new BBN_Game.Camera.ChaseCamera(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            //setup connections:
            n1.connectToNode(n2, 0);
            n1.connectToNode(n3, 0);
            n3.connectToNode(n4, 0);
            n4.connectToNode(n5, 0);
            n1.id = "1";
            n2.id = "2";
            n3.id = "3";
            n4.id = "4";
            n5.id = "5";
            navComputer = new NavigationComputer();
            navComputer.registerObject(player1);
            navComputer.setNewPathForRegisteredObject(player1, n1, n5);
            bf = new BasicEffect(GraphicsDevice, null);
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
            player1.LoadContent(2);
            des.LoadContent();
            skyBox = new BBN_Game.Graphics.Skybox.Skybox(this, 1000, new int[] { 1, 1, 1, 1, 1, 1 }, new string[] { "Skybox/SkyboxTop", "Skybox/SkyboxBottom", "Skybox/SkyboxRight", "Skybox/SkyboxLeft", "Skybox/SkyboxFront", "Skybox/SkyboxBack" });
            spf = Content.Load<SpriteFont>("SpriteFont1");
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
            this.navComputer.updateAIMovement(gameTime);
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
            player1.Draw(gameTime, cam, spriteBatch, des);

            base.Draw(gameTime);
            spriteBatch.Begin();
            spriteBatch.DrawString(spf,String.Format("Position: {0:0.00} ; {1:0.00} ; {2:0.00}",player1.Position.X,player1.Position.Y,player1.Position.Z),new Vector2(0,30),Color.Yellow);
            spriteBatch.DrawString(spf, String.Format("yaw,pitch: {0:0.00} ; {1:0.00}", player1.ShipMovementInfo.totalYaw*180/Math.PI, player1.ShipMovementInfo.totalPitch*180/Math.PI), new Vector2(0, 60), Color.Yellow);
            string pathdebug = "";
            List<Node> path = navComputer.getPath(player1);
            foreach (Node n in path)
            {
                pathdebug += n.id + ", ";
            }
            for (int i = 0; i < path.Count - 1; ++i)
            {
                Algorithms.Draw3DLine(Color.Yellow, path.ElementAt(i).Position, path.ElementAt(i + 1).Position,
                    bf, GraphicsDevice, chasCam.proj, chasCam.view, Matrix.Identity);
            }
            Algorithms.Draw3DLine(Color.Green, path.Last().Position, player1.Position,
                    bf, GraphicsDevice, chasCam.proj, chasCam.view, Matrix.Identity);
            spriteBatch.DrawString(spf, "Path: " + pathdebug, new Vector2(0, 90), Color.Yellow);
            spriteBatch.End();
        }
    }
}
