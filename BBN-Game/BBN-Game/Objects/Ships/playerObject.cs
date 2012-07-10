using System;
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

        SpriteFont f;

        StaticObject target;

        public Boolean mouseInverted = true;
        
        protected float acceleration, deceleration;

        Camera.ChaseCamera chaseCamera;

        Viewport playerViewport;

        static Texture2D HudBarHolder;
        static Texture2D HudBar;

        private const float MissileReload = 10, MechinegunReload = 0.2f, DefensiveReload = 20;

        private float [] reloadTimer;

        protected Boolean twoPlayer;

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

            this.numHudLines = 7;
            typeOfLine = PrimitiveType.LineList;

            Shield = 100;
            Health = 100;
            totalHealth = 100;
        }
        
        /// <summary>
        /// Constructor with a index (multiplayer on xbox use)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="index"></param>
        public playerObject(Game game, Team team, Vector3 position, Vector3 startingDirection, Boolean twoPlayer)
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

            this.twoPlayer = twoPlayer;
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
            playerViewport.Width = twoPlayer ? Game.GraphicsDevice.Viewport.Width / 2 - 1 : Game.GraphicsDevice.Viewport.Width;

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
                if (state.IsKeyDown(Keys.E))
                {
                    if (reloadTimer[0] <= 0)
                    {
                        Controller.GameController.addObject(new Objects.Bullet(Game, this.target, this));
                        reloadTimer[0] = MechinegunReload;
                    }
                }
                #endregion
            }
            #endregion
            #region "Player 2"
            if (twoPlayer)
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

        #region "Update"
        protected override void setVertexPosition(float screenX, float screenY, float radiusOfObject, Color col)
        {
            //Line 1
            targetBoxVertices[0].Position.X = screenX - radiusOfObject * 0.5f;
            targetBoxVertices[0].Position.Y = screenY + radiusOfObject * 1f;
            targetBoxVertices[0].Color = col;

            targetBoxVertices[1].Position.X = screenX - radiusOfObject * 1f;
            targetBoxVertices[1].Position.Y = screenY + radiusOfObject * 0.5f;
            targetBoxVertices[1].Color = col;

            //    Line 2
            targetBoxVertices[2].Position.X = screenX - radiusOfObject * 1f;
            targetBoxVertices[2].Position.Y = screenY - radiusOfObject * 0.5f;
            targetBoxVertices[2].Color = col;

            targetBoxVertices[3].Position.X = screenX - radiusOfObject * 0.5f;
            targetBoxVertices[3].Position.Y = screenY - radiusOfObject * 1f;
            targetBoxVertices[3].Color = col;

            //     line 3
            targetBoxVertices[4].Position.X = screenX + radiusOfObject * 0.5f;
            targetBoxVertices[4].Position.Y = screenY - radiusOfObject * 1f;
            targetBoxVertices[4].Color = col;

            targetBoxVertices[5].Position.X = screenX + radiusOfObject * 1f;
            targetBoxVertices[5].Position.Y = screenY - radiusOfObject * 0.5f;
            targetBoxVertices[5].Color = col;

            //     line 4
            targetBoxVertices[6].Position.X = screenX + radiusOfObject * 1f;
            targetBoxVertices[6].Position.Y = screenY + radiusOfObject * 0.5f;
            targetBoxVertices[6].Color = col;

            targetBoxVertices[7].Position.X = screenX + radiusOfObject * 0.5f;
            targetBoxVertices[7].Position.Y = screenY + radiusOfObject * 1f;
            targetBoxVertices[7].Color = col;
        }



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
        #endregion

        #region "Draw"

        /// <summary>
        /// Draws the payers hud
        /// </summary>
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

            #region "Health Bar"

            sb.DrawString(f, this.Health.ToString("0000"), new Vector2(Game.GraphicsDevice.Viewport.Width / 2f - 10, 0), Color.Red);

            #endregion

            #region "Reload speeds"
            //sb.DrawString(f, reloadTimer[1].ToString("00"), new Vector2(0, 0), Color.Red);
            #endregion

            #region "Debug"

            Vector3 tmp1 = MathEuler.AngleTo(Position + Vector3.Transform(new Vector3(0, 0, 10), Matrix.CreateFromQuaternion(rotation)), Position);
            sb.DrawString(f,tmp1.Y + " - " + tmp1.X + " - " + tmp1.Z, new Vector2(0, 0), Color.Yellow);
            Vector3 tmp = MathEuler.QuaternionToEuler2(rotation);
            sb.DrawString(f, tmp.X + " - " + tmp.Y + " - " + tmp.Z, new Vector2(0, 15), Color.Red);


            #endregion

            sb.End();

            sb.Dispose();
            sb = null;


            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }

        #endregion
    }
}
