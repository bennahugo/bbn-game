using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.Controller;

#region "XNA using statements"
using System.Runtime.InteropServices; //for messageboxes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Audio;
#endregion

/**
 * Author: Nathan Floor(FLRNAT001)
 * 
 * This class controls the menu system for the game.
 * The menu's provided for this game are the main, pause and in-game menus.
 * 
 */

namespace BBN_Game.Menu
{
    class MenuController
    {
        #region Instance Variables

        GraphicsDevice graphics;
        ContentManager Content;
        SpriteBatch spriteBatch;
        BBN_Game.BBNGame game;
        GameController gameController;

        //game states
        GameState currentState;
        GameState previousState;
        //Players currentPlayer;

        int screenWidth;
        int screenHeight;

        #region Content variables

        //backgrounds
        private Texture2D mainMenuTex;
        private Texture2D pauseMenuTex;
        private Texture2D controlsMenuTex;
        private Texture2D xboxControlsTex;
        private Texture2D keybrdControlsTex;
        private Texture2D tradeMenuTex;
        private Texture2D transparentBackgrnd;

        //stats icons
        private Texture2D fighterTex;
        private Texture2D destroyerTex;
        private Texture2D missileTex;

        //menu fonts
        SpriteFont generalMenuFont;
        SpriteFont selectedMenuFont;
        SpriteFont pauseMenuFont;
        SpriteFont tradeMenuFont;

        #endregion

        //manage menu options navigation
        int currentMenuOption = 1;
        Boolean displayXboxControls = false;

        #endregion

        //constructor
        public MenuController(GameController controller, BBN_Game.BBNGame g)
        {
            gameController = controller;
            this.game = g;
            graphics = g.GraphicsDevice;
            spriteBatch = new SpriteBatch(g.GraphicsDevice);
            currentState = controller.CurrentGameState;
            previousState = currentState;
            Content = g.Content;
            screenHeight = graphics.Viewport.Height;
            screenWidth = graphics.Viewport.Width;
        }

        public void updateState()
        {
            currentState = gameController.CurrentGameState;
        }

        public void loadContent()
        {
            //load menu backgrounds
            mainMenuTex = Content.Load<Texture2D>("Menu/nebula_blue");
            pauseMenuTex = Content.Load<Texture2D>("Menu/pause_menu");
            controlsMenuTex = Content.Load<Texture2D>("Menu/industrial");
            xboxControlsTex = Content.Load<Texture2D>("Menu/xbox_controls");
            keybrdControlsTex = Content.Load<Texture2D>("Menu/keyboard_controls");
            tradeMenuTex = Content.Load<Texture2D>("Menu/trade_menu_newest");
            transparentBackgrnd = Content.Load<Texture2D>("Menu/dark_trans_new");

            //load stats icons
            destroyerTex = Content.Load<Texture2D>("stats_icons/malpha_small");
            fighterTex = Content.Load<Texture2D>("stats_icons/fighter_small");
            missileTex = Content.Load<Texture2D>("stats_icons/missile_small");

            //load fonts
            generalMenuFont = Content.Load<SpriteFont>("Fonts/menuFont");
            selectedMenuFont = Content.Load<SpriteFont>("Fonts/selectedFont");
            pauseMenuFont = Content.Load<SpriteFont>("Fonts/pause_menu");
            tradeMenuFont = Content.Load<SpriteFont>("Fonts/trade_menu");
        }

        public void unloadContent()
        {
            //do nothing for now
        }

