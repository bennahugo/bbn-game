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
using BBN_Game.Utils;
using BBN_Game.Map;
using BBN_Game.Collision_Detection;
namespace Editor
{
    class Drawer : MapContent
    {
        /// <summary>
        /// Global Variables:
        /// Position: Vector for the position of the Object
        /// Model: The model of the object
        /// </summary>
        public Vector3 Position { get; set; }
        public float pitch { get; set; }
        public float yaw { get; set; }
        public float roll { get; set; }
        public Vector3 Scale { get; set; }
        public Model model { get; protected set; }
        public Matrix world { get; protected set; }
        public String modelName { get; protected set; }
        public ContentManager contentLoader;
        /// <summary>
        /// Default constructor
        /// </summary>
        public Drawer(Boolean isTemporary):
            base(isTemporary)
        {
            Position = Vector3.Zero;
            world = Matrix.Identity * Matrix.CreateTranslation(Position);
            model = null;
            modelName = null;
            pitch = yaw = roll = 0.0f;
            Scale = Vector3.One;
        }

        /// <summary>
        /// Constructor with Model
        /// </summary>
        /// <param name="pos">"The position of the object"</param>
        /// <param name="m">"The model for the object"</param>
        public Drawer(Vector3 pos, Model m,Boolean isTemporary):
            base(isTemporary)
        {
            Position = pos;
            world = Matrix.Identity * Matrix.CreateTranslation(Position);
            model = m;
            Scale = Vector3.One;
        }

        /// <summary>
        /// Constructor with loading variable
        /// </summary>
        /// <param name="pos">"The position of the object"</param>
        /// <param name="m">The content loader</param>
        public Drawer(Vector3 pos, ContentManager m, Boolean isTemporary) :
            base(isTemporary)
        {
            Position = pos;
            world = Matrix.Identity * Matrix.CreateTranslation(Position);
            contentLoader = m;
            loadModel(); // loads the object from its own method
            Scale = Vector3.One;
        }

        /// <summary>
        /// Abstract class for the object to be able to load in its own model.
        /// </summary>
        /// <param name="m">Content loader</param>
        public virtual void loadModel()
        {
            if (modelName != null)
            {
                model = contentLoader.Load<Model>(modelName);
                CollisionDetectionHelper.setModelData(model);
                CollisionDetectionHelper.ConstructMeshPartBoundingSpherees(model);
                CollisionDetectionHelper.ConstructObjectLevelBoundingSphere(model);
                CollisionDetectionHelper.ConstructMeshLevelBoundingSphere(model);
            }
        }

        public virtual void unload()
        {
            model = null;

            contentLoader.Unload();
        }

        public void update(KeyboardState keyboard, GamePadState GP1, GamePadState GP2)
        {
            //update world Mat of object:
            world = Matrix.Identity *
                Matrix.CreateScale(Scale) *
                Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll)) *
                Matrix.CreateTranslation(Position);
        }

        /// <summary>
        /// Draw method
        /// </summary>
        /// <param name="view">The View matrix</param>
        /// <param name="Projection">The projection matrix</param>
        /// <param name="Lighting">The light colours and positions</param>
        /// <param name="fogColour">The fog colour</param>
        /// <param name="fogVariables">The fog starting and ending points</param>
        public void Draw(Matrix view, Matrix Projection, Vector3[] Lighting, Vector3 fogColour, int[] fogVariables)
        {
            if (model == null) return;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect e in mesh.Effects)
                {
                    e.EnableDefaultLighting();
                    
                    
                    e.LightingEnabled = true;
                    e.DirectionalLight0.Direction = Lighting[0];
                    e.DirectionalLight0.DiffuseColor = Lighting[1];
                    e.DirectionalLight0.SpecularColor = Lighting[2];
                    e.DirectionalLight1.Direction = Lighting[3];
                    e.DirectionalLight1.DiffuseColor = Lighting[4];
                    e.DirectionalLight1.SpecularColor = Lighting[5];
                    e.DirectionalLight2.Direction = Lighting[6];
                    e.DirectionalLight2.DiffuseColor = Lighting[7];
                    e.DirectionalLight2.SpecularColor = Lighting[8];

                    e.Alpha = 1.0f;
                    e.TextureEnabled = false;
                    e.World = world;
                    e.View = view;
                    e.Projection = Projection;
                }
                mesh.Draw();
            }
        }
        /// <summary>
        /// Overrides onAttributeChange Handler to set implemented object attributes.
        /// will throw an exception if there is a conversion issue.
        /// </summary>
        public override void onAttributeChange()
        {
            base.onAttributeChange();
            Position = new Vector3(Convert.ToSingle(base.getAttribute("x")),
                Convert.ToSingle(base.getAttribute("y")), Convert.ToSingle(base.getAttribute("z")));
            yaw = Convert.ToSingle(base.getAttribute("yaw")) * (float)Math.PI / 180;
            pitch = Convert.ToSingle(base.getAttribute("pitch")) * (float)Math.PI / 180;
            roll = Convert.ToSingle(base.getAttribute("roll")) * (float)Math.PI / 180;
            this.Scale = new Vector3(Convert.ToSingle(base.getAttribute("scaleX")),
                Convert.ToSingle(base.getAttribute("scaleY")), Convert.ToSingle(base.getAttribute("scaleZ")));
            this.modelName = base.getAttribute("modelName");
            this.loadModel();
        }
    }
}
