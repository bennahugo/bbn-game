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

namespace BBN_Game
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class BBNGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public List<SlimDX.DirectInput.Joystick> joysticks = new List<SlimDX.DirectInput.Joystick>();
        List<SlimDX.DirectInput.DeviceInstance> directInputList = new List<SlimDX.DirectInput.DeviceInstance>();
        SlimDX.DirectInput.DirectInput directInput = new SlimDX.DirectInput.DirectInput();

        public enum ControllerMode
        {
            KB = 0,
            GamePad = 1,
            Joystick = 2
        }
        public enum ExperimentMode
        {
            Practice, RealThing
        }
        public static ControllerMode controllerMode = ControllerMode.KB;
        public static ExperimentMode mode = ExperimentMode.Practice;
        public int numberOfHits = 0;
        public float totalElapsedTimeSeconds = 0;
        public const float MAX_TIME = 360;
        public float totalTimeRotationP = 0;
        public float totalTimeRotationY = 0;
        public float totalTimeRotationR = 0;
        // getter and setter
        public GraphicsDeviceManager Graphics
        {
            get { return graphics; }
            set { graphics = value; }
        }

        public SpriteBatch sb
        {
            get { return spriteBatch; }
        }

        Controller.GameController gameControler;

        public BBNGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //assign joysticks
            directInputList.AddRange(directInput.GetDevices(SlimDX.DirectInput.DeviceClass.GameController, SlimDX.DirectInput.DeviceEnumerationFlags.AttachedOnly));
            foreach (SlimDX.DirectInput.DeviceInstance obj in directInputList)
            {
                //joysticks.Capacity++;
                joysticks.Add(new SlimDX.DirectInput.Joystick(directInput, obj.InstanceGuid));
                joysticks[joysticks.Count - 1].Acquire();
            }
            gameControler = new BBN_Game.Controller.GameController(this);            
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            gameControler.Initialize();

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

            // TODO: use this.Content to load your game content here
            gameControler.loadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            gameControler.unloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
               this.Exit();

            // TODO: Add your update logic here
            gameControler.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // sets the colour of the seperator
            GraphicsDevice.Clear(Color.Black);

            gameControler.Draw(gameTime);

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            foreach (SlimDX.DirectInput.Joystick joystick in joysticks)
            {
                joystick.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