        #region Controls
        //manage keyboard input
        KeyboardState prevKeyState = Keyboard.GetState();
        private void handleKeyboardControls()
        {
            KeyboardState keyState = Keyboard.GetState();

            if (currentState == GameState.MainMenu)
            {
                #region Main menu

                if (keyState.IsKeyDown(Keys.Down) && prevKeyState.IsKeyUp(Keys.Down))//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;

                //selecting menu options
                if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
                {
                    previousState = currentState;
                    if (currentMenuOption == 1)//start new single player game
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }                    
                    else if (currentMenuOption == 2)
                    {
                        //start new multiplayer game
                        Controller.GameController.NumberOfPlayers = Players.two;
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                        currentMenuOption = 1;
                    }
                    else if (currentMenuOption == 3)//look at and modify game options
                    {
                        currentMenuOption = 1;
                        currentState = GameState.OptionsMenu;
                    }
                    else if (currentMenuOption == 4)//exit game
                        game.Exit();
                }
                #endregion
            }
            else if (currentState == GameState.OptionsMenu)
            {
                #region Controls Menu
                if (keyState.IsKeyDown(Keys.Down) && prevKeyState.IsKeyUp(Keys.Down))//move down menu
                    if (currentMenuOption < 3)
                        currentMenuOption++;
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;

                //selecting menu options TODO
                if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
                {
                    if (currentMenuOption == 1)//display keyboard controls
                    {
                        displayXboxControls = false;
                    }
                    else if (currentMenuOption == 2)//display xbox controls
                    {
                        displayXboxControls = true;
                    }
                    else if (currentMenuOption == 3)//return to previous game-state
                    {
                        currentState = previousState;
                        currentMenuOption = 1;
                    }                   
                }
                #endregion
            }
            else if (currentState == GameState.Paused)
            {
                #region Pause menu
                if (keyState.IsKeyDown(Keys.Down) && prevKeyState.IsKeyUp(Keys.Down))//move down menu
                    if (currentMenuOption < 5)
                        currentMenuOption++;
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;

                //selecting menu options
                if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
                {
                    previousState = currentState;
                    if (currentMenuOption == 1)//resume gameplay
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)//restart current game TODO
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 3)//quite current game & return to main menu
                    {
                        // TODO - quite current game

                        currentMenuOption = 1;
                        currentState = GameState.MainMenu;
                    }
                    else if (currentMenuOption == 4)//look at game controls
                    {
                        currentMenuOption = 1;
                        currentState = GameState.OptionsMenu;
                    }
                    else if (currentMenuOption == 5)//exit game entirely
                        game.Exit();
                }
                #endregion
            }
            else if (currentState == GameState.notLoaded)
            {
                //TODO 
            }
            prevKeyState = keyState;
        }

        //manage xbox controls
        GamePadState prevPadState1 = GamePad.GetState(PlayerIndex.One);
        GamePadState prevPadState2 = GamePad.GetState(PlayerIndex.Two);
        int menuDelay = 7;
        private void handleXboxControls()
        {
            GamePadState padState1 = GamePad.GetState(PlayerIndex.One);
            GamePadState padState2 = GamePad.GetState(PlayerIndex.Two);           

            if (currentState == GameState.MainMenu && menuDelay <= 0)
            {
                #region Main Menu

                #region Player 1
                if (padState1.ThumbSticks.Left.Y <= -0.5)//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (padState1.ThumbSticks.Left.Y >= 0.5)//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                menuDelay = 7;

                if (padState1.Buttons.A == ButtonState.Pressed && prevPadState1.Buttons.A == ButtonState.Released)
                {
                    if (currentMenuOption == 1)//start new single player game
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)
                    {
                        //start new multiplayer game
                        Controller.GameController.NumberOfPlayers = Players.two;
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                        currentMenuOption = 1;
                    }
                    else if (currentMenuOption == 3)//look at and modify game options
                    {
                        currentMenuOption = 1;
                        currentState = GameState.OptionsMenu;
                    }
                    else if (currentMenuOption == 4)//exit game
                        game.Exit();
                }

                #endregion

                #region Player 2
                if (padState2.ThumbSticks.Left.Y <= -0.5)//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (padState2.ThumbSticks.Left.Y >= 0.5)//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                menuDelay = 7;

                if (padState2.Buttons.A == ButtonState.Pressed && prevPadState2.Buttons.A == ButtonState.Released)
                {
                    if (currentMenuOption == 1)//start new single player game
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)
                    {
                        //start new multiplayer game
                        Controller.GameController.NumberOfPlayers = Players.two;
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                        currentMenuOption = 1;
                    }
                    else if (currentMenuOption == 3)//look at and modify game options
                    {
                        currentMenuOption = 1;
                        currentState = GameState.OptionsMenu;
                    }
                    else if (currentMenuOption == 4)//exit game
                        game.Exit();
                }
                #endregion

                
                #endregion
            }
            else if (currentState == GameState.OptionsMenu && menuDelay <= 0)
            {
                #region Control Menu

                #region player 1
                if (padState1.ThumbSticks.Left.Y <= -0.5)//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (padState1.ThumbSticks.Left.Y >= 0.5)//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                menuDelay = 7;

                if (padState1.Buttons.A == ButtonState.Pressed && prevPadState1.Buttons.A == ButtonState.Released)
                {
                    if (currentMenuOption == 1)//display keyboard controls
                        displayXboxControls = false;
                    else if (currentMenuOption == 2)//display xbox controls
                        displayXboxControls = true;
                    else if (currentMenuOption == 3)
                    {
                        currentState = previousState;
                        currentMenuOption = 1;
                    }
                }
                #endregion

                #region player 2
                if (padState2.ThumbSticks.Left.Y <= -0.5)//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (padState2.ThumbSticks.Left.Y >= 0.5)//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                menuDelay = 7;

                if (padState2.Buttons.A == ButtonState.Pressed && prevPadState2.Buttons.A == ButtonState.Released)
                {
                    if (currentMenuOption == 1)//display keyboard controls
                        displayXboxControls = false;
                    else if (currentMenuOption == 2)//display xbox controls
                        displayXboxControls = true;
                    else if (currentMenuOption == 3)
                    {
                        currentState = previousState;
                        currentMenuOption = 1;
                    }
                }
                #endregion
                                
                #endregion
            }
            else if (currentState == GameState.Paused && menuDelay <= 0)
            {
                #region Pause Menu

                #region Player 1
                if (padState1.ThumbSticks.Left.Y <= -0.5)//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (padState1.ThumbSticks.Left.Y >= 0.5)//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                menuDelay = 7;

                if (padState1.Buttons.A == ButtonState.Pressed && prevPadState1.Buttons.A == ButtonState.Released)
                {
                    if (currentMenuOption == 1)//resume gameplay
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)//quit current game & return to main menu
                    {
                        // TODO - quit current game

                        currentMenuOption = 1;
                        currentState = GameState.MainMenu;
                    }
                    else if (currentMenuOption == 3)//look at game controls
                    {
                        currentMenuOption = 1;
                        currentState = GameState.OptionsMenu;
                    }
                    else if (currentMenuOption == 4)//exit game entirely
                        game.Exit();
                }

                #endregion

                #region Player 2
                if (padState2.ThumbSticks.Left.Y <= -0.5)//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (padState2.ThumbSticks.Left.Y >= 0.5)//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                menuDelay = 7;

                if (padState2.Buttons.A == ButtonState.Pressed && prevPadState2.Buttons.A == ButtonState.Released)
                {
                    if (currentMenuOption == 1)//resume gameplay
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)//quit current game & return to main menu
                    {
                        // TODO - quit current game

                        currentMenuOption = 1;
                        currentState = GameState.MainMenu;
                    }
                    else if (currentMenuOption == 3)//look at game controls
                    {
                        currentMenuOption = 1;
                        currentState = GameState.OptionsMenu;
                    }
                    else if (currentMenuOption == 4)//exit game entirely
                        game.Exit();
                }

                #endregion

                #endregion
            }
            else if (currentState == GameState.notLoaded)
            {

            }
            prevPadState1 = padState1;
            prevPadState2 = padState2;
        }
        #endregion
        //manage menu navigation
        public void updateMenu(GameTime gameTime)
        {
            if (menuDelay > 0)
                menuDelay--;

            if (GamePad.GetState(PlayerIndex.One).IsConnected)
                handleXboxControls();
            else
                handleKeyboardControls();
            //include menu animations here TODO
        }

        #region drawing methods            
        
        public void drawTradeMenu(Objects.playerObject player)
        {
            //for slide-up animation
            if (player.UpFactor > 0 && player.GoingUp)
                player.UpFactor -= 2;
            else if ((!player.GoingUp) && player.UpFactor < 150)
                player.UpFactor += 2;

            spriteBatch.Begin();

            //draw trade menu pop-up panel and menu outline for options
            spriteBatch.Draw(tradeMenuTex, new Rectangle(player.getViewport.Width - 160, player.getViewport.Height - 300 + player.UpFactor, 150, 276), Color.White);

            String currency = "$10 000";
            spriteBatch.DrawString(tradeMenuFont, currency, new Vector2(player.getViewport.Width - 108, player.getViewport.Height - 243 + player.UpFactor), Color.Black);
            spriteBatch.DrawString(tradeMenuFont, currency, new Vector2(player.getViewport.Width - 109, player.getViewport.Height - 244 + player.UpFactor), Color.Black);
            spriteBatch.DrawString(tradeMenuFont, currency, new Vector2(player.getViewport.Width - 110, player.getViewport.Height - 245 + player.UpFactor), Color.Red);

            #region draw menu options
            if (player.TradeMenuOption == 1)
            {
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 112, player.getViewport.Height - 218 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 111, player.getViewport.Height - 219 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 110, player.getViewport.Height - 220 + player.UpFactor), Color.Aqua);

                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 100, player.getViewport.Height - 164 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 101, player.getViewport.Height - 165 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 102, player.getViewport.Height - 166 + player.UpFactor), Color.Green);

                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 100, player.getViewport.Height - 108 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 101, player.getViewport.Height - 109 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 102, player.getViewport.Height - 110 + player.UpFactor), Color.Green);
            }
            else if (player.TradeMenuOption == 2)
            {
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 108, player.getViewport.Height - 218 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 109, player.getViewport.Height - 219 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 110, player.getViewport.Height - 220 + player.UpFactor), Color.Green);

                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 104, player.getViewport.Height - 164 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 103, player.getViewport.Height - 165 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 102, player.getViewport.Height - 166 + player.UpFactor), Color.Aqua);

                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 100, player.getViewport.Height - 108 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 101, player.getViewport.Height - 109 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 102, player.getViewport.Height - 110 + player.UpFactor), Color.Green);
            }
            else if (player.TradeMenuOption == 3)
            {
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 108, player.getViewport.Height - 218 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 109, player.getViewport.Height - 219 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer", new Vector2(player.getViewport.Width - 110, player.getViewport.Height - 220 + player.UpFactor), Color.Green);

                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 100, player.getViewport.Height - 164 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 101, player.getViewport.Height - 165 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter", new Vector2(player.getViewport.Width - 102, player.getViewport.Height - 166 + player.UpFactor), Color.Green);

                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 104, player.getViewport.Height - 108 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 103, player.getViewport.Height - 109 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile", new Vector2(player.getViewport.Width - 102, player.getViewport.Height - 110 + player.UpFactor), Color.Aqua);
            }
            #endregion

            #region Draw menu values

            spriteBatch.DrawString(tradeMenuFont, "You have: " + 2, new Vector2(player.getViewport.Width - 105, player.getViewport.Height - 195 + player.UpFactor), Color.Red);
            spriteBatch.DrawString(tradeMenuFont, "You have: " + 3, new Vector2(player.getViewport.Width - 105, player.getViewport.Height - 141 + player.UpFactor), Color.Red);
            spriteBatch.DrawString(tradeMenuFont, "You have: " + player.Missiles, new Vector2(player.getViewport.Width - 105, player.getViewport.Height - 85 + player.UpFactor), Color.Red);

            #endregion

            spriteBatch.End();
        }

        public void drawTradeStats(Objects.playerObject player)
        {
            spriteBatch.Begin();

            //draw icons for ship-counts and missile-counts
            spriteBatch.Draw(destroyerTex, new Rectangle(player.getViewport.Width - 280, player.getViewport.Height - 50, 64, 36), Color.White);
            spriteBatch.DrawString(tradeMenuFont, "2", new Vector2(player.getViewport.Width - 290, player.getViewport.Height - 50), Color.Red);
            spriteBatch.Draw(fighterTex, new Rectangle(player.getViewport.Width - 180, player.getViewport.Height - 50, 64, 41), Color.White);
            spriteBatch.DrawString(tradeMenuFont, "3", new Vector2(player.getViewport.Width - 190, player.getViewport.Height - 50), Color.Red);
            spriteBatch.Draw(missileTex, new Rectangle(player.getViewport.Width - 70, player.getViewport.Height - 50, 64, 48), Color.White);
            spriteBatch.DrawString(tradeMenuFont, "" + player.Missiles, new Vector2(player.getViewport.Width - 80, player.getViewport.Height - 50), Color.Red);

            spriteBatch.End();
        }

        //update what menu is currently being displayed
        public void drawMenu(GameTime gameTime)
        {
            if (currentState == GameState.MainMenu)
            {
                #region Main Menu
                graphics.Clear(Color.Black);
                spriteBatch.Begin();

                //draw main menu background and menu outline for options
                spriteBatch.Draw(mainMenuTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 47, 69), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 48, 68), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 49, 67), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 50, 66), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 51, 65), Color.Blue);

                //display menu options
                if (currentMenuOption == 1)
                {
                    spriteBatch.DrawString(selectedMenuFont, "Single Player", new Vector2(screenWidth - 244, (screenHeight / 2) - 46), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Single Player", new Vector2(screenWidth - 243, (screenHeight / 2) - 47), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Single Player", new Vector2(screenWidth - 242, (screenHeight / 2) - 48), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Single Player", new Vector2(screenWidth - 241, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Single Player", new Vector2(screenWidth - 240, (screenHeight / 2) - 50), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 196, (screenHeight / 2) + 4), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 197, (screenHeight / 2) + 3), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 198, (screenHeight / 2) + 2), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 196, (screenHeight / 2) + 54), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 197, (screenHeight / 2) + 53), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 198, (screenHeight / 2) + 52), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 196, (screenHeight / 2) + 104), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 197, (screenHeight / 2) + 103), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 198, (screenHeight / 2) + 102), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Blue);
                }
                else if (currentMenuOption == 2)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 196, (screenHeight / 2) - 46), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 197, (screenHeight / 2) - 47), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 198, (screenHeight / 2) - 48), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Blue);

                    spriteBatch.DrawString(selectedMenuFont, "MultiPlayer", new Vector2(screenWidth - 244, (screenHeight / 2) + 4), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "MultiPlayer", new Vector2(screenWidth - 243, (screenHeight / 2) + 3), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "MultiPlayer", new Vector2(screenWidth - 242, (screenHeight / 2) + 2), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "MultiPlayer", new Vector2(screenWidth - 241, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "MultiPlayer", new Vector2(screenWidth - 240, (screenHeight / 2)), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 196, (screenHeight / 2) + 54), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 197, (screenHeight / 2) + 53), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 198, (screenHeight / 2) + 52), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 196, (screenHeight / 2) + 104), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 197, (screenHeight / 2) + 103), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 198, (screenHeight / 2) + 102), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Blue);
                }
                else if (currentMenuOption == 3)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 196, (screenHeight / 2) - 46), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 197, (screenHeight / 2) - 47), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 198, (screenHeight / 2) - 48), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 196, (screenHeight / 2) + 4), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 197, (screenHeight / 2) + 3), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 198, (screenHeight / 2) + 2), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Blue);

                    spriteBatch.DrawString(selectedMenuFont, "Controls", new Vector2(screenWidth - 244, (screenHeight / 2) + 54), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Controls", new Vector2(screenWidth - 243, (screenHeight / 2) + 53), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Controls", new Vector2(screenWidth - 242, (screenHeight / 2) + 52), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Controls", new Vector2(screenWidth - 241, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Controls", new Vector2(screenWidth - 240, (screenHeight / 2) + 50), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 196, (screenHeight / 2) + 104), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 197, (screenHeight / 2) + 103), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 198, (screenHeight / 2) + 102), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Blue);
                }
                else if (currentMenuOption == 4)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 196, (screenHeight / 2) - 46), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 197, (screenHeight / 2) - 47), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 198, (screenHeight / 2) - 48), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 196, (screenHeight / 2) + 4), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 197, (screenHeight / 2) + 3), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 198, (screenHeight / 2) + 2), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 196, (screenHeight / 2) + 54), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 197, (screenHeight / 2) + 53), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 198, (screenHeight / 2) + 52), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Blue);

                    spriteBatch.DrawString(selectedMenuFont, "Exit Game", new Vector2(screenWidth - 244, (screenHeight / 2) + 104), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Exit Game", new Vector2(screenWidth - 243, (screenHeight / 2) + 103), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Exit Game", new Vector2(screenWidth - 242, (screenHeight / 2) + 102), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Exit Game", new Vector2(screenWidth - 241, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Exit Game", new Vector2(screenWidth - 240, (screenHeight / 2) + 100), Color.Aqua);
                }

                spriteBatch.End();

                #endregion
            }
            else if (currentState == GameState.OptionsMenu)
            {
                #region Controls Menu
                graphics.Clear(Color.Black);
                spriteBatch.Begin();

                //draw main menu background and menu outline for options
                if (! displayXboxControls)
                {
                    spriteBatch.Draw(controlsMenuTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                    spriteBatch.Draw(keybrdControlsTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                }
                else                
                    spriteBatch.Draw(xboxControlsTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);                
                
                //spriteBatch.Draw(transparentBackgrnd, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.DrawString(selectedMenuFont, "Game Controls", new Vector2(7, 2), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Game Controls", new Vector2(8, 3), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Game Controls", new Vector2(9, 4), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Game Controls", new Vector2(10, 5), Color.Blue);

                #region Draw menu options

                if (currentMenuOption == 1)
                {
                    spriteBatch.DrawString(selectedMenuFont, "Keyboard Controls", new Vector2(13, 102), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Keyboard Controls", new Vector2(12, 103), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Keyboard Controls", new Vector2(11, 104), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Keyboard Controls", new Vector2(10, 105), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "Xbox Controls", new Vector2(7, 152), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox Controls", new Vector2(8, 153), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox Controls", new Vector2(9, 154), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox Controls", new Vector2(10, 155), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(7, 202), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(8, 203), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(9, 204), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(10, 205), Color.Blue);
                }
                else if (currentMenuOption == 2)
                {
                    spriteBatch.DrawString(generalMenuFont, "Keyboard Controls", new Vector2(7, 102), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard Controls", new Vector2(8, 103), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard Controls", new Vector2(9, 104), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard Controls", new Vector2(10, 105), Color.Blue);

                    spriteBatch.DrawString(selectedMenuFont, "Xbox Controls", new Vector2(13, 152), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Xbox Controls", new Vector2(12, 153), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Xbox Controls", new Vector2(11, 154), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Xbox Controls", new Vector2(10, 155), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(7, 202), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(8, 203), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(9, 204), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(10, 205), Color.Blue);
                }
                else if (currentMenuOption == 3)
                {
                    spriteBatch.DrawString(generalMenuFont, "Keyboard Controls", new Vector2(7, 102), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard Controls", new Vector2(8, 103), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard Controls", new Vector2(9, 104), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard Controls", new Vector2(10, 105), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Xbox Controls", new Vector2(7, 152), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox Controls", new Vector2(8, 153), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox Controls", new Vector2(9, 154), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox Controls", new Vector2(10, 155), Color.Blue);

                    spriteBatch.DrawString(selectedMenuFont, "Back", new Vector2(13, 202), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Back", new Vector2(12, 203), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Back", new Vector2(11, 204), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Back", new Vector2(10, 205), Color.Aqua);
                }

                #endregion
                                
                spriteBatch.End();
                #endregion
            }
            else if (currentState == GameState.Paused)
            {
                #region Paused Menu
                //graphics.Clear(Color.Black);
                spriteBatch.Begin();

                //draw main menu background and menu outline for options
                //TODO - draw tranparent background over paused game screen
                String pauseTitle = "Game Paused...";
                //spriteBatch.Draw(mainMenuTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.Draw(transparentBackgrnd, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.Draw(pauseMenuTex, new Rectangle(screenWidth / 2 - 300, screenHeight / 2 - 300, 600, 600), Color.White);
                spriteBatch.DrawString(generalMenuFont, pauseTitle, new Vector2((screenWidth / 2) - pauseTitle.Length - 87, (screenHeight / 2) - 127), Color.Black);
                spriteBatch.DrawString(generalMenuFont, pauseTitle, new Vector2((screenWidth / 2) - pauseTitle.Length - 88, (screenHeight / 2) - 128), Color.Black);
                spriteBatch.DrawString(generalMenuFont, pauseTitle, new Vector2((screenWidth / 2) - pauseTitle.Length - 89, (screenHeight / 2) - 129), Color.Black);
                spriteBatch.DrawString(generalMenuFont, pauseTitle, new Vector2((screenWidth / 2) - pauseTitle.Length - 90, (screenHeight / 2) - 130), Color.Blue);

                //display menu options
                String resume = "Resume Game";
                String restart = "Restart Game";
                String quiteCurrent = "Quit Current Game";
                String controls = "Controls";
                String exitGame = "Exit Game";
                if (currentMenuOption == 1)
                {
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 48, (screenHeight / 2) - 74), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 47, (screenHeight / 2) - 75), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 46, (screenHeight / 2) - 76), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 45, (screenHeight / 2) - 77), Color.Aqua);
                    
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 40, (screenHeight / 2) - 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 41, (screenHeight / 2) - 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 42, (screenHeight / 2) - 29), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 43, (screenHeight / 2) - 30), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 55, (screenHeight / 2) + 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 56, (screenHeight / 2) + 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 57, (screenHeight / 2) + 26), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 58, (screenHeight / 2) + 25), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 22, (screenHeight / 2) + 80), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 23, (screenHeight / 2) + 79), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 24, (screenHeight / 2) + 78), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 25, (screenHeight / 2) + 77), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 27, (screenHeight / 2) + 133), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 28, (screenHeight / 2) + 132), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 29, (screenHeight / 2) + 131), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 30, (screenHeight / 2) + 130), Color.Blue);
                }
                else if (currentMenuOption == 2)
                {
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 42, (screenHeight / 2) - 74), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 43, (screenHeight / 2) - 75), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 44, (screenHeight / 2) - 76), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 45, (screenHeight / 2) - 77), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 46, (screenHeight / 2) - 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 45, (screenHeight / 2) - 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 44, (screenHeight / 2) - 29), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 43, (screenHeight / 2) - 30), Color.Aqua);

                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 55, (screenHeight / 2) + 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 56, (screenHeight / 2) + 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 57, (screenHeight / 2) + 26), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 58, (screenHeight / 2) + 25), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 22, (screenHeight / 2) + 80), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 23, (screenHeight / 2) + 79), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 24, (screenHeight / 2) + 78), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 25, (screenHeight / 2) + 77), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 27, (screenHeight / 2) + 133), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 28, (screenHeight / 2) + 132), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 29, (screenHeight / 2) + 131), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 30, (screenHeight / 2) + 130), Color.Blue);
                }
                else if (currentMenuOption == 3)
                {
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 42, (screenHeight / 2) - 74), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 43, (screenHeight / 2) - 75), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 44, (screenHeight / 2) - 76), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 45, (screenHeight / 2) - 77), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 40, (screenHeight / 2) - 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 41, (screenHeight / 2) - 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 42, (screenHeight / 2) - 29), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 43, (screenHeight / 2) - 30), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 61, (screenHeight / 2) + 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 60, (screenHeight / 2) + 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 59, (screenHeight / 2) + 26), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 58, (screenHeight / 2) + 25), Color.Aqua);

                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 22, (screenHeight / 2) + 80), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 23, (screenHeight / 2) + 79), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 24, (screenHeight / 2) + 78), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 25, (screenHeight / 2) + 77), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 27, (screenHeight / 2) + 133), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 28, (screenHeight / 2) + 132), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 29, (screenHeight / 2) + 131), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 30, (screenHeight / 2) + 130), Color.Blue);
                }
                else if (currentMenuOption == 4)
                {
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 42, (screenHeight / 2) - 74), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 43, (screenHeight / 2) - 75), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 44, (screenHeight / 2) - 76), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 45, (screenHeight / 2) - 77), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 40, (screenHeight / 2) - 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 41, (screenHeight / 2) - 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 42, (screenHeight / 2) - 29), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 43, (screenHeight / 2) - 30), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 55, (screenHeight / 2) + 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 56, (screenHeight / 2) + 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 57, (screenHeight / 2) + 26), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 58, (screenHeight / 2) + 25), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 28, (screenHeight / 2) + 80), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 27, (screenHeight / 2) + 79), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 26, (screenHeight / 2) + 78), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 25, (screenHeight / 2) + 77), Color.Aqua);

                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 27, (screenHeight / 2) + 133), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 28, (screenHeight / 2) + 132), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 29, (screenHeight / 2) + 131), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 30, (screenHeight / 2) + 130), Color.Blue);
                }
                else if (currentMenuOption == 5)
                {
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 42, (screenHeight / 2) - 74), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 43, (screenHeight / 2) - 75), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 44, (screenHeight / 2) - 76), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, resume, new Vector2((screenWidth / 2) - resume.Length - 45, (screenHeight / 2) - 77), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 40, (screenHeight / 2) - 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 41, (screenHeight / 2) - 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 42, (screenHeight / 2) - 29), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, restart, new Vector2((screenWidth / 2) - restart.Length - 43, (screenHeight / 2) - 30), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 55, (screenHeight / 2) + 28), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 56, (screenHeight / 2) + 27), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 57, (screenHeight / 2) + 26), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, quiteCurrent, new Vector2((screenWidth / 2) - quiteCurrent.Length - 58, (screenHeight / 2) + 25), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 22, (screenHeight / 2) + 80), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 23, (screenHeight / 2) + 79), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 24, (screenHeight / 2) + 78), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, controls, new Vector2((screenWidth / 2) - controls.Length - 25, (screenHeight / 2) + 77), Color.Blue);

                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 33, (screenHeight / 2) + 133), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 32, (screenHeight / 2) + 132), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 31, (screenHeight / 2) + 131), Color.Black);
                    spriteBatch.DrawString(pauseMenuFont, exitGame, new Vector2((screenWidth / 2) - exitGame.Length - 30, (screenHeight / 2) + 130), Color.Aqua);
                }

                spriteBatch.End();
                #endregion
            }
            else if (currentState == GameState.notLoaded)
            {
                //TODO
            }
        }    

        #endregion
        
    }
}
