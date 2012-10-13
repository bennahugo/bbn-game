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
        /// Sphere is the sphere generator
        /// texName the name of the texture to use
        /// </summary>
        Graphics.Shapes.Cube cube;

        Texture2D text;
        string texName;

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
        public Skybox(Game game, string texName, float radius, int repeatcount)
            : base(game)
        {
            this.texName = texName;
            text = game.Content.Load<Texture2D>(texName);
            rad = radius;
            repeat = repeatcount;
        }

        /// <summary>
        /// Creates the sphere that is required
        /// </summary>
        public override void Initialize()
        {
            cube = new Shapes.Cube(rad, repeat);

            base.Initialize();
        }

        /// <summary>
        /// Loads the data required
        /// Gets the skyboxEffect shader
        /// Gets the texture that the class was initialised with
        /// </summary>
        public void loadContent()
        {
            e = Game.Content.Load<Effect>("Shader/skyBoxEffect");

            world = e.Parameters["World"];
            view = e.Parameters["View"];
            projection = e.Parameters["Projection"];

            diffuseTex = e.Parameters["diffTex"];

            cube.LoadContent(Game);

            base.LoadContent();
        }

        /// <summary>
        /// Draw method for the entire skybox
        /// </summary>
        /// <param name="gt">The game time</param>
        /// <param name="cam">The camera class</param>
        /// <param name="playerPos">The players position</param>
        public void Draw(GameTime gt, Camera.CameraMatrices cam)
        {
            Matrix worldMatrix = Matrix.Identity;

            e.Begin();
            e.Techniques[0].Passes[0].Begin();

            world.SetValue(worldMatrix);
            view.SetValue(cam.View);
            projection.SetValue(cam.Projection);
            diffuseTex.SetValue(text);

            e.CommitChanges();

            GraphicsDevice.RenderState.CullMode = CullMode.None;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            cube.draw(GraphicsDevice, cam);

            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            e.Techniques[0].Passes[0].End();
            e.End();
        }
    }
}
