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
namespace BBN_Game.Objects
{
    class StaticObject : MapContent
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
        public Boolean visible { get; set; }
        protected Model model;
        protected Matrix world;
        protected String modelName;
        public ContentManager contentLoader;
        /// <summary>
        /// Default constructor
        /// </summary>
        public StaticObject()
        {
            Position = Vector3.Zero;
            world = Matrix.Identity * Matrix.CreateTranslation(Position);
            model = null;
            modelName = null;
            pitch = yaw = roll = 0.0f;
            Scale = Vector3.One;
            visible = true;
        }

        /// <summary>
        /// Constructor with Model
        /// </summary>
        /// <param name="pos">"The position of the object"</param>
        /// <param name="m">"The model for the object"</param>
        public StaticObject(Vector3 pos, Model m)
        {
            Position = pos;
            world = Matrix.Identity * Matrix.CreateTranslation(Position);
            model = m;
            Scale = Vector3.One;
            visible = true;
        }

        /// <summary>
        /// Constructor with loading variable
        /// </summary>
        /// <param name="pos">"The position of the object"</param>
        /// <param name="m">The content loader</param>
        public StaticObject(Vector3 pos, ContentManager m)
        {
            Position = pos;
            world = Matrix.Identity * Matrix.CreateTranslation(Position);
            contentLoader = m;
            loadModel(); // loads the object from its own method
            Scale = Vector3.One;
            visible = true;
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
                CollisionDetectionHelper.ExtractModelData(model);
                CollisionDetectionHelper.ConstructMeshPartBoundingBoxes(model);
                CollisionDetectionHelper.ConstructObjectLevelBoundingBox(model);
                CollisionDetectionHelper.ConstructMeshLevelBoundingBox(model);
            }
        }

        public virtual void unload()
        {
            model = null;

            contentLoader.Unload();
        }

        public override void update(KeyboardState keyboard, GamePadState GP1, GamePadState GP2)
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
        public override void Draw(Matrix view, Matrix Projection, Vector3[] Lighting, Vector3 fogColour, int[] fogVariables)
        {
            if (!visible) return;
            if (model == null) return;                
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect e in mesh.Effects)
                {
                    e.EnableDefaultLighting();
                    e.PreferPerPixelLighting = true;

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
            visible = Convert.ToBoolean(base.getAttribute("visible"));
        }
        /// <summary>
        /// Method to test if a ray intersects the object
        /// </summary>
        /// <param name="rayStart">Start point of ray</param>
        /// <param name="rayEnd">Cut off point of ray</param>
        /// <param name="intersect">Out paramter that returns the distance of intersection or -1 if not intersected</param>
        /// <returns>True iff intersected</returns>
        public bool rayIntersect(Vector3 rayStart, Vector3 rayEnd, out float intersect)
        {
            bool intersected = false;
            float intersection = -1;
            //It is much cheaper to transform the ray into object space than to transform all the objects bounding shapes and triangles to world space:
            Matrix WorldInvert = Matrix.Invert(world);
            Ray ray = new Ray(Vector3.Transform(rayStart,WorldInvert), Vector3.TransformNormal(Vector3.Normalize(rayEnd - rayStart),WorldInvert));
            //Check if the model's bounding box is intersected first, if not then there can be no further boundingbox and triangle intersections:
            if (model.Tag is BoundingBox)
            {                
                if (((BoundingBox)(model.Tag)).Intersects(ray) == null)
                    goto CheckIntersect;
            }
            else
               throw new Exception("Call the Collision Detection Helper's constructObjectLevelBoundingBox on model load");
            //Now for each mesh part:
            foreach (ModelMesh mesh in model.Meshes)
            {
                //Check meshes' bounding box for an intersection, if not intersected on this level we don't have to check the mesh's parts at all:
                if (!(mesh.Tag is BoundingBox))
                    throw new Exception("Call the Collision Detection Helper's constructMeshLevelBoundingBox on model load");
                if (((BoundingBox)mesh.Tag).Intersects(ray) != null)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        //Check part's bounding boxes for intersection. If none is found we dont have to do triangle perfect intersection:
                        if (!(part.Tag is object[] || (part.Tag as object[]).Length >= 2 || (part.Tag as object[])[0] is List<Triangle> || (part.Tag as object[])[1] is List<BoundingBox>))
                            throw new Exception("Call the Collision Detection Helper's Extract Model Data, ConstructMeshPartBoundingBoxes on model load");
                        int bbIndex = 0;
                        foreach (BoundingBox bb in (part.Tag as object[])[1] as List<BoundingBox>)
                        {
                            if (bb.Intersects(ray) != null)
                            {
                                //Well we are definitely in one of the bounding boxes in one of the model's parts. Get triangles that is in this bounding box and check them:
                                for (int i = bbIndex;
                                    i < bbIndex + CollisionDetectionHelper.NUM_TRIANGLES_PER_BOX && i < ((part.Tag as object[])[0] as List<Triangle>).Count;
                                    ++i)
                                {
                                    Triangle tri = ((part.Tag as object[])[0] as List<Triangle>).ElementAt(i);
                                    //Check ray for intersection:
                                    float? dist = ray.Intersects(new Plane(tri.v1, tri.v2, tri.v3));
                                    if (dist != null)
                                    {
                                        //Check that intersection is a point within the triangle (we already know this point is coplanar to the triangle, so we can just make sure that the angle around the point is 2*PI relative to the points of the triangle:
                                        Vector3 ptOfIntersection = ray.Position + ray.Direction * (float)dist;  //point along ray where intersection took place
                                        Vector3 v1 = Vector3.Normalize(tri.v1 - ptOfIntersection);
                                        Vector3 v2 = Vector3.Normalize(tri.v2 - ptOfIntersection);
                                        Vector3 v3 = Vector3.Normalize(tri.v3 - ptOfIntersection);
                                        double sumOfAngles = Math.Acos(Vector3.Dot(v1, v2)) +
                                        Math.Acos(Vector3.Dot(v2, v3)) +
                                        Math.Acos(Vector3.Dot(v3, v1));
                                        if (Math.Abs(sumOfAngles - Math.PI * 2) < 0.0001)
                                        {
                                            intersection = (float)(intersected ? (intersection < dist ? intersection : dist) : dist);
                                            intersected = true;
                                        }
                                    }
                                } // for each triangle in bounding box
                            } //if ray intersects bounding box
                            ++bbIndex;
                        } //for each bounding box
                    } // for each model mesh part
                }// if mesh intersects
            } // for each model mesh

            CheckIntersect:
            if (intersected)
                intersect = intersection;
            else
                intersect = -1;
            return intersected;
        }
    }
}
