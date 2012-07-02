using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace BBN_Game.Objects
{
    class DynamicObject : StaticObject
    {
        /// <summary>
        /// Global Variables:
        /// Position: Vector for the position of the Object
        /// Model: The model of the object
        /// </summary>
        protected Vector3 direction;
        protected Vector3 speed;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DynamicObject()
        {
            Position = Vector3.Zero;
            model = null;
            speed = Vector3.Zero;
            direction = Vector3.Zero;
        }

        /// <summary>
        /// Constructor with Model
        /// </summary>
        /// <param name="pos">"The position of the object"</param>
        /// <param name="m">"The model for the object"</param>
        public DynamicObject(Vector3 pos, Model m)
        {
            Position = pos;
            model = m;
            speed = Vector3.Zero;
            direction = Vector3.Zero;
        }

        /// <summary>
        /// Constructor to load model aswell
        /// </summary>
        /// <param name="pos">Position of the object</param>
        /// <param name="m">The content manager to load object</param>
        public DynamicObject(Vector3 pos, ContentManager m):base(pos,m)
        {
            speed = Vector3.Zero;
            direction = Vector3.Zero;
        }

        public override void update(KeyboardState keyboard, GamePadState GP1, GamePadState GP2)
        {
            world = Matrix.CreateTranslation(Position);
        }

        /// <summary>
        /// Draw method
        /// </summary>
        /// <param name="view">The View matrix</param>
        /// <param name="Projection">The projection matrix</param>
        /// <param name="Lighting">The light colours and positions</param>
        /// <param name="fogColour">The fog colour</param>
        /// <param name="fogVariables">The fog starting and ending points</param>
        public override void Draw(Matrix view, Matrix Projection, Vector3 [] Lighting, Vector3 fogColour, int [] fogVariables)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect e in mesh.Effects)
                {
                    e.EnableDefaultLighting();
                    e.PreferPerPixelLighting = true;

                    e.LightingEnabled = true;
                    e.DirectionalLight0.Direction = Lighting[0];
                    e.DirectionalLight0.Direction = Lighting[1];
                    e.DirectionalLight0.Direction = Lighting[2];
                    e.DirectionalLight1.Direction = Lighting[3];
                    e.DirectionalLight1.Direction = Lighting[4];
                    e.DirectionalLight1.Direction = Lighting[5];
                    e.DirectionalLight2.Direction = Lighting[6];
                    e.DirectionalLight2.Direction = Lighting[7];
                    e.DirectionalLight2.Direction = Lighting[8];

                    e.FogEnabled = true;
                    e.FogColor = fogColour;
                    e.FogStart = fogVariables[0];
                    e.FogEnd = fogVariables[1];

                    e.World = world;
                    e.View = view;
                    e.Projection = Projection;
                }
                mesh.Draw();
            }
        }
    }
}
