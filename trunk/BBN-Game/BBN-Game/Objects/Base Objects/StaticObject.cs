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
        nutral = 2
    }

    class StaticObject : DrawableGameComponent
    {
        #region "Variables"
        /// <summary>
        /// Globals
        /// </summary>
        protected Objects.ObjectData.ObjectData shipData = new BBN_Game.Objects.ObjectData.ObjectData();  // the data for rotaiton and position
        Quaternion rotate = Quaternion.CreateFromAxisAngle(Vector3.Up, 0); // Rotation Qauternion
        protected Model model; // model to draw
        Matrix world; // The world Matrix
        Team team;

        #region "Target box data"
        /// <summary>
        /// Drawing the targeting box
        /// </summary>
        VertexBuffer targetBoxVB;
        VertexDeclaration targetBoxDecleration;

        Effect targetBoxE;
        EffectParameter viewPort;

        VertexPositionColor[] targetBoxVertices;

        int numHudLines = 8;
        PrimitiveType typeOfLine = PrimitiveType.LineStrip;
        SpriteFont targetBoxFont;

        #endregion

        /// <summary>
        /// Static variables for rotaion speeds
        /// </summary>
        protected float yawSpeed, rollSpeed, pitchSpeed, mass, greatestLength;

        protected float Health;
        protected float Shield;

        #region "Getters and setters"
        /// <summary>
        /// Getters and setters
        /// </summary>
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
        public float getGreatestLength
        {
            get { return greatestLength; }
        }
        #endregion
        #endregion

        #region "Constructors"
        /// <summary>
        /// Default constructor
        /// Initialises variables
        /// </summary>
        /// <param name="game">The game</param>
        public StaticObject(Game game, Team team, Vector3 position) : base(game)
        {
            Position = position;
            mass = 1000000000f; // static objects dont move
            this.team = team;
            setData();
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
                targetBoxVertices[i] = new VertexPositionColor(Vector3.Zero, Color.Green);
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

            targetBoxFont = Game.Content.Load<SpriteFont>("Fonts/distanceFont");

            targetBoxDecleration = new VertexDeclaration(Game.GraphicsDevice, VertexPositionColor.VertexElements);
            #endregion

            #region "Collision Detection"

            Collision_Detection.CollisionDetectionHelper.setModelData(model);

            #endregion

            base.LoadContent();
        }

        protected virtual void resetModels()
        {}

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
            BoundingSphere localSphere = shipModel.Meshes[0].BoundingSphere;
            localSphere.Center += Position;

            localSphere.Transform(Matrix.CreateScale(ShipMovementInfo.scale));

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

        #region "Target Box details"
        /// <summary>
        /// Draws the bounding shape around the object apon a fake screen situated y the object with the same rotation as the viewing screen
        /// </summary>
        /// <param name="cam">the camera class containing the view matrix</param>
        public void drawSuroundingBox(Camera.CameraMatrices cam, playerObject currentPlayerforViewport)
        {
            // if its the current player dont draw it
            if (this is playerObject)
                if (((playerObject)this).getViewport.Equals(Game.GraphicsDevice.Viewport))
                    return;

            if (IsVisible(cam))
            {                
                Vector2 screenViewport = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

                if (setVertexCoords(cam, screenViewport, currentPlayerforViewport))
                    drawBox(screenViewport);
            }
        }

        /// <summary>
        /// Sets the positions in the vertex data to draw the target box
        /// </summary>
        /// <param name="cam">Camera matrices for the creating of the boxes coords</param>
        /// <param name="screenViewport">A vector to holding {screen width, screen height}</param>
        private Boolean setVertexCoords(Camera.CameraMatrices cam, Vector2 screenViewport, playerObject player)
        {
            Color col = this.Equals(player.Target) ? Color.Red : this.team.Equals(Team.nutral) ? Color.Yellow :
                        this.Team.Equals(player.team) ? Color.Green : Color.Orange;

            float radiusOfObject;
            radiusOfObject = greatestLength; // sets the greatest size of the object

            float distance = (Position - cam.Position).Length(); // distance the object is from the camera
            float radius = (greatestLength / 2) * shipData.scale; // a variable for checking distances away from camera
            // Check if the objectis further away from the camera than its actual size.
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
            if (this is Objects.Destroyer)
                setSquareVertices(screenX, screenY, radiusOfObject, col);
            else if (this is Objects.Fighter)
                setHexagonVertices(screenX, screenY, radiusOfObject, col);
            else if (this is Objects.playerObject)
                setReticleVertices(screenX, screenY, radiusOfObject, col);
            else if (this is Objects.Projectile)
                setTriangleVertices(screenX, screenY, radiusOfObject, col);
            else if (this is Objects.Base)
                setCircleVertices(screenX, screenY, radiusOfObject, col);
            else if (this is Objects.Turret)
                setTriangleReticleVertices(screenX, screenY, radiusOfObject, col);

            // set the y back to the non depth version
            screenY = halfScreenY - ((screenPos.Y / screenPos.W) * halfScreenY);
            float distanceToPlayer = (Position - player.Position).Length();

            drawDistances(distanceToPlayer, screenX, screenY, radiusOfObject, col); // draw the distances to the object

            // set the variable to the new position vectors
            targetBoxVB.SetData<VertexPositionColor>(targetBoxVertices);
            return true;
        }

        private void drawDistances(float distance, float x, float y, float radius, Color col)
        {
            SpriteBatch b = new SpriteBatch(Game.GraphicsDevice);

            b.Begin();
            b.DrawString(targetBoxFont, distance.ToString("0000"), new Vector2(x + radius, y + radius), col);
            b.End();

            b.Dispose();
            b = null;
        }

        #region "Vertex setters (Shapes for target box)"
        /// <summary>
        /// Draws a triangle for the target box
        /// </summary>
        /// <param name="screenX">The position of the object in x coord 2D</param>
        /// <param name="screenY">The position of the object in y coord 2D</param>
        /// <param name="radiusOfObject">The widht/radius of the object (greatest half length)</param>
        private void setTriangleReticleVertices(float screenX, float screenY, float radiusOfObject, Color col)
        {
            setVertexData(11, col);
            typeOfLine = PrimitiveType.TriangleList;

            Vector2 topLeft = new Vector2(screenX - radiusOfObject, screenY + radiusOfObject);
            Vector2 topRight = new Vector2(screenX + radiusOfObject, screenY + radiusOfObject);
            Vector2 botLeft = new Vector2(screenX - radiusOfObject, screenY - radiusOfObject);
            Vector2 botRight = new Vector2(screenX + radiusOfObject, screenY - radiusOfObject);

            float amount = radiusOfObject * 0.25f;

            // top right triangle
            targetBoxVertices[0].Position.X = topRight.X - amount;
            targetBoxVertices[0].Position.Y = topRight.Y - amount;
            targetBoxVertices[1].Position.X = topRight.X + amount/2;
            targetBoxVertices[1].Position.Y = topRight.Y - amount/2;
            targetBoxVertices[2].Position.X = topRight.X - amount/2;
            targetBoxVertices[2].Position.Y = topRight.Y + amount/2;

            // top left
            targetBoxVertices[3].Position.X = topLeft.X + amount;
            targetBoxVertices[3].Position.Y = topLeft.Y - amount;
            targetBoxVertices[4].Position.X = topLeft.X + amount/2;
            targetBoxVertices[4].Position.Y = topLeft.Y + amount/2;
            targetBoxVertices[5].Position.X = topLeft.X - amount/2;
            targetBoxVertices[5].Position.Y = topLeft.Y - amount/2;

            // bot left
            targetBoxVertices[6].Position.X = botLeft.X + amount;
            targetBoxVertices[6].Position.Y = botLeft.Y + amount;
            targetBoxVertices[7].Position.X = botLeft.X - amount/2;
            targetBoxVertices[7].Position.Y = botLeft.Y + amount/2;
            targetBoxVertices[8].Position.X = botLeft.X + amount/2;
            targetBoxVertices[8].Position.Y = botLeft.Y - amount/2;

            // bot right
            targetBoxVertices[9].Position.X = botRight.X - amount;
            targetBoxVertices[9].Position.Y = botRight.Y + amount;
            targetBoxVertices[10].Position.X = botRight.X - amount / 2;
            targetBoxVertices[10].Position.Y = botRight.Y - amount / 2;
            targetBoxVertices[11].Position.X = botRight.X + amount / 2;
            targetBoxVertices[11].Position.Y = botRight.Y + amount / 2;
        }

        /// <summary>
        /// Draws a triangle for the target box
        /// </summary>
        /// <param name="screenX">The position of the object in x coord 2D</param>
        /// <param name="screenY">The position of the object in y coord 2D</param>
        /// <param name="radiusOfObject">The widht/radius of the object (greatest half length)</param>
        private void setTriangleVertices(float screenX, float screenY, float radiusOfObject, Color col)
        {
            setVertexData(3, col);
            typeOfLine = PrimitiveType.LineStrip;

            //Line 1
            targetBoxVertices[0].Position.X = screenX;
            targetBoxVertices[0].Position.Y = screenY + radiusOfObject * 1.8f;

            //Line 2
            targetBoxVertices[1].Position.X = screenX - radiusOfObject * 1.8f;
            targetBoxVertices[1].Position.Y = screenY - radiusOfObject;

            //Line 3
            targetBoxVertices[2].Position.X = screenX + radiusOfObject * 1.8f;
            targetBoxVertices[2].Position.Y = screenY - radiusOfObject;

            //Line 4
            targetBoxVertices[3].Position.X = screenX;
            targetBoxVertices[3].Position.Y = screenY + radiusOfObject * 1.8f;
        }

        /// <summary>
        /// Draws a suqare for the target box
        /// </summary>
        /// <param name="screenX">The position of the object in x coord 2D</param>
        /// <param name="screenY">The position of the object in y coord 2D</param>
        /// <param name="radiusOfObject">The widht/radius of the object (greatest half length)</param>
        private void setSquareVertices(float screenX, float screenY, float radiusOfObject, Color col)
        {
            setVertexData(4, col);
            typeOfLine = PrimitiveType.LineStrip;

            //Line 1
            targetBoxVertices[0].Position.X = screenX - radiusOfObject;
            targetBoxVertices[0].Position.Y = screenY + radiusOfObject;

            //Line 2
            targetBoxVertices[1].Position.X = screenX - radiusOfObject;
            targetBoxVertices[1].Position.Y = screenY - radiusOfObject;

            //Line 3
            targetBoxVertices[2].Position.X = screenX + radiusOfObject;
            targetBoxVertices[2].Position.Y = screenY - radiusOfObject;

            //Line 4
            targetBoxVertices[3].Position.X = screenX + radiusOfObject;
            targetBoxVertices[3].Position.Y = screenY + radiusOfObject;

            //Line 5
            targetBoxVertices[4].Position.X = screenX - radiusOfObject;
            targetBoxVertices[4].Position.Y = screenY + radiusOfObject;
        }

        /// <summary>
        /// Draws 4 lines oposit each other to create a small target selector
        /// </summary>
        /// <param name="screenX">The position of the object in x coord 2D</param>
        /// <param name="screenY">The position of the object in y coord 2D</param>
        /// <param name="radiusOfObject">The widht/radius of the object (greatest half length)</param>
        private void setReticleVertices(float screenX, float screenY, float radiusOfObject, Color col)
        {
            setVertexData(7, col);
            typeOfLine = PrimitiveType.LineList;

            //Line 1
            targetBoxVertices[0].Position.X = screenX - radiusOfObject * 0.5f;
            targetBoxVertices[0].Position.Y = screenY + radiusOfObject * 1f;
            
            targetBoxVertices[1].Position.X = screenX - radiusOfObject * 1f;
            targetBoxVertices[1].Position.Y = screenY + radiusOfObject * 0.5f;

            //Line 2
            targetBoxVertices[2].Position.X = screenX - radiusOfObject * 1f;
            targetBoxVertices[2].Position.Y = screenY - radiusOfObject * 0.5f;

            targetBoxVertices[3].Position.X = screenX - radiusOfObject * 0.5f;
            targetBoxVertices[3].Position.Y = screenY - radiusOfObject * 1f;

            // line 3
            targetBoxVertices[4].Position.X = screenX + radiusOfObject * 0.5f;
            targetBoxVertices[4].Position.Y = screenY - radiusOfObject * 1f;

            targetBoxVertices[5].Position.X = screenX + radiusOfObject * 1f;
            targetBoxVertices[5].Position.Y = screenY - radiusOfObject * 0.5f;

            // line 4
            targetBoxVertices[6].Position.X = screenX + radiusOfObject * 1f;
            targetBoxVertices[6].Position.Y = screenY + radiusOfObject * 0.5f;

            targetBoxVertices[7].Position.X = screenX + radiusOfObject * 0.5f;
            targetBoxVertices[7].Position.Y = screenY + radiusOfObject * 1f;
        }

        /// <summary>
        /// Creates vertices in order to draw a circle around the object
        /// </summary>
        /// <param name="screenX">The X position for the object</param>
        /// <param name="screenY">The y position for the object</param>
        /// <param name="radiusOfObject">the objects greatest length halved</param>
        private void setCircleVertices(float screenX, float screenY, float radiusOfObject, Color col)
        {
            setVertexData(360 / 20, col);
            typeOfLine = PrimitiveType.LineStrip;

            for (int i = 0; i <= 360; i+=20)
            {
                targetBoxVertices[i/20].Position.X = screenX + (float)Math.Sin(MathHelper.ToRadians(i)) * radiusOfObject;
                targetBoxVertices[i/20].Position.Y = screenY + (float)Math.Cos(MathHelper.ToRadians(i)) * radiusOfObject;
            }
        }

        /// <summary>
        /// sets the vertices for the target box to a hexagon
        /// </summary>
        /// <param name="screenX">The position of the object in x coord 2D</param>
        /// <param name="screenY">The position of the object in y coord 2D</param>
        /// <param name="radiusOfObject">The widht/radius of the object (greatest half length)</param>
        private void setHexagonVertices(float screenX, float screenY, float radiusOfObject, Color col)
        {
            setVertexData(8, col);
            typeOfLine = PrimitiveType.LineStrip;

            //Line 1
            targetBoxVertices[0].Position.X = screenX - radiusOfObject / 2;
            targetBoxVertices[0].Position.Y = screenY + radiusOfObject;

            //Line 2
            targetBoxVertices[1].Position.X = screenX - radiusOfObject;
            targetBoxVertices[1].Position.Y = screenY + radiusOfObject / 2;

            //Line 3
            targetBoxVertices[2].Position.X = screenX - radiusOfObject;
            targetBoxVertices[2].Position.Y = screenY - radiusOfObject / 2;

            //Line 4
            targetBoxVertices[3].Position.X = screenX - radiusOfObject / 2;
            targetBoxVertices[3].Position.Y = screenY - radiusOfObject;

            //Line 5
            targetBoxVertices[4].Position.X = screenX + radiusOfObject / 2;
            targetBoxVertices[4].Position.Y = screenY - radiusOfObject;

            //Line 6
            targetBoxVertices[5].Position.X = screenX + radiusOfObject;
            targetBoxVertices[5].Position.Y = screenY - radiusOfObject / 2;

            //Line 7
            targetBoxVertices[6].Position.X = screenX + radiusOfObject;
            targetBoxVertices[6].Position.Y = screenY + radiusOfObject / 2;

            //Line 8
            targetBoxVertices[7].Position.X = screenX + radiusOfObject / 2;
            targetBoxVertices[7].Position.Y = screenY + radiusOfObject;

            //Line 9
            targetBoxVertices[8].Position.X = screenX - radiusOfObject / 2;
            targetBoxVertices[8].Position.Y = screenY + radiusOfObject;
        }
        #endregion
        /// <summary>
        /// Does the actual drawing of the box
        /// </summary>
        /// <param name="screenViewport">Vector 2 holding { screen width, screen height}</param>
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

        /// <summary>
        /// Boolean variable holding the last entry that was used when initialising verteces holder
        /// 
        /// A method that checks to see if it should re initialise the vertices holder in order to change its colours
        /// </summary>
        Color prevCol = Color.Black;
        private void setVertexData(int numLines, Color col)
        {
            if ((numHudLines != numLines) || col != prevCol)
            {
                numHudLines = numLines;
                targetBoxVertices = new VertexPositionColor[numHudLines + 1];
                for (int i = 0; i < numHudLines + 1; i++)
                {
                    targetBoxVertices[i] = new VertexPositionColor(Vector3.Zero, col);
                }
                targetBoxVB = new VertexBuffer(Game.GraphicsDevice, typeof(VertexPositionColor), numHudLines + 1, BufferUsage.None);
                prevCol = col;
            }
        }
        #endregion
        #endregion
    }
}
