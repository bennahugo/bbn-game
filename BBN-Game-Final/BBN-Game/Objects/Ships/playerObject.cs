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
    enum CurrentCam
    {
        Chase = 0,
        FirstPerson = 1
    }

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

        CurrentCam cameraType = CurrentCam.Chase;

        Camera.ChaseCamera chaseCamera;
        Camera.FirstPersonCam fpCamera;

        Viewport playerViewport;

        static Texture2D HudBarHolder;
        static Texture2D HudBar;
        static Texture2D arrow;

        private const float MissileReload = 7, MechinegunReload = 0.5f, DefensiveReload = 20;

        private float[] reloadTimer;

        protected Boolean twoPlayer;

        private KeyboardState oldState;

        private List<Objects.StaticObject> previousList;

        /// <summary>
        /// Getter and setter
        /// </summary>
        /// 
        #region manage the pop-up menu for trading ships and missiles
        private int upFactor = 150;
        public int UpFactor
        {
            get { return upFactor; }
            set { upFactor = value; }
        }
        Boolean isGoingUp = true;
        public Boolean GoingUp
        {
            get { return isGoingUp; }
            set { isGoingUp = value; }
        }


        #endregion

        #region "Map features"
        static Texture2D mapBackground, playerT, towerT, destroyerT, fighterT, baseT;
        Boolean drawMapBool = false;
        #endregion

        private int numMissiles = 5;
        public int Missiles
        {
            get { return numMissiles; }
            set { numMissiles = value; }
        }
        public Camera.CameraMatrices Camera
        {
            get { return cameraType.Equals(CurrentCam.Chase) ? new Camera.CameraMatrices(chaseCamera.view, chaseCamera.proj, chaseCamera.position, chaseCamera.viewingAnle) : new Camera.CameraMatrices(fpCamera.view, fpCamera.proj, fpCamera.Position, fpCamera.viewingAnle); }
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
        private int tradeMenuOption = 1;
        public int TradeMenuOption
        {
            get { return tradeMenuOption; }
            set { tradeMenuOption = value; }
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
            this.pitchSpeed = 40;
            this.rollSpeed = pitchSpeed * 1.5f;
            this.yawSpeed = pitchSpeed * 1.05f;
            this.maxSpeed = 50;
            this.minSpeed = -10;
            this.greatestLength = 10f;
            this.shipData.scale = 1f;

            this.numHudLines = 7;
            typeOfLine = PrimitiveType.LineList;

            Shield = 100;
            Health = 500;
            totalHealth = 500;
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
            previousList = new List<StaticObject>();
        }

        protected override void resetModels()
        {
            if (this.Team == Team.Red)
                model = Game.Content.Load<Model>("Models/Ships/playerShip");
            else
                model = Game.Content.Load<Model>("Models/Ships/playerShip");

            f = Game.Content.Load<SpriteFont>("SpriteFont1");

            // make sure that you get the static hud values
            HudBar = Game.Content.Load<Texture2D>("HudTextures/HealthBar");
            HudBarHolder = Game.Content.Load<Texture2D>("HudTextures/HealthBarHolder");
            arrow = Game.Content.Load<Texture2D>("HudTextures/arrow");
            playerT = Game.Content.Load<Texture2D>("HudTextures/arrow");
            mapBackground = Game.Content.Load<Texture2D>("HudTextures/map");

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
            this.cameraType = CurrentCam.Chase;
            chaseCamera = new BBN_Game.Camera.ChaseCamera(playerViewport.Width, playerViewport.Height);
            fpCamera = new BBN_Game.Camera.FirstPersonCam(playerViewport.Width, playerViewport.Height);
            #endregion

            oldState = Keyboard.GetState();

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
            if (GamePad.GetState(PlayerIndex.One).IsConnected)
                xboxControls((float)gt.ElapsedGameTime.TotalSeconds);
            else
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
                #region "Extra Controls (Camera)"
                if (state.IsKeyDown(Keys.F1) && oldState.IsKeyUp(Keys.F1))
                    cameraType = cameraType.Equals(CurrentCam.Chase) ? CurrentCam.FirstPerson : CurrentCam.Chase;
                if (state.IsKeyDown(Keys.R) && oldState.IsKeyUp(Keys.R))
                    Controller.GridDataCollection.tryCaptureTower(this);
                if (state.IsKeyDown(Keys.V) && oldState.IsKeyUp(Keys.V))
                    getNewTarget();
                if (state.IsKeyDown(Keys.N) && oldState.IsKeyUp(Keys.N))
                    drawMapBool = !drawMapBool;
                #endregion

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
                if (state.IsKeyDown(Keys.Space) && oldState.IsKeyUp(Keys.Space))
                    mouseInverted = mouseInverted ? false : true;
                #endregion

                #region "Guns"
                if (state.IsKeyDown(Keys.F))
                {
                    if (target != null)
                        if (reloadTimer[1] <= 0 && numMissiles > 0)
                        {
                            Controller.GameController.addObject(new Objects.Missile(Game, this.target, this));
                            reloadTimer[1] = MissileReload;
                            numMissiles--;
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
                    if (state.IsKeyDown(Keys.PageDown) && oldState.IsKeyUp(Keys.PageDown))
                        getNewTarget();
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
                        if (reloadTimer[1] <= 0 && numMissiles > 0)
                        {
                            Controller.GameController.addObject(new Objects.Missile(Game, this.target, this));
                            reloadTimer[1] = MissileReload;
                            numMissiles--;
                        }
                    }
                    if (state.IsKeyDown(Keys.NumPad7))
                    {
                        if (reloadTimer[0] <= 0)
                        {
                            Controller.GameController.addObject(new Objects.Bullet(Game, this.target, this));
                            reloadTimer[0] = MechinegunReload;
                        }
                    }
                }
                    #endregion


            // reset loader
            for (int i = 0; i < 3; ++i)
                reloadTimer[i] = reloadTimer[i] > 0 ? reloadTimer[i] - (1 * time) : 0;
            #endregion

            oldState = state;
        }

        GamePadState prevPadState1 = GamePad.GetState(PlayerIndex.One);
        GamePadState prevPadState2 = GamePad.GetState(PlayerIndex.Two);
        public void xboxControls(float time)
        {
            GamePadState pad1State = GamePad.GetState(PlayerIndex.One);
            GamePadState pad2State = GamePad.GetState(PlayerIndex.Two);

            #region Player 1
            if (index == PlayerIndex.One)
            {

                #region Extra Controls: Turret Capture & Camera

                if (pad1State.Buttons.LeftShoulder == ButtonState.Pressed && prevPadState1.Buttons.LeftShoulder == ButtonState.Released)
                    cameraType = cameraType.Equals(CurrentCam.Chase) ? CurrentCam.FirstPerson : CurrentCam.Chase;
                if (pad1State.Buttons.X == ButtonState.Pressed && prevPadState1.Buttons.X == ButtonState.Released)
                    Controller.GridDataCollection.tryCaptureTower(this);

                #endregion

                #region Rotations

                #region yaw

                if (pad1State.ThumbSticks.Left.X <= -0.5)
                {
                    shipData.yaw += yawSpeed * time;
                }
                if (pad1State.ThumbSticks.Left.X >= 0.5)
                {
                    shipData.yaw -= yawSpeed * time;
                }

                #endregion

                #region Pitch & Roll

                float pitch = 0;
                float roll = 0;

                if (pad1State.ThumbSticks.Right.Y >= 0.5)
                    pitch = pitchSpeed * time;
                else if (pad1State.ThumbSticks.Right.Y <= -0.5)
                    pitch = -pitchSpeed * time;

                if (pad1State.ThumbSticks.Right.X <= -0.5)
                    roll = (rollSpeed) * time;
                else if (pad1State.ThumbSticks.Right.X >= 0.5)
                    roll = -(rollSpeed) * time;

                shipData.roll -= roll;
                shipData.pitch = mouseInverted ? shipData.pitch + pitch : shipData.pitch - pitch;

                #endregion

                #endregion

                #region Accel Deccel checks
                if (pad1State.ThumbSticks.Left.Y >= 0.6)
                {
                    if (shipData.speed < maxSpeed)
                    {
                        shipData.speed += acceleration * time;
                    }
                }
                else if (pad1State.ThumbSticks.Left.Y <= -0.6)
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

                #region Guns

                //fire missile
                if (pad1State.Buttons.B == ButtonState.Pressed)
                {
                    if (reloadTimer[1] <= 0 && numMissiles > 0)
                    {
                        Controller.GameController.addObject(new Objects.Missile(Game, this.target, this));
                        reloadTimer[1] = MissileReload;
                        numMissiles--;
                    }
                }

                //fire cannon
                if (pad1State.Triggers.Right >= 0.6)
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

            #region Player 2
            if (twoPlayer)
                if (index == PlayerIndex.Two)
                {
                    // Allows the game to exit using XBox
                    if (pad2State.Buttons.Start == ButtonState.Pressed)
                        this.Game.Exit();

                    #region Extra Controls: Turret Capture & Camera

                    if (pad2State.Buttons.LeftShoulder == ButtonState.Pressed && prevPadState2.Buttons.LeftShoulder == ButtonState.Released)
                        cameraType = cameraType.Equals(CurrentCam.Chase) ? CurrentCam.FirstPerson : CurrentCam.Chase;
                    if (pad2State.Buttons.X == ButtonState.Pressed && prevPadState2.Buttons.X == ButtonState.Released)
                        Controller.GridDataCollection.tryCaptureTower(this);

                    #endregion

                    #region Rotations

                    #region yaw

                    if (pad2State.ThumbSticks.Left.X <= -0.5)
                    {
                        shipData.yaw += yawSpeed * time;
                    }
                    if (pad2State.ThumbSticks.Left.X >= 0.5)
                    {
                        shipData.yaw -= yawSpeed * time;
                    }

                    #endregion

                    #region Pitch & Roll

                    float pitch = 0;
                    float roll = 0;

                    if (pad2State.ThumbSticks.Right.Y >= 0.5)
                        pitch = pitchSpeed * time;
                    else if (pad2State.ThumbSticks.Right.Y <= -0.5)
                        pitch = -pitchSpeed * time;

                    if (pad2State.ThumbSticks.Right.X <= -0.5)
                        roll = (rollSpeed) * time;
                    else if (pad2State.ThumbSticks.Right.X >= 0.5)
                        roll = -(rollSpeed) * time;

                    shipData.roll -= roll;
                    shipData.pitch = mouseInverted ? shipData.pitch + pitch : shipData.pitch - pitch;

                    #endregion

                    #endregion

                    #region Accel Deccel checks
                    if (pad2State.ThumbSticks.Left.Y >= 0.6)
                    {
                        if (shipData.speed < maxSpeed)
                        {
                            shipData.speed += acceleration * time;
                        }
                    }
                    else if (pad2State.ThumbSticks.Left.Y <= -0.6)
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

                    #region Guns

                    //fire missile
                    if (pad2State.Buttons.B == ButtonState.Pressed)
                    {
                        if (reloadTimer[1] <= 0 && numMissiles > 0)
                        {
                            Controller.GameController.addObject(new Objects.Missile(Game, this.target, this));
                            reloadTimer[1] = MissileReload;
                            numMissiles--;
                        }
                    }

                    //fire cannon
                    if (pad2State.Triggers.Right >= 0.6)
                    {
                        if (reloadTimer[0] <= 0)
                        {
                            Controller.GameController.addObject(new Objects.Bullet(Game, this.target, this));
                            reloadTimer[0] = MechinegunReload;
                        }
                    }

                    #endregion
                }
            // reset loader
            for (int i = 0; i < 3; ++i)
                reloadTimer[i] = reloadTimer[i] > 0 ? reloadTimer[i] - (1 * time) : 0;
            #endregion

            prevPadState1 = pad1State;
            prevPadState2 = pad2State;
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

        public override void killObject()
        {

        }

        /// <summary>
        /// override for the update method to call on the camera update methods.
        /// </summary>
        /// <param name="gt">Game time variable</param>
        public override void Update(GameTime gt)
        {
            // todo add the if statement on the enum for cockpit View
            if (cameraType.Equals(CurrentCam.Chase))
                chaseCamera.update(gt, Position, Matrix.CreateFromQuaternion(rotation));
            else
                fpCamera.update(gt, Position, Matrix.CreateFromQuaternion(rotation), this.getGreatestLength);


            if (target != null)
            {
                if (target is Turret)
                {
                    if (((Turret)target).Repairing)
                        target = null;
                }
                else
                {
                    if (target.getHealth <= 0)
                        target = null;
                }
            }

            if (target == null)
            {
                getNewTarget();
            }

            base.Update(gt);
        }

        /// <summary>
        /// This tries to get 
        /// </summary>
        public void getNewTarget()
        {
            List<Grid.GridObjectInterface> list = Controller.GameController.getTargets(this);

            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list.ElementAt(i) is StaticObject)
                    {
                        StaticObject obj = (StaticObject)list.ElementAt(i);

                        if (obj.Team != Team.neutral)
                        {
                            if (obj.Team != this.Team)
                            {
                                if (!previousList.Contains(obj))
                                {
                                    previousList.Add(obj);
                                    target = obj;
                                    return;
                                }
                            }
                        }
                    }

                }
                previousList.Clear();
            }
        }
        #endregion

        #region "Draw"

        public override void Draw(GameTime gameTime, BBN_Game.Camera.CameraMatrices cam)
        {
            if (this.cameraType.Equals(CurrentCam.Chase))
                base.Draw(gameTime, cam);
        }

        /// <summary>
        /// Draws the payers hud
        /// </summary>
        public void drawHud(SpriteBatch sb, List<Objects.StaticObject> list, GameTime gt)
        {
            if (!Game.GraphicsDevice.Viewport.Equals(playerViewport))
                return;



            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            Viewport viewport = Game.GraphicsDevice.Viewport;

            if (drawMapBool)
            {
                drawMap(sb, viewport);
                return;
            }


            int hudHeight = (int)((float)viewport.Height * 0.175f);
            int hudWidth = (int)((float)viewport.Width * 0.2f);

            sb.Begin();

            sb.DrawString(f, 1 / gt.ElapsedGameTime.TotalSeconds + " - " + this.greatestLength, new Vector2(100, 100), Color.Green);

            #region "Speed"
            sb.DrawString(f, shipData.speed.ToString("00"), new Vector2(hudWidth * 0.15f, viewport.Height - hudHeight * 0.55f), Color.Aqua);

            sb.Draw(HudBarHolder, new Rectangle(0, viewport.Height - hudHeight, hudWidth, hudHeight), Color.Aqua);
            #endregion

            #region "Health Bar"

            sb.DrawString(f, this.Health.ToString("0000"), new Vector2(Game.GraphicsDevice.Viewport.Width / 2f - 10, 0), Color.Red);

            int barStartX = (int)((float)hudWidth * 0.05f);
            int barWidthX = (int)((((float)hudWidth * 0.99f) - barStartX) * (Health / totalHealth));

            int barStartY = viewport.Height - (int)((float)hudHeight * 0.297f);
            int barWidthY = (int)(((float)hudHeight * 0.297f));

            int textHeight = HudBar.Height;
            int textWidth = (int)(HudBar.Width * (Health / totalHealth));

            sb.Draw(HudBar, new Rectangle(barStartX, barStartY, barWidthX, barWidthY), new Rectangle(0, 0, textWidth, textHeight), new Color(1 - (Health / totalHealth), (Health / totalHealth), 0));

            #endregion

            #region "Enemy Health-bars"



            #region Base's health bars

            //Your base and enemy base health bars
            textHeight = 15;
            if (BBN_Game.Controller.GameController.team1.teamId == this.Team)
            {
                textWidth = (int)(100 * (BBN_Game.Controller.GameController.team1.teamBase.getHealth /
                    BBN_Game.Controller.GameController.team1.teamBase.getTotalHealth));
                sb.Draw(HudBar, new Rectangle(10, 10, 100, 15),
                    new Rectangle(0, 0, textWidth, textHeight),
                    new Color(1 - (BBN_Game.Controller.GameController.team1.teamBase.getHealth /
                        BBN_Game.Controller.GameController.team1.teamBase.getTotalHealth),
                        (BBN_Game.Controller.GameController.team1.teamBase.getHealth /
                        BBN_Game.Controller.GameController.team1.teamBase.getTotalHealth), 0));

                textWidth = (int)(100 * (BBN_Game.Controller.GameController.team2.teamBase.getHealth /
                    BBN_Game.Controller.GameController.team2.teamBase.getTotalHealth));
                sb.Draw(HudBar, new Rectangle(this.playerViewport.Width - 115, 10, 100, 15),
                    new Rectangle(0, 0, textWidth, textHeight),
                    new Color(1 - (BBN_Game.Controller.GameController.team2.teamBase.getHealth /
                        BBN_Game.Controller.GameController.team2.teamBase.getTotalHealth),
                        (BBN_Game.Controller.GameController.team2.teamBase.getHealth /
                        BBN_Game.Controller.GameController.team2.teamBase.getTotalHealth), 0));
            }
            else
            {
                textWidth = (int)(100 * (BBN_Game.Controller.GameController.team2.teamBase.getHealth /
                    BBN_Game.Controller.GameController.team2.teamBase.getTotalHealth));
                sb.Draw(HudBar, new Rectangle(10, 10, 100, 15),
                    new Rectangle(0, 0, textWidth, textHeight),
                    new Color(1 - (BBN_Game.Controller.GameController.team2.teamBase.getHealth /
                        BBN_Game.Controller.GameController.team2.teamBase.getTotalHealth),
                        (BBN_Game.Controller.GameController.team2.teamBase.getHealth /
                        BBN_Game.Controller.GameController.team2.teamBase.getTotalHealth), 0));

                textWidth = (int)(100 * (BBN_Game.Controller.GameController.team1.teamBase.getHealth /
                    BBN_Game.Controller.GameController.team1.teamBase.getTotalHealth));
                sb.Draw(HudBar, new Rectangle(this.playerViewport.Width - 115, 10, 100, 15),
                    new Rectangle(0, 0, textWidth, textHeight),
                    new Color(1 - (BBN_Game.Controller.GameController.team1.teamBase.getHealth /
                        BBN_Game.Controller.GameController.team1.teamBase.getTotalHealth),
                        (BBN_Game.Controller.GameController.team1.teamBase.getHealth /
                        BBN_Game.Controller.GameController.team1.teamBase.getTotalHealth), 0));
            }
            #endregion

            #endregion

            #region "Reload speeds"
            #endregion

            #region "Arrows"
            int width = (int)(Math.Min(viewport.Height, viewport.Width) * 0.05);
            float distance = Math.Min(viewport.Height, viewport.Width);

            Base b;
            if (this.Team == Team.Red)
            {
                b = Controller.GameController.BaseTeam2;
            }
            else
            {
                b = Controller.GameController.BaseTeam1;
            }

            if (!b.IsVisible(this.Camera))
            {
                Vector3 A = b.Position - Camera.Position;

                Matrix mat = Matrix.CreateFromQuaternion(this.rotation);

                float x = Vector3.Dot(A, mat.Right);
                float y = Vector3.Dot(A, mat.Up);

                float angle = (float)Math.Atan2(y, x);

                int posx = (int)MathHelper.Clamp(-((int)(Math.Cos(angle) * distance)) + viewport.Width / 2, 0 + width, viewport.Width);
                int posy = (int)MathHelper.Clamp(-((int)(Math.Sin(angle) * distance)) + viewport.Height / 2, 0 + viewport.Height * 0.1f + width, viewport.Height * 0.8f - width);

                sb.Draw(arrow, new Rectangle((int)(posx - width / 2), (int)(posy - width / 2), width, width), new Rectangle(0, 0, arrow.Width, arrow.Height), Color.Orange, angle, new Vector2(arrow.Width / 2, arrow.Height / 2), SpriteEffects.FlipHorizontally, 1);
            }

            if (target != null)
                if (!target.IsVisible(this.Camera))
                {
                    Vector3 A = target.Position - Camera.Position;

                    Matrix mat = Matrix.CreateFromQuaternion(this.rotation);

                    float x = Vector3.Dot(A, mat.Right);
                    float y = Vector3.Dot(A, mat.Up);

                    float angle = (float)Math.Atan2(y, x);

                    int posx = (int)MathHelper.Clamp(-((int)(Math.Cos(angle) * distance)) + viewport.Width / 2, 0 + width, viewport.Width);
                    int posy = (int)MathHelper.Clamp(-((int)(Math.Sin(angle) * distance)) + viewport.Height / 2, 0 + viewport.Height * 0.1f + width, viewport.Height * 0.8f - width);

                    sb.Draw(arrow, new Rectangle((int)(posx - width / 2), (int)(posy - width / 2), width, width), new Rectangle(0, 0, arrow.Width, arrow.Height), Color.Red, angle, new Vector2(arrow.Width / 2, arrow.Height / 2), SpriteEffects.FlipHorizontally, 1);
                    //sb.DrawString(f, "<>", new Vector2(posx, posy), Color.Red);
                }
            #endregion

            sb.End();

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }

        private void drawMap(SpriteBatch sb, Viewport view)
        {
            List<StaticObject> objects = Controller.GameController.getAllObjects;
            List<StaticObject> Dobjects = Controller.GameController.DynamicObjects;

            float radius = Controller.GameController.mapRadius;
            radius = radius * 0.5f;

            sb.Begin();

            sb.Draw(mapBackground, new Rectangle(0, 0, view.Width, view.Height), Color.White);

            foreach (StaticObject obj in objects)
            {
                if (obj is Projectile)
                    break;

                Vector2 pos = new Vector2(obj.Position.X, obj.Position.Z);
                pos = new Vector2(pos.X + radius, pos.Y + radius);
                pos = new Vector2(pos.X / (radius * 2), pos.Y / (radius * 2));
                pos = new Vector2(pos.X * view.Width, pos.Y * (view.Height * 1.4f));

                float objectWidth = view.Width * 0.01f;

                Color c = obj.Team.Equals(Team.neutral) ? Color.Yellow : obj.Equals(this.target) ? Color.Red : obj.Team.Equals(this.Team) ? Color.Green : Color.Orange;

                //Texture tex = 

                sb.Draw(playerT, new Rectangle((int)(pos.X - objectWidth / 2), (int)(pos.Y - objectWidth / 2), (int)objectWidth, (int)objectWidth), c);
            }
            sb.End();
        }

        #endregion
    }
}