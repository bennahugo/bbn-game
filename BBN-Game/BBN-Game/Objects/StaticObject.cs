using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region "XNA Using Statements"
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
#endregion

/////
///
/// Author - Brandon James Talbot
/// 
/// This is the base class for the entire Object tree
/// This contains the variables required for all classes
////

namespace BBN_Game.Objects
{
    class StaticObject : DrawableGameComponent
    {
        #region "Variables"
        /// <summary>
        /// Globals
        /// </summary>
        protected Objects.ObjectData.ObjectData shipData; // the data for rotaiton and position
        Quaternion rotate; // Rotation Qauternion
        protected Model model; // model to draw
        Matrix world; // The world Matrix

        /// <summary>
        /// Static variables for rotaion speeds
        /// </summary>
        protected float yawSpeed, rollSpeed, pitchSpeed, mass;

        #region "Getters and setters"
        /// <summary>
        /// Getters and setters
        /// </summary>
        public float getRollSpeed
        {
            get { return rollSpeed; }
        }
        public float getpitchSpeed
        {
            get { return pitchSpeed; }
        }
        public float getYawSpeed
        {
            get { return yawSpeed; }
        }
        public Vector3 Position 
        {
            get { return shipData.position; }
            set { shipData.position = value; }
        }
        public Quaternion rotation
        {
            get { return rotate; }
            set { rotate = value;}
        }
        public Model shipModel
        {
            get { return model; }
            set { model = value; }
        }
        public float Mass
        {
            get { return mass; }
        }
        public ObjectData.ObjectData ShipMovementInfo
        {
            get { return shipData; }
            set { shipData = value; }
        }
        #endregion
        #endregion

        #region "Constructors"
        /// <summary>
        /// Default constructor
        /// Initialises variables
        /// </summary>
        /// <param name="game">The game</param>
        public StaticObject(Game game) : base(game)
        {
            shipData = new BBN_Game.Objects.ObjectData.ObjectData();
            Position = Vector3.Zero;
            rotate = Quaternion.CreateFromAxisAngle(Vector3.Up, 0);
            mass = 1000000000f; // static objects dont move
        }
        #endregion

        #region "Data & update Methods"
        /// <summary>
        /// Initialize method
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Load content method
        /// This adds the creation of bounding boxes for current model
        /// </summary>
        protected virtual void LoadContent()
        {
            #region "Collision Detection"

            Collision_Detection.CollisionDetectionHelper.setModelData(model);

            #endregion
            base.LoadContent();
        }

        /// <summary>
        /// Update method
        /// Sets the world matrix
        /// </summary>
        /// <param name="gt">Game time variable</param>
        public override void Update(GameTime gt)
        {
            setWorldMatrix((float)gt.ElapsedGameTime.TotalSeconds, Matrix.CreateFromQuaternion(rotate));

            base.Update(gt);
        }

        /// <summary>
        /// Sets the world matrix
        /// </summary>
        /// <param name="time">The game time variable</param>
        /// <param name="m">Rotation matrix</param>
        public virtual void setWorldMatrix(float time, Matrix m)
        {
            #region "Rotation"
            Quaternion pitch = Quaternion.CreateFromAxisAngle(m.Right, MathHelper.ToRadians(shipData.pitch) * time * pitchSpeed);
            Quaternion roll = Quaternion.CreateFromAxisAngle(m.Backward, MathHelper.ToRadians(shipData.roll) * time * rollSpeed);
            Quaternion yaw = Quaternion.CreateFromAxisAngle(m.Up, MathHelper.ToRadians(shipData.yaw) * time * yawSpeed);

            rotate = yaw * pitch * roll * rotate;
            rotate.Normalize();
            #endregion

            world = Matrix.CreateFromQuaternion(rotate);
            world.Translation = Position;

            shipData.resetAngles();
        }

        /// <summary>
        /// Determines if the object is currently visible with the current camera
        /// </summary>
        /// <param name="camera">Camera Class</param>
        /// <returns>Boolean value - True is visible -- false - not visible</returns>
        public virtual bool IsVisible(Camera.CameraMatrices camera)
        {
            BoundingSphere localSphere = shipModel.Meshes[0].BoundingSphere;
            localSphere.Center += Position;

            ContainmentType contains = camera.getBoundingFrustum.Contains(localSphere);
            if (contains == ContainmentType.Contains || contains == ContainmentType.Intersects)
                return true;

            return false;
        }

        #endregion

        #region "Draw Methods"
        /// <summary>
        /// Draw method for the object
        /// This draws without lighting effects and fog
        /// </summary>
        /// <param name="gameTime">Game time</param>
        /// <param name="cam">Camera class</param>
        public virtual void Draw(GameTime gameTime, Camera.CameraMatrices cam)
        {
            foreach (ModelMesh m in model.Meshes)
            {
                foreach (BasicEffect e in m.Effects)
                {
                    e.EnableDefaultLighting();
                    e.PreferPerPixelLighting = true;
                    e.Parameters["World"].SetValue(world);
                    e.Parameters["View"].SetValue(cam.View);
                    e.Parameters["Projection"].SetValue(cam.Projection);
                }
                m.Draw();
            }

            base.Draw(gameTime);
        }

        public void drawSuroundingBox()
        {

        }

        #endregion
    }
}
