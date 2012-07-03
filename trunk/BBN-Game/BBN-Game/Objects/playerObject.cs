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


        // debug
        SpriteFont f;

        #region "Globals"
        /// <summary>
        /// Global variables
        /// Index is the player index for xbox
        /// OrigState is the state of the mouse when centered (Computer controls)
        /// The width and height are storage variables for the width and height of the viewport of the game
        /// Mouse inverted is another variable to determine the motion of the mouse (joystick)
        /// </summary>
        PlayerIndex index;
        MouseState origState;

        float width, height;

        public Boolean mouseInverted = true;

        #endregion

        #region "Constructors - Data setting"

        /// <summary>
        /// Ovveride to set the data for rotation speeds etc...
        /// </summary>
        protected override void setData()
        {
            this.acceleration = 20f;
            this.deceleration = 15;

            this.rollSpeed = 25;
            this.pitchSpeed = 25;
            this.yawSpeed = rollSpeed * 2;
            this.maxSpeed = 50;
            this.minSpeed = -10;
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="game"></param>
        public playerObject(Game game)
            : base(game)
        {
        }

        /// <summary>
        /// Constructor with a index (multiplayer on xbox use)
        /// </summary>
        /// <param name="game"></param>
        /// <param name="index"></param>
        public playerObject(Game game, PlayerIndex index)
            : base(game)
        {
            this.index = index;
        }

        /// <summary>
        /// Load content laods the players Model
        /// </summary>
        /// <param name="player">The palyer index (Team 1 or 2)</param>
        public void LoadContent(int player)
        {
            this.model = Game.Content.Load<Model>("Models/Ships/Fighter" + (player == 1 ? "Red" : "Blue"));
            this.f = Game.Content.Load<SpriteFont>("SpriteFont1");
            base.LoadContent();
        }

        /// <summary>
        /// Initialises the variables
        /// And for computer gets the mouse ready to control the ship
        /// </summary>
        public void Initialize()
        {
            this.width = Game.GraphicsDevice.Viewport.Width;
            this.height = Game.GraphicsDevice.Viewport.Height;

            resetMouse();
            origState = Mouse.GetState();

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

            #region "Accel Deccel checks"
            if (state.IsKeyDown(Keys.W))
            {
                if (shipData.speed < maxSpeed)
                {
                    shipData.speed += acceleration * time;

                    if (shipData.speed > maxSpeed)
                        shipData.speed = maxSpeed;
                }
            }
            else if (state.IsKeyDown(Keys.S))
            {
                if (shipData.speed > minSpeed)
                {
                    shipData.speed -= deceleration * time;

                    if (shipData.speed < minSpeed)
                        shipData.speed = minSpeed;
                }
            }
            else
            {
                if (shipData.speed > 0)
                {
                    shipData.speed -= deceleration * time;

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

            MouseState mState = Mouse.GetState();
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
            float pitch = (origState.Y - mState.Y) * pitchSpeed * time;
            float roll = (origState.X - mState.X) * rollSpeed * time;

            if (state.IsKeyDown(Keys.Up))
                pitch = pitchSpeed * 5 * time;
            else if (state.IsKeyDown(Keys.Down))
                pitch = -pitchSpeed * 5 * time;

            if (state.IsKeyDown(Keys.Left))
                roll = (rollSpeed * 5) * time;
            else if (state.IsKeyDown(Keys.Right))
                roll = -(rollSpeed * 5) * time;

            shipData.roll -= roll;
            shipData.pitch = mouseInverted ? shipData.pitch + pitch : shipData.pitch - pitch;
            #endregion

            // Debug
            if (state.IsKeyDown(Keys.Space))
                mouseInverted = mouseInverted ? false : true;

            resetMouse();
            #endregion
        }

        /// <summary>
        /// Resets the mouse position to the center of the screen
        /// So that it can determine movement of mouse
        /// </summary>
        private void resetMouse()
        {
            Mouse.SetPosition((int)width / 2, (int)height / 2);
        }

        #endregion

        /// for debug
        #region "Debug"
        public void Draw(GameTime gameTime, BBN_Game.Camera.CameraMatrices cam, SpriteBatch b, Objects.Destroyer target)
        {
            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            b.Begin();
            b.DrawString(f, "Speed: " + shipData.speed, new Vector2(10, 10), Color.Red);

            b.DrawString(f, "W/S - Accelerate/Decelerate and Move backward", new Vector2(10, 110), Color.Red);
            b.DrawString(f, "A/D - Yaw (left/right respectively)", new Vector2(10, 130), Color.Red);
            b.DrawString(f, "Mouse - Pitch/roll (respective to normal)- Alternatively (Arrow keys)", new Vector2(10, 150), Color.Red);
            b.DrawString(f, "Space - Mouse inverted (pitch)", new Vector2(10, 170), Color.Red);
            
            if (target.IsVisible(cam))
                b.DrawString(f, "Target Visible", new Vector2(Game.GraphicsDevice.Viewport.Width / 2 - 100, 0), Color.Red);


            b.End();
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            

            base.Draw(gameTime, cam);
        }
        #endregion
    }
}
