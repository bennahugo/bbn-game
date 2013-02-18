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

    enum Team
    {
        Red = 0,
        Blue = 1,
        neutral = 2
    }

    class StaticObject : DrawableGameComponent, Grid.GridObjectInterface
    {
        #region "Variables"
        /// <summary>
        /// Globals
        /// </summary>
        protected Objects.ObjectData.ObjectData shipData = new BBN_Game.Objects.ObjectData.ObjectData();  // the data for rotaiton and position
        Quaternion rotate = Quaternion.CreateFromAxisAngle(Vector3.Up, 0); // Rotation Qauternion
        protected Model model; // model to draw
        protected Matrix world; // The world Matrix
        Team team;

        protected Texture2D healthBarTex; 

        #region "Target box data"
        /// <summary>
        /// Drawing the targeting box
        /// </summary>
        VertexBuffer targetBoxVB;
        VertexDeclaration targetBoxDecleration;

        Effect targetBoxE;
        EffectParameter viewPort;

        protected VertexPositionColor[] targetBoxVertices;

        protected int numHudLines;
        protected PrimitiveType typeOfLine {get ; set;}

        SpriteFont targetBoxFont;

        #endregion

        #region "Grid blocks"
        protected List<Vector3> gridLocations;
        #endregion  

        protected BoundingSphere Bsphere;

        /// <summary>
        /// Static variables for rotaion speeds
        /// </summary>
        protected float yawSpeed, rollSpeed, pitchSpeed, mass, greatestLength;

        protected float Health, totalHealth;
        protected float Shield;

        #region "Getters and setters"
        /// <summary>
        /// Getters and setters
        /// </summary>
        public BoundingSphere getBoundingSphere()
        {
            BoundingSphere sphere = Bsphere;
            sphere.Center = Position;
            return sphere;
        }
        public void doDamage(float dmg)
        {
            Health -= dmg;
        }
        public float getHealth
        {
            get { return Health; }
        }
        public float getTotalHealth
        {
            get { return totalHealth; }
        }
        public Team Team
        {
            get { return team; }
            set { team = value; }
        }
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
        public float setRollSpeed
        {
            set { rollSpeed = value; }
        }
        public float setpitchSpeed
        {
            set { pitchSpeed = value; }
        }
        public float setYawSpeed
        {
            set { yawSpeed = value; }
        }
        public Vector3 Position
        {
            get { return shipData.position; }
            set { shipData.position = value; Bsphere.Center = value; }
        }
        public Quaternion rotation
        {
            get { return rotate; }
            set { rotate = value; }
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
        public float getGreatestLength
        {
            get { return greatestLength; }
        }
        public Matrix getWorld
        {
            get { return world; }
        }
        #endregion
        #endregion

        #region "Grid required Methods"
        /// <summary>
        /// Returns the count of grid locations in the list
        /// </summary>
        /// <returns>number of locations</returns>
        public int getCapacity()
        {
            return gridLocations.Count;
        }
        /// <summary>
        /// Gets the specified location in the grid
        /// </summary>
        /// <param name="index">the index of the location</param>
        /// <returns>the vector 3 location</returns>
        public Vector3 getLocation(int index)
        {
            return gridLocations.ElementAt(index);
        }
        /// <summary>
        /// Clears the grid locations
        /// </summary>
        public void removeAllLocations()
        {
            gridLocations = new List<Vector3>();
        }
        /// <summary>
        /// adds a grid location to the list
        /// </summary>
        /// <param name="location">Location in the grid</param>
        public void setNewLocation(Vector3 location)
        {
            gridLocations.Add(location);
        }
        #endregion

        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// Initialises variables
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="position">The Position of the object</param>
        /// <param name="team">The team for the object</param>
        public StaticObject(Game game, Team team, Vector3 position)
            : base(game)
        {
            Position = position;
            mass = 1000000000f; // static objects dont move
            this.team = team;
            setData();
            gridLocations = new List<Vector3>();
        }

        /// <summary>
        /// Sets the data for the object (these are defaults)
        /// </summary>
        protected virtual void setData()
        {
            this.mass = 100000000000000000f;
            this.rollSpeed = 30;
            this.pitchSpeed = 30;
            this.yawSpeed = rollSpeed * 2;
            greatestLength = 10.0f;
            Shield = 100;
            Health = 100;
            totalHealth = 100;
            numHudLines = 4;
            typeOfLine = PrimitiveType.LineStrip;
        }
        #endregion

        #region "Data & update Methods"
        /// <summary>
        /// Initialize method
        /// </summary>
        public override void Initialize()
        {
            /// set the basic version of the box drawer
            targetBoxVertices = new VertexPositionColor[numHudLines + 1];
            for (int i = 0; i < numHudLines + 1; i++)
            {
                targetBoxVertices[i] = new VertexPositionColor(Vector3.Zero, Color.White);
            }
            targetBoxVB = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionColor), numHudLines + 1, BufferUsage.None);

            base.Initialize();
        }

        /// <summary>
        /// Load content method
        /// This adds the creation of bounding boxes for current model
        /// </summary>
        public virtual void LoadContent()
        {
            // get modal first
            resetModels();

            #region "Target box data"
            targetBoxE = Game.Content.Load<Effect>("Shader/targetBox");
            viewPort = targetBoxE.Parameters["viewPort"];

            healthBarTex = Game.Content.Load<Texture2D>("HudTextures/GlobalHealthBar");

            targetBoxFont = Game.Content.Load<SpriteFont>("Fonts/distanceFont");

            targetBoxDecleration = new VertexDeclaration(Game.GraphicsDevice, VertexPositionColor.VertexElements);
            #endregion

            // neeeded for greatestLength
            this.greatestLength = getGreatestLengthValue();

            base.LoadContent();
        }

        /// <summary>
        /// This resets the models for the current model (Allows for setting of the models of the character)
        /// </summary>
        protected virtual void resetModels()
        {
            #region "Collision Detection"

            Collision_Detection.CollisionDetectionHelper.setModelData(model);

            #endregion
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

            world = Matrix.CreateScale(shipData.scale) * Matrix.CreateFromQuaternion(rotate);
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
            BoundingSphere localSphere = Bsphere;

            localSphere.Center = Position;

            ContainmentType contains = camera.getBoundingFrustum.Contains(localSphere);
            if (contains == ContainmentType.Contains || contains == ContainmentType.Intersects)
                return true;

            return false;
        }

        protected virtual BoundingSphere createShpere()
        {
            BoundingSphere sphere = new BoundingSphere();

            sphere = new BoundingSphere();
            foreach (ModelMesh m in model.Meshes)
            {
                if (sphere.Radius == 0)
                    sphere = m.BoundingSphere;
                else
                    sphere = BoundingSphere.CreateMerged(sphere, m.BoundingSphere);
            }
            sphere.Radius *= this.shipData.scale;

            return sphere;
        }

        private int getGreatestLengthValue()
        {
            Bsphere = createShpere();

            return (int)(Bsphere.Radius * 2);
        }

        public virtual void killObject()
        {
            Controller.GameController.removeObject(this);  
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
            if (!IsVisible(cam))
                return;

            if (((cam.Position - Position).Length() > 600) && !(this is Planets.Planet) && !(this is Asteroid)) // depth culling
                return;


            foreach (ModelMesh m in model.Meshes)
            {
                foreach (BasicEffect e in m.Effects)
                {
                    e.Parameters["World"].SetValue(world);
                    e.Parameters["View"].SetValue(cam.View);
                    e.Parameters["Projection"].SetValue(cam.Projection);
                    e.LightingEnabled = true;

                    e.PreferPerPixelLighting = true;
                    e.DirectionalLight0.Enabled = true;
                    e.DirectionalLight0.DiffuseColor = new Vector3(0.9f, 0.9f, 0.9f);
                    e.DirectionalLight0.SpecularColor = new Vector3(0.7f, 0.7f, 0.7f);
                    e.AmbientLightColor = new Vector3(0.6f, 0.6f, 0.6f);
                    e.DirectionalLight0.Direction = new Vector3(-1, 0, 0);
                }
                m.Draw();
            }

            base.Draw(gameTime);
        }

        #region "Target Boxes"
        /// <summary>
        /// Draws the Target vox for the player
        /// </summary>
        /// <param name="cam">Camera MAtrices class</param>
        /// <param name="currentPlayerforViewport">The current player for the viewport</param>
        public void drawSuroundingBox(SpriteBatch b, Camera.CameraMatrices cam, playerObject currentPlayerforViewport)
        {
            //if its the current player dont draw it
            if (this is playerObject)
                if (((playerObject)this).getViewport.Equals(Game.GraphicsDevice.Viewport))
                    return;

            if (this is Objects.Bullet || this is Objects.Planets.Planet || this is Objects.Asteroid) // dont draw for bullets
                return;

            if ((cam.Position - Position).Length() > 800) // depth culling
                return;

            if (IsVisible(cam))
            {
                Vector2 screenViewport = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                if (setVertexCoords(b, cam, screenViewport, currentPlayerforViewport))
                    drawBox(screenViewport);
            }
        }

        /// <summary>
        /// Sets up the viewport of the object
        /// </summary>
        /// <param name="cam">The camera MAtrices</param>
        /// <param name="screenViewport">The Screen viewport dimensions</param>
        /// <param name="player">The player for the current viewport</param>
        /// <returns></returns>
        private Boolean setVertexCoords(SpriteBatch b, Camera.CameraMatrices cam, Vector2 screenViewport, playerObject player)
        {
            Color col;
            if (this is Objects.Turret)
                col = ((Objects.Turret)this).Repairing ? Color.Aqua : this.Equals(player.Target) ? Color.Red : this.team.Equals(Team.neutral) ? Color.Yellow :
                        this.Team.Equals(player.team) ? Color.Green : Color.Orange;
            else
                col = this.Equals(player.Target) ? Color.Red : this.team.Equals(Team.neutral) ? Color.Yellow :
                        this.Team.Equals(player.team) ? Color.Green : Color.Orange;

            float radiusOfObject;
            radiusOfObject = greatestLength * 5f; // sets the greatest size of the object

            float distance = (Position - cam.Position).Length(); // distance the object is from the camera
            float radius = (greatestLength / 2); // a variable for checking distances away from camera
            //Check if the objectis further away from the camera than its actual size.
            if (distance > radius)
            {
                float angularSize = (float)Math.Tan(radius / distance); // calculate the size differance due to distance away
                radiusOfObject = angularSize * GraphicsDevice.Viewport.Height / MathHelper.ToRadians(cam.viewAngle); // change the size of the object in accordance to the viewing angle
            }

            // The view and projection matrices together
            Matrix viewProj = cam.View * cam.Projection;
            Vector4 screenPos = Vector4.Transform(Position, viewProj); // the position the screen is at according to the matrices

            float halfScreenY = screenViewport.Y / 2.0f; // half the size of the screen
            float halfScreenX = screenViewport.X / 2.0f; // half the size of the screen

            float screenY = ((screenPos.Y / screenPos.W) * halfScreenY) + halfScreenY; // the position of the object in 2d space y
            float screenX = ((screenPos.X / screenPos.W) * halfScreenX) + halfScreenX; // the position of the object in 2d space x

            // set positions for lines to draw 
            setVertexPosition(screenX, screenY, radiusOfObject, col);

            // set the y back to the non depth version
            screenY = halfScreenY - ((screenPos.Y / screenPos.W) * halfScreenY);
            float distanceToPlayer = (cam.Position - Position).Length();

            drawData(b, distanceToPlayer, screenX, screenY, radiusOfObject, col, player); // draw the distances to the object

            // set the variable to the new position vectors
            targetBoxVB.SetData<VertexPositionColor>(targetBoxVertices);
            return true;
        }

        /// <summary>
        /// This sets the shape of the vertices for the object
        /// </summary>
        /// <param name="screenX">The objects x co-ord</param>
        /// <param name="screenY">The objects y co-ord</param>
        /// <param name="radiusOfObject">The size of the object</param>
        /// <param name="col">The colour of the object</param>
        protected virtual void setVertexPosition(float screenX, float screenY, float radiusOfObject, Color col)
        {
            //Line 1
            targetBoxVertices[0].Position.X = screenX - radiusOfObject;
            targetBoxVertices[0].Position.Y = screenY + radiusOfObject;
            targetBoxVertices[0].Color = col;

            //Line 2
            targetBoxVertices[1].Position.X = screenX - radiusOfObject;
            targetBoxVertices[1].Position.Y = screenY - radiusOfObject;
            targetBoxVertices[1].Color = col;

            //Line 3
            targetBoxVertices[2].Position.X = screenX + radiusOfObject;
            targetBoxVertices[2].Position.Y = screenY - radiusOfObject;
            targetBoxVertices[2].Color = col;

            //Line 4
            targetBoxVertices[3].Position.X = screenX + radiusOfObject;
            targetBoxVertices[3].Position.Y = screenY + radiusOfObject;
            targetBoxVertices[3].Color = col;

            //Line 5
            targetBoxVertices[4].Position.X = screenX - radiusOfObject;
            targetBoxVertices[4].Position.Y = screenY + radiusOfObject;
            targetBoxVertices[4].Color = col;
        }

        /// <summary>
        /// Draws the distance of the object from the player
        /// </summary>
        /// <param name="distance">Distance of the object</param>
        /// <param name="x">X pos of the object</param>
        /// <param name="y">Y pos of the object</param>
        /// <param name="radius">Radius of the object</param>
        /// <param name="col">Colour for the box</param>
        private void drawData(SpriteBatch b, float distance, float x, float y, float radius, Color col, Objects.playerObject player)
        {
            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            float healthPercent = this.Health / this.totalHealth;

            float heightPercent = ((radius*2) / (greatestLength/2)) / 100;

            b.Begin();
            if (this.Equals(player.Target))
                b.Draw(healthBarTex, new Rectangle((int)(x - radius), (int)(y - radius - (25 * heightPercent)), (int)(radius * 2 * healthPercent), (int)(20 * heightPercent)), new Rectangle(0, 0, (int)(healthBarTex.Width * healthPercent), healthBarTex.Height), new Color(1 - (Health / totalHealth), (Health / totalHealth), 0));

            b.DrawString(targetBoxFont, distance.ToString("0000"), new Vector2(x + radius, y + radius), col);
            b.End();

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
        }

        /// <summary>
        /// This does the actual drawing of the object
        /// </summary>
        /// <param name="screenViewport">Size of the viewport</param>
        private void drawBox(Vector2 screenViewport)
        {
            targetBoxE.Begin();
            targetBoxE.Techniques[0].Passes[0].Begin();
            viewPort.SetValue(screenViewport);
            targetBoxE.CommitChanges();

            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            GraphicsDevice.VertexDeclaration = targetBoxDecleration;
            GraphicsDevice.Vertices[0].SetSource(targetBoxVB, 0, VertexPositionColor.SizeInBytes);

            GraphicsDevice.DrawPrimitives(typeOfLine, 0, numHudLines);

            GraphicsDevice.Vertices[0].SetSource(null, 0, 0);

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            targetBoxE.Techniques[0].Passes[0].End();
            targetBoxE.End();
        }
        #endregion
        #endregion
    }
}
