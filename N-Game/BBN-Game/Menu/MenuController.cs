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
        //Players currentPlayer;

        int screenWidth;
        int screenHeight;

        #region Content variables

        //backgrounds
        private Texture2D mainMenuTex;
        private Texture2D pauseMenuTex;
        private Texture2D optionsMenu;

        //menu fonts
        SpriteFont generalMenuFont;
        SpriteFont selectedMenuFont;

        #endregion

        //manage menu options navigation
        int currentMenuOption = 1;
        
        #endregion

        //constructor
        public MenuController(GameController controller, BBN_Game.BBNGame g)
        {
            gameController = controller;
            this.game = g;
            graphics = g.GraphicsDevice;
            spriteBatch = new SpriteBatch(g.GraphicsDevice);
            currentState = controller.CurrentGameState;            
            Content = g.Content;
            screenHeight = graphics.Viewport.Height;
            screenWidth = graphics.Viewport.Width;
        }

        public void loadContent()
        {
            //load menu backgrounds
            mainMenuTex = Content.Load<Texture2D>("Menu/star_destroyer_bridge");
            pauseMenuTex = Content.Load<Texture2D>("Menu/waterfall_city");

            //load fonts
            generalMenuFont = Content.Load<SpriteFont>("Fonts/menuFont");
            selectedMenuFont = Content.Load<SpriteFont>("Fonts/selectedFont");

        }

        public void unloadContent()
        {

        }

        //manage keyboard input
        KeyboardState prevKeyState = Keyboard.GetState();
        private void handleKeyboardControls()
        {
            KeyboardState keyState = Keyboard.GetState();

            if (currentState == GameState.MainMenu)
            {
                if (keyState.IsKeyDown(Keys.Down) && prevKeyState.IsKeyUp(Keys.Down))//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;

                //selecting menu options
                if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
                {
                    if (currentMenuOption == 1)//start new single player game
                    {
                        currentState = GameState.Playing;
                        gameController.CurrentGameState = GameState.Playing;
                    }
                    else if (currentMenuOption == 2)
                    {
                        //start new multiplayer game TODO
                    }
                    else if (currentMenuOption == 3)//look at and modify game options
                    {
                        currentMenuOption = 1;
                        currentState = GameState.OptionsMenu;
                    }
                    else if (currentMenuOption == 4)//exit game
                        game.Exit();
                }
            }
            else if (currentState == GameState.OptionsMenu)
            {
                if (keyState.IsKeyDown(Keys.Down) && prevKeyState.IsKeyUp(Keys.Down))//move down menu
                    if (currentMenuOption < 4)
                        currentMenuOption++;
                if (keyState.IsKeyDown(Keys.Up) && prevKeyState.IsKeyUp(Keys.Up))//move up menu
                    if (currentMenuOption > 1)
                        currentMenuOption--;

                //selecting menu options
                if (keyState.IsKeyDown(Keys.Enter) && prevKeyState.IsKeyUp(Keys.Enter))
                {
                    if (currentMenuOption == 4)//return to main menu
                    {
                        currentMenuOption = 1;
                        currentState = GameState.MainMenu;
                    }
                }
            }
            else if (currentState == GameState.Paused)
            {

            }
            prevKeyState = keyState;
        }

        //manage menu navigation
        public void updateMenu(GameTime gameTime)
        {
            handleKeyboardControls();
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
                spriteBatch.Draw(mainMenuTex,new Rectangle(0,0,screenWidth,screenHeight),Color.White);
                spriteBatch.DrawString(selectedMenuFont, "Main Menu", new Vector2((screenWidth / 2) - 50, 66), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Main Menu", new Vector2((screenWidth / 2) - 51, 65), Color.Red);

                //display menu options
                if (currentMenuOption == 1)
                {
                    spriteBatch.DrawString(selectedMenuFont, "Single Player", new Vector2(screenWidth - 239, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Single Player", new Vector2(screenWidth - 240, (screenHeight / 2) - 50), Color.Orange);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Options", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Options", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Red);
                }
                else if (currentMenuOption == 2)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Red);

                    spriteBatch.DrawString(selectedMenuFont, "MultiPlayer", new Vector2(screenWidth - 239, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "MultiPlayer", new Vector2(screenWidth - 240, (screenHeight / 2)), Color.Orange);

                    spriteBatch.DrawString(generalMenuFont, "Options", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Options", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Red);
                }
                else if (currentMenuOption == 3)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Red);

                    spriteBatch.DrawString(selectedMenuFont, "Options", new Vector2(screenWidth - 239, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Options", new Vector2(screenWidth - 240, (screenHeight / 2) + 50), Color.Orange);

                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Exit Game", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Red);
                }
                else if (currentMenuOption == 4)
                {
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Single Player", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "MultiPlayer", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Options", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Options", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Red);

                    spriteBatch.DrawString(selectedMenuFont, "Exit Game", new Vector2(screenWidth - 239, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Exit Game", new Vector2(screenWidth - 240, (screenHeight / 2) + 100), Color.Orange);
                }

                spriteBatch.End();

                #endregion 
            }
            else if (currentState == GameState.OptionsMenu)
            {
                #region Options Menu
                graphics.Clear(Color.Black);
                spriteBatch.Begin();

                //draw main menu background and menu outline for options
                spriteBatch.Draw(pauseMenuTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);
                spriteBatch.DrawString(selectedMenuFont, "Options Menu", new Vector2((screenWidth / 2) - 100, 66), Color.Black);
                spriteBatch.DrawString(selectedMenuFont, "Options Menu", new Vector2((screenWidth / 2) - 101, 65), Color.Red);

                //display menu options
                if (currentMenuOption == 1)
                {
                    spriteBatch.DrawString(selectedMenuFont, "Random1", new Vector2(screenWidth - 239, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Random1", new Vector2(screenWidth - 240, (screenHeight / 2) - 50), Color.Orange);

                    spriteBatch.DrawString(generalMenuFont, "Random2", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random2", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Random3", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random3", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Main Menu", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Main Menu", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Red);
                }
                else if (currentMenuOption == 2)
                {
                    spriteBatch.DrawString(generalMenuFont, "Random1", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random1", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Red);

                    spriteBatch.DrawString(selectedMenuFont, "Random2", new Vector2(screenWidth - 239, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Random2", new Vector2(screenWidth - 240, (screenHeight / 2)), Color.Orange);

                    spriteBatch.DrawString(generalMenuFont, "Random3", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random3", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Main Menu", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Main Menu", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Red);
                }
                else if (currentMenuOption == 3)
                {
                    spriteBatch.DrawString(generalMenuFont, "Random1", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random1", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Random2", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random2", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Red);

                    spriteBatch.DrawString(selectedMenuFont, "Random3", new Vector2(screenWidth - 239, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Random3", new Vector2(screenWidth - 240, (screenHeight / 2) + 50), Color.Orange);

                    spriteBatch.DrawString(generalMenuFont, "Main Menu", new Vector2(screenWidth - 199, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Main Menu", new Vector2(screenWidth - 200, (screenHeight / 2) + 100), Color.Red);
                }
                else if (currentMenuOption == 4)
                {
                    spriteBatch.DrawString(generalMenuFont, "Random1", new Vector2(screenWidth - 199, (screenHeight / 2) - 49), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random1", new Vector2(screenWidth - 200, (screenHeight / 2) - 50), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Random2", new Vector2(screenWidth - 199, (screenHeight / 2) + 1), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random2", new Vector2(screenWidth - 200, (screenHeight / 2)), Color.Red);

                    spriteBatch.DrawString(generalMenuFont, "Random3", new Vector2(screenWidth - 199, (screenHeight / 2) + 51), Color.Black);
                    spriteBatch.DrawString(generalMenuFont, "Random3", new Vector2(screenWidth - 200, (screenHeight / 2) + 50), Color.Red);

                    spriteBatch.DrawString(selectedMenuFont, "Main Menu", new Vector2(screenWidth - 239, (screenHeight / 2) + 101), Color.Black);
                    spriteBatch.DrawString(selectedMenuFont, "Main Menu", new Vector2(screenWidth - 240, (screenHeight / 2) + 100), Color.Orange);
                }

                spriteBatch.End();
                #endregion
            }
            else if (currentState == GameState.Paused)
            {
                #region Paused Menu
                graphics.Clear(Color.Black);
                spriteBatch.Begin();

                //draw main menu background and menu outline for options
                spriteBatch.Draw(mainMenuTex, new Rectangle(0, 0, screenWidth, screenHeight), Color.White);

                //display menu options
                spriteBatch.DrawString(generalMenuFont, "Main Menu", new Vector2((screenWidth / 2) - 50, (screenHeight / 2) - 119), Color.Black);
                spriteBatch.DrawString(generalMenuFont, "Main Menu", new Vector2((screenWidth / 2) - 51, (screenHeight / 2) - 120), Color.Red);

                spriteBatch.End();
                #endregion
            }
            else if (currentState == GameState.notLoaded)
            {

            }
        }
    }
}
