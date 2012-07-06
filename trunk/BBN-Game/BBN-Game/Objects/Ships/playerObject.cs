﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


/////
///
/// Author - Brandon James Talbot
/// 
/// This is the player object
/// This class contains all the data for the player controlled object in the game
////

namespace BBN_Game.Objects
{
    class playerObject : DynamicObject
    {


        // debug
        SpriteFont f;

        #region "Globals"
        /// <summary>
        /// Global variables
        /// Index is the player index for xbox
        /// OrigState is the state of the mouse when centered (Computer controls)
        /// Mouse inverted is another variable to determine the motion of the mouse (joystick)
        /// acceleration - The acceleration speed of the object
        /// Decelleration - The deceleration speed of the object
        /// chaseCamera - The chase camera
        /// </summary>
        PlayerIndex index;

        StaticObject target;

        public Boolean mouseInverted = true;
        
        protected float acceleration, deceleration;

        Camera.ChaseCamera chaseCamera;

        Viewport playerViewport;

        static Texture2D HudBarHolder;
        static Texture2D HudBar;

        private const int MissileReload = 15, MechinegunReload = 5, DefensiveReload = 20;

        private float [] reloadTimer;

        /// <summary>
        /// Getter and setter
        /// </summary>
        public Camera.CameraMatrices Camera
        {
            get { return new Camera.CameraMatrices(chaseCamera.view, chaseCamera.proj, chaseCamera.position, chaseCamera.viewingAnle); }
        }
        public Viewport getViewport
        {
            get { return playerViewport; }
        }
        public StaticObject Target
        {
            get { return target; }
            set { target = value; }
        }
        #endregion

        #region "Constructors - Data setting"

        /// <summary>
        /// Ovveride to set the data for rotation speeds etc...
        /// </summary>
        protected override void setData()
        {
            this.acceleration = 20f;
            this.deceleration = 15;

            this.mass = 10;
            this.pitchSpeed = 50;
            this.rollSpeed = pitchSpeed * 1.5f;
            this.yawSpeed = pitchSpeed;
            this.maxSpeed = 50;
            this.minSpeed = -10;
            this.greatestLength = 6f;
        }
        
        /// <summary>
        /// Constructor with a index (multiplayer on xbox use)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="index"></param>
        public playerObject(Game game, Team team, Vector3 position, Vector3 startingDirection)
            : base(game, team, position)
        {
            this.index = team == Team.Red ? PlayerIndex.One : PlayerIndex.Two;

            float distance = (float)Math.Sqrt(startingDirection.Z * startingDirection.Z + startingDirection.X * startingDirection.X);
            float tpitch = distance == 0 ? (float)Math.Sign(-startingDirection.Y) * (float)Math.PI / 2 : -(float)Math.Atan2(startingDirection.Y, distance);
            float tyaw = (float)Math.Atan2(startingDirection.X, startingDirection.Z);

            rotation = Quaternion.CreateFromYawPitchRoll(tyaw, tpitch, 0);

            reloadTimer = new float[3];
            for (int i = 0; i < 3; ++i)
                reloadTimer[i] = 0;
        }

        protected override void resetModels()
        {
            if (this.Team == Team.Red)
                model = Game.Content.Load<Model>("Models/Ships/PlayerRed");
            else
                model = Game.Content.Load<Model>("Models/Ships/PlayerBlue");

            f = Game.Content.Load<SpriteFont>("SpriteFont1");

            // make sure that you get the static hud values
            HudBar = Game.Content.Load<Texture2D>("HudTextures/HealthBar");
            HudBarHolder = Game.Content.Load<Texture2D>("HudTextures/HealthBarHolder");

            base.resetModels();
        }

