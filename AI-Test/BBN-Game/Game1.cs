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
using BBN_Game.Map;
using BBN_Game.Objects;

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
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Team t1 = new Team();
            player1 = new BBN_Game.Objects.playerObject(this,t1,new Vector3(0,0,0),new Vector3(0,0,0));

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
            //des.Initialize();

            chasCam = new BBN_Game.Camera.ChaseCamera(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            //setup connections:
            BBNMap.loadMap("Content/patrolPath.xml", Content, GraphicsDevice);
            navComputer = new NavigationComputer();
            navComputer.registerObject(player1);
            navComputer.setNewPathForRegisteredObject(player1, BBNMap.content["0"] as Node, BBNMap.content["8"] as Node);
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
            player1.LoadContent();
           // des.LoadContent();
           // skyBox = new BBN_Game.Graphics.Skybox.Skybox(this, 1000, new int[] { 1, 1, 1, 1, 1, 1 }, new string[] { "Skybox/SkyboxTop", "Skybox/SkyboxBottom", "Skybox/SkyboxRight", "Skybox/SkyboxLeft", "Skybox/SkyboxFront", "Skybox/SkyboxBack" });
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
           // des.Update(gameTime);
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
            //Camera.CameraMatrices cam = new BBN_Game.Camera.CameraMatrices(chasCam.view, chasCam.proj,new Vector3(0,0,0),(float)Math.PI/4);
            //skyBox.Draw(gameTime, cam, player1.Position);
            //des.Draw(gameTime, cam);
            player1.Draw(gameTime,player1.Camera);

            base.Draw(gameTime);
            spriteBatch.Begin();
            spriteBatch.DrawString(spf,String.Format("Position: {0:0.00} ; {1:0.00} ; {2:0.00}",player1.Position.X,player1.Position.Y,player1.Position.Z),new Vector2(0,25),Color.Yellow);
            List<Node> path = navComputer.getPath(player1);
            if (path.Count > 0)
            {
                Node nextWaypoint = path.Last();

                Vector3 vWantDir = Vector3.Normalize(nextWaypoint.Position - player1.Position);

                float distance = (float)Math.Sqrt(vWantDir.Z * vWantDir.Z + vWantDir.X * vWantDir.X);
                float tpitch = distance == 0 ? (float)Math.Sign(-vWantDir.Y) * (float)Math.PI / 2 : -(float)Math.Atan2(vWantDir.Y, distance);
                float tyaw = (float)Math.Atan2(vWantDir.X, vWantDir.Z);
                Vector3 vLookDir = -Matrix.CreateFromQuaternion(player1.rotation).Forward;
                distance = (float)Math.Sqrt(vLookDir.Z * vLookDir.Z + vLookDir.X * vLookDir.X);
                float cyaw = (float)Math.Atan2(vLookDir.X, vLookDir.Z);
                float cpitch = distance == 0 ? (float)Math.Sign(-vLookDir.Y) * (float)Math.PI / 2 : -(float)Math.Atan2(vLookDir.Y, distance);

                spriteBatch.DrawString(spf, String.Format("Current Yaw,pitch: {0:0.00} ; {1:0.00}", cyaw * 180 / Math.PI, cpitch * 180 / Math.PI), new Vector2(0, 50), Color.Yellow);
                spriteBatch.DrawString(spf, String.Format("Target Yaw,pitch: {0:0.00} ; {1:0.00}", tyaw * 180 / Math.PI, tpitch * 180 / Math.PI), new Vector2(0, 75), Color.Yellow);

                string pathdebug = "";

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
            }
            spriteBatch.End();
        }
    }
}
