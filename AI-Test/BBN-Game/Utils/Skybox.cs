using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

/////
/// Author - Brandon James Talbot
///
/// Draws the skybox for the game by using a Quad Drawer
/////
namespace BBN_Game.Graphics.Skybox
{
    class Skybox
    {
        /// <summary>
        /// Global Variables
        ///
        /// Sphere is the sphere generator
        /// texName the name of the texture to use
        /// </summary>
        Graphics.Shapes.Cube sphere;

        Texture2D text;

        Effect e;
        EffectParameter world;
        EffectParameter view;
        EffectParameter projection;
        EffectParameter diffuseTex;
        float rad;
        int repeat;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="texName">Name of the texture to use</param>
        public Skybox()
        {
        }

        /// <summary>
        /// Creates the sphere that is required
        /// </summary>
        public void Initialize(float radius, int repeatcount)
        {
            rad = radius;
            repeat = repeatcount;
            sphere = new Graphics.Shapes.Cube(radius, repeat);
        }

        /// <summary>
        /// Loads the data required
        /// Gets the skyboxEffect shader
        /// Gets the texture that the class was initialised with
        /// </summary>
        public void loadContent(Texture2D tex, ContentManager content, GraphicsDevice gd)
        {
            text = tex;

            e = content.Load<Effect>("Shader/skyBoxEffect");

            world = e.Parameters["World"];
            view = e.Parameters["View"];
            projection = e.Parameters["Projection"];

            diffuseTex = e.Parameters["diffTex"];

            sphere.LoadContent(gd);
        }

        /// <summary>
        /// Draw method for the entire skybox
        /// </summary>
        /// <param name="gt">The game time</param>
        /// <param name="cam">The camera class</param>
        /// <param name="playerPos">The players position</param>
        public void Draw(Matrix v, Matrix Projection, GraphicsDevice gd)
        {
            Matrix worldMatrix = Matrix.Identity;

            e.Begin();
            e.Techniques[0].Passes[0].Begin();

            world.SetValue(worldMatrix);
            view.SetValue(v);
            projection.SetValue(Projection);
            diffuseTex.SetValue(text);

            e.CommitChanges();

            gd.RenderState.CullMode = CullMode.None;
            gd.RenderState.DepthBufferWriteEnable = false;

            sphere.draw(gd);

            gd.RenderState.DepthBufferWriteEnable = true;
            gd.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            e.Techniques[0].Passes[0].End();
            e.End();
        }
    }
}