        /// <summary>
        /// Initialises the variables
        /// And for computer gets the mouse ready to control the ship
        /// </summary>
        public override void Initialize()
        {
            #region "Viewport initialization"
            playerViewport = Game.GraphicsDevice.Viewport;
            playerViewport.Width = Game.GraphicsDevice.Viewport.Width / 2 - 1;

            // if player 2 it must start in a differant place
            if (index == PlayerIndex.Two)
            {
                playerViewport.X = Game.GraphicsDevice.Viewport.Width / 2 + 1;
            }
            #endregion

            #region "Camera initialization"
            chaseCamera = new BBN_Game.Camera.ChaseCamera(playerViewport.Width, playerViewport.Height);
            #endregion

            base.Initialize();
        }

        #endregion

        #region "Controls"

        /// <summary>
        /// override for the update method to call on the camera update methods.
        /// </summary>
        /// <param name="gt">Game time variable</param>
        public override void Update(GameTime gt)
        {
            // todo add the if statement on the enum for cockpit View
            chaseCamera.update(gt, Position, Matrix.CreateFromQuaternion(rotation));

            base.Update(gt);
        }

        /// <summary>
        /// Overide of the controller
        /// Calls on the keyboard and gamepad key checks
        /// </summary>
        /// <param name="gt">Game time variable</param>
        public override void controller(GameTime gt)
        {
            keyBoardChecks((float)gt.ElapsedGameTime.TotalSeconds);

            base.controller(gt);
        }
        /// <summary>
        /// Does all the keyboard checking
        /// </summary>
        /// <param name="time">Ellapsed time for last step</param>
        public void keyBoardChecks(float time)
        {
            KeyboardState state = Keyboard.GetState();


            #region "Player 1"
            if (index == PlayerIndex.One)
            {
                #region "Accel Deccel checks"
                if (state.IsKeyDown(Keys.W))
                {
                    if (shipData.speed < maxSpeed)
                    {
                        shipData.speed += acceleration * time;
                    }
                }
                else if (state.IsKeyDown(Keys.S))
                {
                    if (shipData.speed > minSpeed)
                    {
                        shipData.speed -= deceleration * time;
                    }
                }
                else
                {
                    if (shipData.speed > 0)
                    {
                        shipData.speed -= deceleration * time * 2;

                        if (shipData.speed < 0)
                            shipData.speed = 0;
                    }
                    else if (shipData.speed < 0)
                    {
                        shipData.speed += acceleration * time;

                        if (shipData.speed > 0)
                            shipData.speed = 0;
                    }
                }
                #endregion

                #region "Rotations"
                #region "Yaw"
                if (state.IsKeyDown(Keys.A))
                {
                    shipData.yaw += yawSpeed * time;
                }
                if (state.IsKeyDown(Keys.D))
                {
                    shipData.yaw -= yawSpeed * time;
                }
                #endregion
                #region "pitch & roll"
                float pitch = 0;
                float roll = 0;

                if (state.IsKeyDown(Keys.I))
                    pitch = pitchSpeed * time;
                else if (state.IsKeyDown(Keys.K))
                    pitch = -pitchSpeed * time;

                if (state.IsKeyDown(Keys.J))
                    roll = (rollSpeed) * time;
                else if (state.IsKeyDown(Keys.L))
                    roll = -(rollSpeed) * time;

                shipData.roll -= roll;
                shipData.pitch = mouseInverted ? shipData.pitch + pitch : shipData.pitch - pitch;
                #endregion

                // Debug
                if (state.IsKeyDown(Keys.Space))
                    mouseInverted = mouseInverted ? false : true;
                #endregion

                #region "Guns"
                if (state.IsKeyDown(Keys.F))
                {
                    if (reloadTimer[1] <= 0)
                    {
                        Controller.GameController.addObject(new Objects.Missile(Game, this.target, this));
                        reloadTimer[1] = MissileReload;
                    }
                }
                #endregion
            }
            #endregion
            #region "Player 2"
            if (index == PlayerIndex.Two)
            {
                #region "Accel Deccel checks"
                if (state.IsKeyDown(Keys.Up))
                {
                    if (shipData.speed < maxSpeed)
                    {
                        shipData.speed += acceleration * time;
                    }
                }
                else if (state.IsKeyDown(Keys.Down))
                {
                    if (shipData.speed > minSpeed)
                    {
                        shipData.speed -= deceleration * time;
                    }
                }
                else
                {
                    if (shipData.speed > 0)
                    {
                        shipData.speed -= deceleration * time * 2;

                        if (shipData.speed < 0)
                            shipData.speed = 0;
                    }
                    else if (shipData.speed < 0)
                    {
                        shipData.speed += acceleration * time;

                        if (shipData.speed > 0)
                            shipData.speed = 0;
                    }
                }
                #endregion

                #region "Rotations"
                #region "Yaw"
                if (state.IsKeyDown(Keys.Left))
                {
                    shipData.yaw += yawSpeed * time;
                }
                if (state.IsKeyDown(Keys.Right))
                {
                    shipData.yaw -= yawSpeed * time;
                }
                #endregion
                #region "pitch & roll"
                float pitch = 0;
                float roll = 0;

                if (state.IsKeyDown(Keys.NumPad8))
                    pitch = pitchSpeed * time;
                else if (state.IsKeyDown(Keys.NumPad5))
                    pitch = -pitchSpeed * time;

                if (state.IsKeyDown(Keys.NumPad4))
                    roll = (rollSpeed) * time;
                else if (state.IsKeyDown(Keys.NumPad6))
                    roll = -(rollSpeed) * time;

                shipData.roll -= roll;
                shipData.pitch = mouseInverted ? shipData.pitch + pitch : shipData.pitch - pitch;
                #endregion

                // Debug
                if (state.IsKeyDown(Keys.NumPad0))
                    mouseInverted = mouseInverted ? false : true;
                #endregion

                #region "Guns"
                if (state.IsKeyDown(Keys.NumPad1))
                {
                    if (reloadTimer[1] <= 0)
                    {
                        Controller.GameController.addObject(new Objects.Missile(Game, this.target, this));
                        reloadTimer[1] = MissileReload;
                    }
                }
            }
            #endregion


            // reset loader
            for (int i = 0; i < 3; ++i)
                reloadTimer[i] = reloadTimer[i] > 0 ? reloadTimer[i] - (1 * time) : 0;
            #endregion
        }

