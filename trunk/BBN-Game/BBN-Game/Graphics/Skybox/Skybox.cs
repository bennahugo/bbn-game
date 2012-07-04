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
        Graphics.Sphere.Sphere sphere;

        Texture2D text;
        string texName;

        Effect e;
        EffectParameter world;
        EffectParameter view;
        EffectParameter projection;
        EffectParameter diffuseTex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="texName">Name of the texture to use</param>
        public Skybox(Game game, string texName)
            : base(game)
        {
            this.texName = texName;
        }

        /// <summary>
        /// Creates the sphere that is required
        /// </summary>
        public override void Initialize()
        {
            sphere = new Sphere.Sphere(100000, 10, 10);
            sphere.TileUVs(20, 20);

            base.Initialize();
        }

        /// <summary>
        /// Loads the data required
        /// Gets the skyboxEffect shader
        /// Gets the texture that the class was initialised with
        /// </summary>
        public void loadContent()
        {
            text = Game.Content.Load<Texture2D>("Skybox/" + texName);

            e = Game.Content.Load<Effect>("Shader/skyBoxEffect");

            world = e.Parameters["World"];
            view = e.Parameters["View"];
            projection = e.Parameters["Projection"];

            diffuseTex = e.Parameters["diffTex"];

            sphere.LoadContent(Game);

            base.LoadContent();
        }

        /// <summary>
        /// Draw method for the entire skybox
        /// </summary>
        /// <param name="gt">The game time</param>
        /// <param name="cam">The camera class</param>
        /// <param name="playerPos">The players position</param>
        public void Draw(GameTime gt, Camera.CameraMatrices cam, Vector3 playerPos)
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

            sphere.draw(GraphicsDevice, cam);

            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            e.Techniques[0].Passes[0].End();
            e.End();
        }
    }
}
