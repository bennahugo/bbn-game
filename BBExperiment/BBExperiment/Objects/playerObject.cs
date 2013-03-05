using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BBN_Game.Controller;


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
        const int MAX_JOYSTICK = 62974;
        
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

        static Texture2D playerRed;
        static Texture2D playerRedBlack;
        static Texture2D playerBlue, playerBlueBlack;
        static Texture2D arrow;
        static Texture2D missileReload;
        static Texture2D missileReloadBlack;
        static Texture2D baseHealth, baseHealthBlack;

        private const float MissileReload = 7, MechinegunReload = 0.5f, DefensiveReload = 20;

        private float[] reloadTimer;

        protected Boolean twoPlayer;

        private KeyboardState oldState;

        private List<Objects.StaticObject> previousList;

        /// <summary>
        /// Getter and setter
        /// </summary>
        /// 

        #region "Map features"
        static Texture2D playerT, towerT, destroyerT, fighterT, baseT;
        public static Texture2D mapBackground;
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
            get { return cameraType.Equals(CurrentCam.Chase) ? new Camera.CameraMatrices(chaseCamera.view, chaseCamera.proj, chaseCamera.position, chaseCamera.viewingAnle, chaseCamera.frustrum) : new Camera.CameraMatrices(fpCamera.view, fpCamera.proj, fpCamera.Position, fpCamera.viewingAnle, fpCamera.frustrum); }
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
            this.acceleration = 15f;
            this.deceleration = 10;

            this.mass = 10;
            this.pitchSpeed = 40;
            this.rollSpeed = pitchSpeed * 1.5f;
            this.yawSpeed = pitchSpeed * 1.05f;
            this.maxSpeed = 30;
            this.minSpeed = -10;
            this.greatestLength = 10f;
            this.shipData.scale = 1f;

            this.numHudLines = 7;
            typeOfLine = PrimitiveType.LineList;

            Shield = 100;
            Health = 600;
            totalHealth = 600;
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
                model = Game.Content.Load<Model>("Models/Ships/playerShipRed");
            else
                model = Game.Content.Load<Model>("Models/Ships/playerShipBlue");

            f = Game.Content.Load<SpriteFont>("Fonts/pause_menu");

            // make sure that you get the static hud values
            playerRed = Game.Content.Load<Texture2D>("HudTextures/playerred");
            playerRedBlack = Game.Content.Load<Texture2D>("HudTextures/playerred_b");
            playerBlue = Game.Content.Load<Texture2D>("HudTextures/playerblue");
            playerBlueBlack = Game.Content.Load<Texture2D>("HudTextures/playerblue_b");

            arrow = Game.Content.Load<Texture2D>("HudTextures/arrow");
            playerT = Game.Content.Load<Texture2D>("HudTextures/arrow");
            mapBackground = Game.Content.Load<Texture2D>("HudTextures/map");
            towerT = Game.Content.Load<Texture2D>("HudTextures/diamond");
            destroyerT = Game.Content.Load<Texture2D>("HudTextures/square");
            fighterT = Game.Content.Load<Texture2D>("HudTextures/hexagon");
            baseT = Game.Content.Load<Texture2D>("HudTextures/circle");
            missileReload = Game.Content.Load<Texture2D>("HudTextures/rocket_c");
            missileReloadBlack = Game.Content.Load<Texture2D>("HudTextures/rocket_bw");
            baseHealth = Game.Content.Load<Texture2D>("HudTextures/base_w");
            baseHealthBlack = Game.Content.Load<Texture2D>("HudTextures/base");

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
            if (BBNGame.controllerMode.Equals(BBNGame.ControllerMode.GamePad) && GamePad.GetState(PlayerIndex.One).IsConnected)
                xboxControls((float)gt.ElapsedGameTime.TotalSeconds);
            else if (BBNGame.controllerMode.Equals(BBNGame.ControllerMode.KB))
                keyBoardChecks((float)gt.ElapsedGameTime.TotalSeconds);
            else if (BBNGame.controllerMode.Equals(BBNGame.ControllerMode.Joystick))
                joystickControls((float)gt.ElapsedGameTime.TotalSeconds);
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
            {/*
                #region "Extra Controls (Camera)"
                if (state.IsKeyDown(Keys.F1) && oldState.IsKeyUp(Keys.F1))
                    cameraType = cameraType.Equals(CurrentCam.Chase) ? CurrentCam.FirstPerson : CurrentCam.Chase;
                if (state.IsKeyDown(Keys.V) && oldState.IsKeyUp(Keys.V))
                    getNewTarget();
                if (state.IsKeyDown(Keys.B) && oldState.IsKeyUp(Keys.B))
                    drawMapBool = !drawMapBool;
                #endregion
                */
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
                    ((BBNGame)Game).totalTimeRotationY += time;
                }
                if (state.IsKeyDown(Keys.D))
                {
                    shipData.yaw -= yawSpeed * time;
                    ((BBNGame)Game).totalTimeRotationY += time;
                }
                #endregion
                #region "pitch & roll"
                float pitch = 0;
                float roll = 0;

                if (state.IsKeyDown(Keys.I))
                {
                    pitch = pitchSpeed * time;
                    ((BBNGame)Game).totalTimeRotationP += time;
                }
                else if (state.IsKeyDown(Keys.K))
                {
                    pitch = -pitchSpeed * time;
                    ((BBNGame)Game).totalTimeRotationP += time;
                }

                if (state.IsKeyDown(Keys.J))
                {
                    roll = (rollSpeed) * time;
                    ((BBNGame)Game).totalTimeRotationR += time;
                }
                else if (state.IsKeyDown(Keys.L))
                {
                    roll = -(rollSpeed) * time;
                    ((BBNGame)Game).totalTimeRotationR += time;
                }

                shipData.roll -= roll;
                shipData.pitch = mouseInverted ? shipData.pitch + pitch : shipData.pitch - pitch;
                #endregion

                // Debug
                if (state.IsKeyDown(Keys.Space) && oldState.IsKeyUp(Keys.Space))
                    mouseInverted = mouseInverted ? false : true;
                #endregion

                #region "Guns"
                /*if (state.IsKeyDown(Keys.F))
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
                }*/
                #endregion
            }
            #endregion
            
            oldState = state;
        }

        GamePadState prevPadState1 = GamePad.GetState(PlayerIndex.One);
        
        public void xboxControls(float time)
        {
            GamePadState pad1State = GamePad.GetState(PlayerIndex.One);

            #region Player 1
            if (index == PlayerIndex.One)
            {

                #region Extra Controls: Turret Capture & Camera

                if (pad1State.Buttons.LeftShoulder == ButtonState.Pressed && prevPadState1.Buttons.LeftShoulder == ButtonState.Released)
                    cameraType = cameraType.Equals(CurrentCam.Chase) ? CurrentCam.FirstPerson : CurrentCam.Chase;
                if (pad1State.Buttons.Y == ButtonState.Pressed && prevPadState1.Buttons.Y == ButtonState.Released)
                    drawMapBool = !drawMapBool;
                //select new target
                if (pad1State.Triggers.Left >= 0.6 && prevPadState1.Triggers.Left < 0.6)
                    getNewTarget();

                #endregion

                #region Rotations

                #region yaw

                if (pad1State.ThumbSticks.Left.X <= -0.5)
                {
                    shipData.yaw += yawSpeed * time;
                    ((BBNGame)Game).totalTimeRotationY += time;
                }
                if (pad1State.ThumbSticks.Left.X >= 0.5)
                {
                    shipData.yaw -= yawSpeed * time;
                    ((BBNGame)Game).totalTimeRotationY += time;
                }

                #endregion

                #region Pitch & Roll

                float pitch = 0;
                float roll = 0;

                if (pad1State.ThumbSticks.Right.Y >= 0.5)
                {
                    pitch = pitchSpeed * time;
                    ((BBNGame)Game).totalTimeRotationP += time;
                }
                else if (pad1State.ThumbSticks.Right.Y <= -0.5)
                {
                    pitch = -pitchSpeed * time;
                    ((BBNGame)Game).totalTimeRotationP += time;
                }

                if (pad1State.ThumbSticks.Right.X <= -0.5)
                {
                    roll = (rollSpeed) * time;
                    ((BBNGame)Game).totalTimeRotationR += time;
                }
                else if (pad1State.ThumbSticks.Right.X >= 0.5)
                {
                    roll = -(rollSpeed) * time;
                    ((BBNGame)Game).totalTimeRotationR += time;
                }

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
               /* if (pad1State.Buttons.B == ButtonState.Pressed)
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
                }*/

                #endregion
            }
            #endregion
            prevPadState1 = pad1State;
        }
        public void joystickControls(float time)
        {
            BBNGame gamePtr = (BBNGame)Game;
            if (gamePtr.joysticks.Count > 0)
            {
                SlimDX.DirectInput.JoystickState js = gamePtr.joysticks[0].GetCurrentState();
                #region rotations
                //roll
                if (js.X > MAX_JOYSTICK / 2 * 1.3f)
                {
                    shipData.roll += rollSpeed * ((js.X - (float)MAX_JOYSTICK / 2) / ((float)MAX_JOYSTICK / 2)) * time;
                    ((BBNGame)Game).totalTimeRotationR += time;
                }
                else if (js.X < MAX_JOYSTICK / 2 * 0.7f)
                {
                    shipData.roll -= rollSpeed * (1 - js.X / ((float)MAX_JOYSTICK / 2)) * time;
                    ((BBNGame)Game).totalTimeRotationR += time;
                }
                //pitch
                if (js.Y > MAX_JOYSTICK / 2 * 1.1)
                {
                    shipData.pitch -= pitchSpeed * ((js.Y - (float)MAX_JOYSTICK / 2) / ((float)MAX_JOYSTICK / 2)) * time;
                    ((BBNGame)Game).totalTimeRotationP += time;
                }
                else if (js.Y < MAX_JOYSTICK / 2 * 0.9)
                {
                    shipData.pitch += pitchSpeed * (1 - js.Y / ((float)MAX_JOYSTICK / 2)) * time;
                    ((BBNGame)Game).totalTimeRotationP += time;
                }
                System.Diagnostics.Debug.WriteLine(js.GetSliders()[0]);
                //yaw
                if (js.GetPointOfViewControllers()[0] == 27000)
                {
                    shipData.yaw += yawSpeed * time;
                    ((BBNGame)Game).totalTimeRotationY += time;
                }
                else if (js.GetPointOfViewControllers()[0] == 9000)
                {
                    shipData.yaw -= yawSpeed * time;
                    ((BBNGame)Game).totalTimeRotationY += time;
                }
                #endregion
                #region Acceleration
                if (js.GetButtons()[1])
                {
                    if (shipData.speed > minSpeed)
                    {
                        shipData.speed -= deceleration * time;
                    }
                }
                else if (js.GetButtons()[0])
                {
                    if (shipData.speed < maxSpeed)
                    {
                        shipData.speed += acceleration * time;
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
                #region guns
                //fire cannon
               /* if (js.GetButtons()[0])
                {
                    if (reloadTimer[0] <= 0)
                    {
                        Controller.GameController.addObject(new Objects.Bullet(Game, this.target, this));
                        reloadTimer[0] = MechinegunReload;
                    }
                }*/
                #endregion
            }
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


            if (target == null)
            {
                if (GameController.CheckPoints.Count > 0)
                {
                    target = GameController.CheckPoints.ElementAt(0);
                    GameController.CheckPoints.RemoveAt(0);
                }
            }
            else
            {
                if ((target.Position - this.Position).LengthSquared() < 10*10)
                    if (GameController.CheckPoints.Count > 0)
                    {
                        target = GameController.CheckPoints.ElementAt(0);
                        GameController.CheckPoints.RemoveAt(0);
                    }
                    else target = null;
            }
            reloadTimer[0] -= (float)gt.ElapsedGameTime.TotalSeconds;
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



            
            
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            Viewport viewport = Game.GraphicsDevice.Viewport;

            if (drawMapBool)
            {
                drawMap(sb, viewport);
                return;
            }


            int hudHeight = (int)((float)viewport.Height * 0.175f);
            int hudWidth = (int)((float)viewport.Width * 0.2f);

            sb.Begin();

            //sb.DrawString(f, 1 / gt.ElapsedGameTime.TotalSeconds + " - " + this.greatestLength, new Vector2(100, 100), Color.Green);

            #region "stats"
            
            sb.DrawString(f, "Statistics:", Vector2.Zero, Color.Lime);
            sb.DrawString(f, String.Format("{0:.00} seconds elapsed", Math.Round(((BBNGame)Game).totalElapsedTimeSeconds, 2)), new Vector2(0, 25), Color.Yellow);
            sb.DrawString(f, ((BBNGame)Game).numberOfHits+" hits", new Vector2(0, 50), Color.Yellow);
            sb.DrawString(f, String.Format("Yawed for {0:.00} seconds", Math.Round(((BBNGame)Game).totalTimeRotationY, 2)), new Vector2(0, 75), Color.Yellow);
            sb.DrawString(f, String.Format("Pitched for {0:.00} seconds", Math.Round(((BBNGame)Game).totalTimeRotationP, 2)), new Vector2(0, 100), Color.Yellow);
            sb.DrawString(f, String.Format("Rolled for {0:.00} seconds", Math.Round(((BBNGame)Game).totalTimeRotationR, 2)), new Vector2(0, 125), Color.Yellow);
            if (((BBNGame)Game).totalElapsedTimeSeconds > BBNGame.MAX_TIME)
            {
                sb.DrawString(f, "~~~ G A M E   O V E R ~~~", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2) - f.MeasureString("~~~ G A M E   O V E R ~~~") / 2, Color.LightPink);
            }
            if (target == null)
                if (BBNGame.mode.Equals(BBNGame.ExperimentMode.RealThing))
                    sb.DrawString(f, "THANK YOU FOR PARTICIPATING! :D", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2)- f.MeasureString("THANK YOU FOR PARTICIPATING! :D")/2, Color.LightBlue);
                else
                    sb.DrawString(f, "PRACTICE SESSION OVER. NOW FOR THE REAL DEAL...", new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2) - f.MeasureString("PRACTICE SESSION OVER. NOW FOR THE REAL DEAL...") / 2, Color.LightBlue);
            #endregion

            #region "Arrows"
            int width = (int)(Math.Min(viewport.Height, viewport.Width) * 0.05);
            float distance = Math.Min(viewport.Height, viewport.Width);
            
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

            GraphicsDevice.BlendState = BlendState.Additive;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        #region "Map drawing methods"
        private void drawMap(SpriteBatch sb, Viewport view)
        {
            List<StaticObject> objects = Controller.GameController.getAllObjects;

            float radius = Controller.GameController.mapRadius;
            radius = radius * 0.5f;

            sb.Begin();
            
                float objectWidth = view.Width * 0.015f;

            sb.Draw(mapBackground, new Rectangle(0, 0, view.Width, view.Height), Color.White);

            float wordDistance = 100;

            sb.Draw(playerT, new Rectangle((int)(view.Width * 0.05f), (int)(view.Height * 0.9f), (int)objectWidth, (int)objectWidth), Color.White);
            sb.DrawString(f, "-> Player", new Vector2((int)(view.Width * 0.05f) + objectWidth + 5, (int)(view.Height * 0.895f)), Color.White);
            sb.Draw(baseT, new Rectangle((int)(view.Width * 0.05f + objectWidth + 5 + wordDistance), (int)(view.Height * 0.9f), (int)objectWidth, (int)objectWidth), Color.White);
            sb.DrawString(f, "-> Next Checkpoint", new Vector2((int)(view.Width * 0.05f + ((objectWidth + 5) * 2 + wordDistance)), (int)(view.Height * 0.895f)), Color.White);

            List<StaticObject> printList = new List<StaticObject>();
            printList.Add(this);
            if (target != null)
                printList.Add(target);
            foreach (StaticObject obj in printList)
            {

                    Vector2 pos = new Vector2(obj.Position.X, obj.Position.Z);
                    pos = new Vector2(obj.Position.X + radius, pos.Y + radius);
                    pos = new Vector2(obj.Position.X / (radius * 2), pos.Y / (radius * 2));
                    pos = new Vector2(obj.Position.X * view.Width, pos.Y * (view.Height * 1.4f));


                    Color c = Color.Yellow;                    

                    Texture2D tex = obj is playerObject ? playerT : baseT;

                    if (obj is playerObject)
                    {
                        Vector3 oPos = obj.Position + Vector3.Transform(new Vector3(0, 0, 10), Matrix.CreateFromQuaternion(obj.rotation));
                        Vector3 A = oPos - obj.Position;

                        float x = Vector3.Dot(A, Vector3.UnitX);
                        float y = Vector3.Dot(A, Vector3.UnitZ);

                        float angle = (float)Math.Atan2(y, x);
                        sb.Draw(tex, new Rectangle((int)(pos.X - objectWidth / 2), (int)(pos.Y - objectWidth / 2), (int)objectWidth, (int)objectWidth),
                                     new Rectangle(0, 0, tex.Width, tex.Height), c, angle, new Vector2(tex.Width / 2, tex.Height / 2), SpriteEffects.None, 0);
                    }
                    else
                        sb.Draw(tex, new Rectangle((int)(pos.X - objectWidth / 2), (int)(pos.Y - objectWidth / 2), (int)objectWidth, (int)objectWidth), c);
            }
            sb.End();
        }
        #endregion

        #endregion
    }
}