        #endregion

        public void Draw(GameTime gameTime, BBN_Game.Camera.CameraMatrices cam)
        {
            base.Draw(gameTime, cam);
        }

        public void drawHud()
        {
            //if (!Game.GraphicsDevice.Viewport.Equals(playerViewport)) removed cus it was made obsolete
            //    return;

            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            SpriteBatch sb = new SpriteBatch(Game.GraphicsDevice);

            Viewport viewport = Game.GraphicsDevice.Viewport;

            int hudHeight = (int)((float)viewport.Height * 0.175f);
            int hudWidth = (int)((float)viewport.Width * 0.2f);

            sb.Begin();

            #region "Speed"
            sb.DrawString(f, shipData.speed.ToString("00"), new Vector2(hudWidth * 0.15f, viewport.Height - hudHeight * 0.55f), Color.Aqua);

            sb.Draw(HudBarHolder, new Rectangle(0, viewport.Height - hudHeight, hudWidth, hudHeight), Color.Aqua);

            int barStartX = (int)((float)hudWidth * 0.05f);
            int barWidthX = (int)((((float)hudWidth * 0.99f) - barStartX)  * (shipData.speed / maxSpeed)) ;

            int barStartY = viewport.Height - (int)((float)hudHeight * 0.297f);
            int barWidthY = (int)(((float)hudHeight * 0.297f));

            int textHeight = HudBar.Height;
            int textWidth = (int)(HudBar.Width * (shipData.speed / maxSpeed));

            sb.Draw(HudBar, new Rectangle(barStartX, barStartY, barWidthX, barWidthY), new Rectangle(0, 0, textWidth, textHeight), new Color((shipData.speed / maxSpeed), 1 - (shipData.speed / maxSpeed), 0));
            #endregion

            #region "Reload speeds"
            sb.DrawString(f, reloadTimer[1].ToString("00"), new Vector2(0, 0), Color.Red);
            #endregion


            sb.End();

            sb.Dispose();
            sb = null;


            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }

    }
}