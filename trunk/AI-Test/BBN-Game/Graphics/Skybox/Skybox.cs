using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

/////
/// Author - Brandon James Talbot
///
/// Draws the skybox for the game by using a Quad Drawer
/////
namespace BBN_Game.Graphics.Skybox
{
    class Skybox : DrawableGameComponent
    {
        /// <summary>
        /// Global Variables
        /// 
        /// Textures [] - array of textures to use
        /// Quads - the Quads to draw
        /// </summary>
        private Texture2D[] textures;
        private List<QuadDrawer> Quads;

        public Skybox(Game game, int gameRadius, int [] repeat, string[] texNames)
            : base(game)
        {
            Quads = new List<QuadDrawer>();
            textures = new Texture2D[6];
            for (int i = 0; i < 6; ++i)
                textures[i] = Game.Content.Load<Texture2D>(texNames[i]);

            ///// TOP ----------------------------------------------------------------------------------------
            Quads.Add(new QuadDrawer(new Vector3(gameRadius + 0.5f, gameRadius, -gameRadius),
                    new Vector3(-gameRadius - 0.5f, gameRadius, -gameRadius),
                    new Vector3(gameRadius + 0.5f, gameRadius, gameRadius),
                    new Vector3(-gameRadius - 0.5f, gameRadius, gameRadius), repeat[0], textures[0], Game.GraphicsDevice));

            ///// Bottom ----------------------------------------------------------------------------------------
            Quads.Add(new QuadDrawer(new Vector3(-gameRadius - 0.5f, -gameRadius, -gameRadius),
                    new Vector3(gameRadius + 0.5f, -gameRadius, -gameRadius),
                    new Vector3(-gameRadius - 0.5f, -gameRadius, gameRadius),
                    new Vector3(gameRadius + 0.5f, -gameRadius, gameRadius), repeat[1], textures[1], Game.GraphicsDevice));

            ///// Right ----------------------------------------------------------------------------------------
            Quads.Add(new QuadDrawer(new Vector3(gameRadius, gameRadius, -gameRadius - 0.5f),
                    new Vector3(gameRadius, gameRadius, +gameRadius + 0.5f),
                    new Vector3(gameRadius, -gameRadius, -gameRadius - 0.5f),
                    new Vector3(gameRadius, -gameRadius, +gameRadius + 0.5f), repeat[2], textures[2], Game.GraphicsDevice));

            ///// LEFT ----------------------------------------------------------------------------------------
            Quads.Add(new QuadDrawer(new Vector3(-gameRadius, gameRadius, +gameRadius + 0.5f),
                    new Vector3(-gameRadius, gameRadius, -gameRadius - 0.5f),
                    new Vector3(-gameRadius, -gameRadius, +gameRadius + 0.5f),
                    new Vector3(-gameRadius, -gameRadius, -gameRadius - 0.5f), repeat[3], textures[3], Game.GraphicsDevice));

            ///// FRONT ----------------------------------------------------------------------------------------
            Quads.Add(new QuadDrawer(new Vector3(gameRadius + 0.5f, gameRadius, gameRadius),
                    new Vector3(-gameRadius - 0.5f, gameRadius, gameRadius),
                    new Vector3(gameRadius + 0.5f, -gameRadius, gameRadius),
                    new Vector3(-gameRadius - 0.5f, -gameRadius, gameRadius), repeat[4], textures[4], Game.GraphicsDevice));

            ///// BACK ----------------------------------------------------------------------------------------
            Quads.Add(new QuadDrawer(new Vector3(-gameRadius - 0.5f, gameRadius, -gameRadius),
                    new Vector3(gameRadius + 0.5f, gameRadius, -gameRadius),
                    new Vector3(-gameRadius - 0.5f, -gameRadius, -gameRadius),
                    new Vector3(gameRadius + 0.5f, -gameRadius, -gameRadius), repeat[5], textures[5], Game.GraphicsDevice));

            
        }

        /// <summary>
        /// Draw method for the entire skybox
        /// </summary>
        /// <param name="gt">The game time</param>
        /// <param name="cam">The camera class</param>
        /// <param name="playerPos">The players position</param>
        public void Draw(GameTime gt, Camera.CameraMatrices cam, Vector3 playerPos)
        {
            Matrix world = Matrix.CreateTranslation(playerPos);
            BasicEffect effect = new BasicEffect(Game.GraphicsDevice, new EffectPool());
            effect.EmissiveColor = new Vector3(1,1,1);
            foreach (QuadDrawer q in Quads)
                q.Draw(cam.View, world, cam.Projection, effect); 
            
            base.Draw(gt);
        }
    }
}
