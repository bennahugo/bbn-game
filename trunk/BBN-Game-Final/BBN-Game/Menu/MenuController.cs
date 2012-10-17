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
        BBN_Game.BBNGame game;
        GameController gameController;

        //game states
        GameState currentState;
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
            currentState = controller.CurrentGameState;
            gameController.PreviousState = currentState;
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
            controlsMenuTex = Content.Load<Texture2D>("Menu/control_menu");
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
                    else if (currentMenuOption == 4)
                        currentMenuOption = 1;
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                    else if (currentMenuOption == 1)
                        currentMenuOption = 4;

                //selecting menu options
                if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
                {
                    gameController.PreviousState = currentState;
                    
                    if (currentMenuOption == 1)//start new single player game
                    {
                        gameController.PreviousState = GameState.notLoaded;
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }                    
                    else if (currentMenuOption == 2)
                    {
                        //start new multiplayer game
                        Controller.GameController.NumberOfPlayers = Players.two;
                        gameController.PreviousState = GameState.notLoaded;
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
                    else if (currentMenuOption == 3)
                        currentMenuOption = 1;
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                    else if (currentMenuOption == 1)
                        currentMenuOption = 3;

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
                        currentState = gameController.PreviousState;
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
                    else if (currentMenuOption == 5)
                        currentMenuOption = 1;
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;
                    else if (currentMenuOption == 1)
                        currentMenuOption = 5;

                //selecting menu options
                if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
                {
                    gameController.PreviousState = currentState;
                    if (currentMenuOption == 1)//resume gameplay
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)//restart current game TODO
                    {
                        gameController.PreviousState = GameState.notLoaded;
                        currentState = GameState.reload;
                        gameController.CurrentGameState = GameState.reload;
                    }
                    else if (currentMenuOption == 3)//quite current game & return to main menu
                    {
                        // TODO - quite current game

                        currentMenuOption = 1;
                        currentState = GameState.MainMenu;
                        Controller.GameController.ObjectsLoaded = false;
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
            else if (currentState == GameState.EndGame)
            {
                if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
                {
                    currentState = GameState.MainMenu;
                    Controller.GameController.ObjectsLoaded = false;
                }
            }
            prevKeyState = keyState;
        }

        //manage xbox controls
        GamePadState prevPadState1 = GamePad.GetState(PlayerIndex.One);
        GamePadState prevPadState2 = GamePad.GetState(PlayerIndex.Two);
        int menuDelay = 8;
        private void handleXboxControls()
        {
            GamePadState padState1 = GamePad.GetState(PlayerIndex.One);
            GamePadState padState2 = GamePad.GetState(PlayerIndex.Two);

            if (currentState == GameState.MainMenu)
            {
                #region Main Menu

                #region Player 1
                if (menuDelay <= 0)
                {
                    if (padState1.ThumbSticks.Left.Y <= -0.5)//move down menu
                        if (currentMenuOption < 4)
                            currentMenuOption++;
                        else if (currentMenuOption == 4)
                            currentMenuOption = 1;
                    if (padState1.ThumbSticks.Left.Y >= 0.5)//move up menu
                        if (currentMenuOption > 1)
                            currentMenuOption--;
                        else if (currentMenuOption == 1)
                            currentMenuOption = 4;
                    menuDelay = 8;
                }

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
                if (menuDelay <= 0)
                {
                    if (padState2.ThumbSticks.Left.Y <= -0.5)//move down menu
                        if (currentMenuOption < 4)
                            currentMenuOption++;
                        else if (currentMenuOption == 4)
                            currentMenuOption = 1;
                    if (padState2.ThumbSticks.Left.Y >= 0.5)//move up menu
                        if (currentMenuOption > 1)
                            currentMenuOption--;
                        else if (currentMenuOption == 1)
                            currentMenuOption = 4;
                    menuDelay = 8;
                }

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
            else if (currentState == GameState.OptionsMenu)
            {
                #region Control Menu

                #region player 1
                if (menuDelay <= 0)
                {
                    if (padState1.ThumbSticks.Left.Y <= -0.5)//move down menu
                        if (currentMenuOption < 3)
                            currentMenuOption++;
                        else if (currentMenuOption == 3)
                            currentMenuOption = 1;
                    if (padState1.ThumbSticks.Left.Y >= 0.5)//move up menu
                        if (currentMenuOption > 1)
                            currentMenuOption--;
                        else if (currentMenuOption == 1)
                            currentMenuOption = 3;
                    menuDelay = 8;
                }

                if (padState1.Buttons.A == ButtonState.Pressed && prevPadState1.Buttons.A == ButtonState.Released)
                {
                    if (currentMenuOption == 1)//display keyboard controls
                        displayXboxControls = false;
                    else if (currentMenuOption == 2)//display xbox controls
                        displayXboxControls = true;
                    else if (currentMenuOption == 3)
                    {
                        currentState = gameController.PreviousState;
                        currentMenuOption = 1;
                    }
                }
                #endregion

                #region player 2
                if (menuDelay <= 0)
                {
                    if (padState2.ThumbSticks.Left.Y <= -0.5)//move down menu
                        if (currentMenuOption < 3)
                            currentMenuOption++;
                        else if (currentMenuOption == 3)
                            currentMenuOption = 1;
                    if (padState2.ThumbSticks.Left.Y >= 0.5)//move up menu
                        if (currentMenuOption > 1)
                            currentMenuOption--;
                        else if (currentMenuOption == 1)
                            currentMenuOption = 3;
                    menuDelay = 8;
                }

                if (padState2.Buttons.A == ButtonState.Pressed && prevPadState2.Buttons.A == ButtonState.Released)
                {
                    if (currentMenuOption == 1)//display keyboard controls
                        displayXboxControls = false;
                    else if (currentMenuOption == 2)//display xbox controls
                        displayXboxControls = true;
                    else if (currentMenuOption == 3)
                    {
                        currentState = gameController.PreviousState;
                        currentMenuOption = 1;
                    }
                }
                #endregion
                                
                #endregion
            }
            else if (currentState == GameState.Paused)
            {
                #region Pause Menu

                #region Player 1
                if (menuDelay <= 0)
                {
                    if (padState1.ThumbSticks.Left.Y <= -0.5)//move down menu
                        if (currentMenuOption < 5)
                            currentMenuOption++;
                        else if (currentMenuOption == 5)
                            currentMenuOption = 1;

                    if (padState1.ThumbSticks.Left.Y >= 0.5)//move up menu
                        if (currentMenuOption > 1)
                            currentMenuOption--;
                        else if (currentMenuOption == 1)
                            currentMenuOption = 5;
                    menuDelay = 8;
                }

                if (padState1.Buttons.A == ButtonState.Pressed && prevPadState1.Buttons.A == ButtonState.Released)
                {
                    gameController.PreviousState = currentState;
                    if (currentMenuOption == 1)//resume gameplay
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)
                    {
                        //TODO Restart current game
                    }
                    else if (currentMenuOption == 3)//quit current game & return to main menu
                    {
                        // TODO - quit current game
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

                #region Player 2
                if (menuDelay <= 0)
                {
                    if (padState2.ThumbSticks.Left.Y <= -0.5)//move down menu
                        if (currentMenuOption < 5)
                            currentMenuOption++;
                        else if (currentMenuOption == 5)
                            currentMenuOption = 1;
                    if (padState2.ThumbSticks.Left.Y >= 0.5)//move up menu
                        if (currentMenuOption > 1)
                            currentMenuOption--;
                        else if (currentMenuOption == 1)
                            currentMenuOption = 5;
                    menuDelay = 8;
                }

                if (padState2.Buttons.A == ButtonState.Pressed && prevPadState2.Buttons.A == ButtonState.Released)
                {
                    gameController.PreviousState = currentState;
                    if (currentMenuOption == 1)//resume gameplay
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)
                    {
                        //TODO restart game
                    }
                    else if (currentMenuOption == 3)//quit current game & return to main menu
                    {
                        // TODO - quit current game

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

                #endregion
            }
            else if (currentState == GameState.notLoaded)
            {

            }
            else if (currentState == GameState.EndGame)
            {
                if (padState1.Buttons.A == ButtonState.Pressed && prevPadState1.Buttons.A == ButtonState.Released)
                {
                    currentState = GameState.MainMenu;
                    Controller.GameController.ObjectsLoaded = false;
                }
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
        
        //draw end-game screen 
        public void drawEndGame(SpriteBatch spriteBatch,Objects.playerObject player)
        {
            graphics.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(mainMenuTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
            spriteBatch.DrawString(selectedMenuFont, "Game Over", new Vector2((screenWidth / 2) - 50, 20), Color.Red);
            spriteBatch.DrawString(selectedMenuFont, ""+player.Team.ToString(), new Vector2((screenWidth / 2) - 50, 70), Color.Red);
            spriteBatch.DrawString(selectedMenuFont, "Main Menu", new Vector2((screenWidth / 2) - 50, screenHeight - 100), Color.Red);
            spriteBatch.End();
        }

        public void drawTradeMenu(SpriteBatch spriteBatch, Objects.playerObject player)
        {
            //for slide-up animation
            if (player.UpFactor > 0 && player.GoingUp)
                player.UpFactor -= 10;
            else if ((!player.GoingUp) && player.UpFactor < 150)
                player.UpFactor += 10;

            spriteBatch.Begin();

            //draw trade menu pop-up panel and menu outline for options
            spriteBatch.Draw(tradeMenuTex, new Rectangle(player.getViewport.Width - 160, player.getViewport.Height - 300 + player.UpFactor, 150, 276), Color.White);
            int iCurrency = (player.Team == Objects.Team.Red ? Controller.GameController.team1.teamCredits : Controller.GameController.team2.teamCredits);
            int iNumDestroyer = (player.Team == Objects.Team.Red ? Controller.GameController.team1.teamDestroyers.Count : Controller.GameController.team2.teamDestroyers.Count);
            int iNumFighter = (player.Team == Objects.Team.Red ? Controller.GameController.team1.teamFighters.Count : Controller.GameController.team2.teamFighters.Count);
            String currency = "Available: $" + iCurrency;
            float currencyWidth = tradeMenuFont.MeasureString(currency).X;
            spriteBatch.DrawString(tradeMenuFont, currency, new Vector2(player.getViewport.Width - 84 - currencyWidth / 2, player.getViewport.Height - 243 + player.UpFactor), Color.Black);
            spriteBatch.DrawString(tradeMenuFont, currency, new Vector2(player.getViewport.Width - 85 - currencyWidth / 2, player.getViewport.Height - 244 + player.UpFactor), Color.Black);
            spriteBatch.DrawString(tradeMenuFont, currency, new Vector2(player.getViewport.Width - 86 - currencyWidth / 2, player.getViewport.Height - 245 + player.UpFactor), Color.Yellow);

            #region draw menu options
            if (player.TradeMenuOption == 1)
            {
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost, 
                    new Vector2(player.getViewport.Width - 132, player.getViewport.Height - 220 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost, 
                    new Vector2(player.getViewport.Width - 133, player.getViewport.Height - 221 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost,
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 222 + player.UpFactor), (iCurrency >= TradingInformation.destroyerCost && iNumDestroyer <= GameController.MAX_NUM_DESTROYERS_PER_TEAM) ? Color.Aqua : Color.Red);

                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost, 
                    new Vector2(player.getViewport.Width - 136, player.getViewport.Height - 165 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost, 
                    new Vector2(player.getViewport.Width - 135, player.getViewport.Height - 166 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost,
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 167 + player.UpFactor), (iCurrency >= TradingInformation.fighterCost && iNumFighter <= GameController.MAX_NUM_FIGHTERS_PER_TEAM) ? Color.Green : Color.DarkRed);

                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost, 
                    new Vector2(player.getViewport.Width - 136, player.getViewport.Height - 109 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost, 
                    new Vector2(player.getViewport.Width - 135, player.getViewport.Height - 110 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost, 
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 111 + player.UpFactor), iCurrency >= TradingInformation.missileCost ? Color.Green : Color.DarkRed);
            }
            else if (player.TradeMenuOption == 2)
            {
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost,
                    new Vector2(player.getViewport.Width - 136, player.getViewport.Height - 220 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost,
                    new Vector2(player.getViewport.Width - 135, player.getViewport.Height - 221 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost,
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 222 + player.UpFactor), (iCurrency >= TradingInformation.destroyerCost && iNumDestroyer <= GameController.MAX_NUM_DESTROYERS_PER_TEAM) ? Color.Green : Color.DarkRed);

                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost,
                    new Vector2(player.getViewport.Width - 132, player.getViewport.Height - 165 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost,
                    new Vector2(player.getViewport.Width - 133, player.getViewport.Height - 166 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost,
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 167 + player.UpFactor), (iCurrency >= TradingInformation.fighterCost && iNumFighter <= GameController.MAX_NUM_FIGHTERS_PER_TEAM) ? Color.Aqua : Color.Red);

                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost,
                    new Vector2(player.getViewport.Width - 136, player.getViewport.Height - 109 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost,
                    new Vector2(player.getViewport.Width - 135, player.getViewport.Height - 110 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost,
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 111 + player.UpFactor), iCurrency >= TradingInformation.missileCost ? Color.Green : Color.DarkRed);
            }
            else if (player.TradeMenuOption == 3)
            {
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost,
                    new Vector2(player.getViewport.Width - 136, player.getViewport.Height - 220 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost,
                    new Vector2(player.getViewport.Width - 135, player.getViewport.Height - 221 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Destroyer $" + TradingInformation.destroyerCost,
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 222 + player.UpFactor), (iCurrency >= TradingInformation.destroyerCost && iNumDestroyer <= GameController.MAX_NUM_DESTROYERS_PER_TEAM) ? Color.Green : Color.DarkRed);

                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost,
                    new Vector2(player.getViewport.Width - 136, player.getViewport.Height - 165 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost,
                    new Vector2(player.getViewport.Width - 135, player.getViewport.Height - 166 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Fighter $" + TradingInformation.fighterCost,
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 167 + player.UpFactor), (iCurrency >= TradingInformation.fighterCost && iNumFighter <= GameController.MAX_NUM_FIGHTERS_PER_TEAM) ? Color.Green : Color.DarkRed);

                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost,
                    new Vector2(player.getViewport.Width - 132, player.getViewport.Height - 109 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost,
                    new Vector2(player.getViewport.Width - 133, player.getViewport.Height - 110 + player.UpFactor), Color.Black);
                spriteBatch.DrawString(tradeMenuFont, "Missile $" + TradingInformation.missileCost,
                    new Vector2(player.getViewport.Width - 134, player.getViewport.Height - 111 + player.UpFactor), iCurrency >= TradingInformation.missileCost ? Color.Aqua : Color.Red);
            }
            #endregion

            #region Draw menu values

            spriteBatch.DrawString(tradeMenuFont, "You have: " + (player.Team == Objects.Team.Red ? Controller.GameController.team1.teamDestroyers.Count : Controller.GameController.team2.teamDestroyers.Count).ToString(), 
                new Vector2(player.getViewport.Width - 105, player.getViewport.Height - 195 + player.UpFactor), Color.Red);
            spriteBatch.DrawString(tradeMenuFont, "You have: " + (player.Team == Objects.Team.Red ? Controller.GameController.team1.teamFighters.Count : Controller.GameController.team2.teamFighters.Count).ToString(), 
                new Vector2(player.getViewport.Width - 105, player.getViewport.Height - 141 + player.UpFactor), Color.Red);
            spriteBatch.DrawString(tradeMenuFont, "You have: " + player.Missiles, new Vector2(player.getViewport.Width - 105, player.getViewport.Height - 85 + player.UpFactor), Color.Red);

            #endregion

            spriteBatch.End();
        }

        public void drawTradeStats(SpriteBatch spriteBatch, Objects.playerObject player)
        {
            spriteBatch.Begin();

            //draw icons for ship-counts and missile-counts
            spriteBatch.Draw(destroyerTex, new Rectangle(player.getViewport.Width - 280, player.getViewport.Height - 50, 64, 36), Color.White);
            spriteBatch.DrawString(tradeMenuFont,
                (player.Team == Objects.Team.Red ? Controller.GameController.team1.teamDestroyers.Count : Controller.GameController.team2.teamDestroyers.Count).ToString(),
                new Vector2(player.getViewport.Width - 290, player.getViewport.Height - 50), Color.Red);
            spriteBatch.Draw(fighterTex, new Rectangle(player.getViewport.Width - 180, player.getViewport.Height - 50, 64, 41), Color.White);
            spriteBatch.DrawString(tradeMenuFont,
                (player.Team == Objects.Team.Red ? Controller.GameController.team1.teamFighters.Count : Controller.GameController.team2.teamFighters.Count).ToString(),
                new Vector2(player.getViewport.Width - 190, player.getViewport.Height - 50), Color.Red);
            spriteBatch.Draw(missileTex, new Rectangle(player.getViewport.Width - 70, player.getViewport.Height - 50, 64, 48), Color.White);
            spriteBatch.DrawString(tradeMenuFont, "" + player.Missiles, new Vector2(player.getViewport.Width - 80, player.getViewport.Height - 50), Color.Red);

            spriteBatch.End();
        }

        //menu animation
        int animationDelay = 1000;
        private void menuOptionAnimation(SpriteBatch spriteBatch,GameTime gameTime,String text)
        {
            //expand text
            int nextPhase = 0;
            while(nextPhase < 5)
            {
                //spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth - 361, 321), Color.Black);
                //spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth - 360, 320), Color.Black);
                //spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth - 359, 319), Color.Black);
                //spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth - 358, 318), Color.Aqua);
                animationDelay--;

                if (animationDelay < 0)
                {
                    nextPhase++;
                    animationDelay = 10;
                    Vector2 FontOrigin = generalMenuFont.MeasureString(text) / 2;
                    spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth / 2, screenHeight / 2), Color.LightGreen,
            0, FontOrigin, 1.0f * nextPhase, SpriteEffects.None, 0.5f);
                }
            }

            //shrink text
            while (nextPhase > 0)
            {
                //spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth - 361, 321), Color.Black);
                //spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth - 360, 320), Color.Black);
                //spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth - 359, 319), Color.Black);
                //spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth - 358, 318), Color.Aqua);
                animationDelay--;

                if (animationDelay < 0)
                {
                    nextPhase--;
                    animationDelay = 10;
                    Vector2 FontOrigin = generalMenuFont.MeasureString(text) / 2;
                    spriteBatch.DrawString(generalMenuFont, text, new Vector2(screenWidth / 2, screenHeight / 2), Color.LightGreen,
            0, FontOrigin, 1.0f * nextPhase, SpriteEffects.None, 0.5f);
                }
            }
        }

        //update what menu is currently being displayed
        public void drawMenu(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (currentState == GameState.MainMenu)
            {
                #region Main Menu
                graphics.Clear(Color.Black);
                spriteBatch.Begin();

                //draw main menu background and menu outline for options
                spriteBatch.Draw(mainMenuTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 107, 69), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 108, 68), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 109, 67), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 110, 66), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Nebulon 12", new Vector2((screenWidth / 2) - 111, 65), Color.Blue);
                
                //draw menu outline
                spriteBatch.Draw(pauseMenuTex, new Rectangle(screenWidth - 600, 100, 600, 600), Color.White);

                #region display menu options                

                if (currentMenuOption == 1)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 361, 321), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 360, 320), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 359, 319), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 358, 318), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 349, 369), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 350, 368), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 351, 367), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 352, 366), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 332, 421), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 333, 420), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 334, 419), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 335, 418), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 341, 476), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 342, 475), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 343, 474), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 344, 473), Color.Blue);
                }
                else if (currentMenuOption == 2)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 355, 321), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 356, 320), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 357, 319), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 358, 318), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 355, 369), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 354, 368), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 353, 367), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 352, 366), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 332, 421), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 333, 420), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 334, 419), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 335, 418), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 341, 476), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 342, 475), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 343, 474), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 344, 473), Color.Blue);
                }
                else if (currentMenuOption == 3)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 355, 321), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 356, 320), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 357, 319), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 358, 318), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 349, 369), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 350, 368), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 351, 367), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 352, 366), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 338, 421), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 337, 420), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 336, 419), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 335, 418), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 341, 476), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 342, 475), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 343, 474), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 344, 473), Color.Blue);
                }
                else if (currentMenuOption == 4)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 355, 321), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 356, 320), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 357, 319), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 358, 318), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 349, 369), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 350, 368), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 351, 367), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 352, 366), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 332, 421), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 333, 420), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 334, 419), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Controls", new Vector2(screenWidth - 335, 418), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 347, 476), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 346, 475), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 345, 474), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 344, 473), Color.Aqua);
                }
                #endregion

                spriteBatch.End();

                #endregion
            }
            else if (currentState == GameState.OptionsMenu)
            {
                #region Controls Menu
                graphics.Clear(Color.Gray);
                spriteBatch.Begin();

                //draw main menu background and menu outline for options
                if (!displayXboxControls)
                {
                    graphics.Clear(Color.Gray);
                    //spriteBatch.Draw(mainMenuTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                    spriteBatch.Draw(keybrdControlsTex, new Rectangle(350, 0, screenWidth - 350, screenHeight), Color.White);
                }
                else
                {
                    graphics.Clear(Color.Black);
                    spriteBatch.Draw(xboxControlsTex, new Rectangle(350, 50, screenWidth - 350, screenHeight - 50), Color.White);
                }
                
                spriteBatch.Draw(controlsMenuTex, new Rectangle(10, 100, 275, 510), Color.White);

                spriteBatch.DrawString(selectedMenuFont, "Game Controls", new Vector2(7, 22), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Game Controls", new Vector2(8, 23), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Game Controls", new Vector2(9, 24), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Game Controls", new Vector2(10, 25), Color.Blue);

                #region Draw menu options

                if (currentMenuOption == 1)
                {
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(89, 248), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(90, 247), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(91, 246), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(92, 245), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(123, 351), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(122, 350), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(121, 349), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(120, 348), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(125, 453), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(124, 452), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(123, 451), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(122, 450), Color.Blue);
                }
                else if (currentMenuOption == 2)
                {
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(95, 248), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(94, 247), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(93, 246), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(92, 245), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(117, 351), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(118, 350), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(119, 349), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(120, 348), Color.Aqua);

                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(125, 453), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(124, 452), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(123, 451), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(122, 450), Color.Blue);
                }
                else if (currentMenuOption == 3)
                {
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(95, 248), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(94, 247), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(93, 246), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Keyboard", new Vector2(92, 245), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(123, 351), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(122, 350), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(121, 349), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Xbox", new Vector2(120, 348), Color.Blue);

                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(119, 453), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(120, 452), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(121, 451), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Back", new Vector2(122, 450), Color.Aqua);
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
                spriteBatch.DrawString(generalMenuFont, pauseTitle, new Vector2((screenWidth / 2) - pauseTitle.Length - 62, (screenHeight / 2) - 122), Color.Black);
                spriteBatch.DrawString(generalMenuFont, pauseTitle, new Vector2((screenWidth / 2) - pauseTitle.Length - 63, (screenHeight / 2) - 123), Color.Black);
                spriteBatch.DrawString(generalMenuFont, pauseTitle, new Vector2((screenWidth / 2) - pauseTitle.Length - 64, (screenHeight / 2) - 124), Color.Black);
                spriteBatch.DrawString(generalMenuFont, pauseTitle, new Vector2((screenWidth / 2) - pauseTitle.Length - 65, (screenHeight / 2) - 125), Color.Blue);

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
