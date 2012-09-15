using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
/////
///
/// Author - Benjamin Hugo
/// 
/// This class contains all the data needed for collision detection
////
namespace BBN_Game.Collision_Detection
{
    public static class CollisionDetectionHelper
    {
        /// <summary>
        /// Modify this const to set the number of vertices per bounding sphere. This may speed up
        /// collision detection at the cost of accuracy.
        /// </summary>
        public const int NUM_VERTICES_PER_BOX = 100;
        public const float BLOCK_SIZE_FACTOR = 0.015f;
        /// <summary>
        /// Each model part will have several datastrutures associated with it so we need an array of objects to store them all
        /// </summary>
        /// <param name="part">Model mesh part under consideration</param>
        private static void ConstructCollisionDetectionInfoStore(ModelMeshPart part)
        {
            part.Tag = new object[2];
        }

        /// <summary>
        /// Constructs All data needed using the mothods below (A one call method)
        /// </summary>
        /// <param name="model">The model to be deconstructed into bounding boxes</param>
        public static void setModelData(Model model)
        {
            CollisionDetectionHelper.ConstructMeshPartBoundingSpherees(model);
            CollisionDetectionHelper.ConstructObjectLevelBoundingSphere(model);
            CollisionDetectionHelper.ConstructMeshLevelBoundingSphere(model);
        }

        /// <summary>
        /// Construct a bounding box from the model mesh parts.
        /// This method will construct spheres for every few triangles in an object
        /// Stored in modelmeshpart.tag as a List of BoundingSphere (the last of which is the bounding sphere over the model part)
        /// </summary>
        /// <param name="model">Model to construct spheres for</param>
        public static void ConstructMeshPartBoundingSpherees(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Tag is List<BoundingSphere>)
                        continue;                   //if already calculated, don't calculate again
                    List<BoundingSphere> results = new List<BoundingSphere>();
                    
                    Vector3[] vertices = new Vector3[part.NumVertices];
                    mesh.VertexBuffer.GetData<Vector3>(part.StreamOffset + part.BaseVertex * part.VertexStride, vertices, 0, part.NumVertices, part.VertexStride);
                    int numBlocks = (int)Math.Ceiling(mesh.BoundingSphere.Radius * 2 * BLOCK_SIZE_FACTOR);
                    List<Vector3>[, ,] bins = new List<Vector3>[numBlocks, numBlocks, numBlocks];
                    foreach (Vector3 v in vertices)
                    {
                        int x = Math.Max((int)((v.X + mesh.BoundingSphere.Radius - mesh.BoundingSphere.Center.X) * BLOCK_SIZE_FACTOR), 0);
                        int y = Math.Max((int)((v.Y + mesh.BoundingSphere.Radius - mesh.BoundingSphere.Center.Y) * BLOCK_SIZE_FACTOR), 0);
                        int z = Math.Max((int)((v.Z + mesh.BoundingSphere.Radius - mesh.BoundingSphere.Center.Z) * BLOCK_SIZE_FACTOR), 0);
                        if (bins[x, y, z] == null)
                            bins[x, y, z] = new List<Vector3>();
                        bins[x, y, z].Add(v);
                    }
                    for (int x = 0; x < numBlocks; ++x)
                        for (int y = 0; y < numBlocks; ++y)
                            for (int z = 0; z < numBlocks; ++z)
                            {
                                if (bins[x, y, z] == null) continue;
                                for (int i = 0; i < bins[x,y,z].Count(); i += NUM_VERTICES_PER_BOX)
                                {
                                    List<Vector3> selection = new List<Vector3>();
                                    for (int j = i; j < bins[x, y, z].Count() && j < i + NUM_VERTICES_PER_BOX; ++j)
                                        selection.Add(bins[x, y, z].ElementAt(j));
                                    BoundingSphere bs = BoundingSphere.CreateFromPoints(selection);
                                    results.Add(bs);
                                }
                            }
                    //add bounding sphere over model part
                    BoundingSphere partsphere = results.First();
                    for (int i = 1; i < results.Count; ++i)
                        partsphere = BoundingSphere.CreateMerged(partsphere, results.ElementAt(i));
                    results.Add(partsphere);

                    part.Tag  = results;
                }
        }
        /// <summary>
        /// Constructs a bounding box over the object from the boxes of each sub-part of it
        /// Stored in Model.Tag for later collision detection use
        /// </summary>
        /// <param name="model">Model to construct bounding box for</param>
        public static void ConstructObjectLevelBoundingSphere(Model model)
        {
            if (model.Tag is BoundingSphere)
                return;               //already calculated, don't do it again!
            BoundingSphere result = new BoundingSphere();
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (!(part.Tag is List<BoundingSphere>))
                        throw new Exception("Call the Collision Detection Helper's ConstructMeshPartBoundingSpheres first");
                    foreach (BoundingSphere box in part.Tag as List<BoundingSphere>)
                        result = BoundingSphere.CreateMerged(box, result);
                }
            model.Tag = result;
        }
        /// <summary>
        /// Method to construct bounding boxes around each of the model's meshes. Test against this before testing ModelMeshPart bounding boxes.
        /// Bounding box will be stored in mesh.Tag
        /// </summary>
        /// <param name="model"></param>
        public static void ConstructMeshLevelBoundingSphere(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (mesh.Tag is BoundingSphere)
                    continue;               //already calculated, don't do it again!
                BoundingSphere result = new BoundingSphere();
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (!(part.Tag is List<BoundingSphere>))
                        throw new Exception("Call the Collision Detection Helper's ConstructMeshPartBoundingSpheres first");
                    foreach (BoundingSphere box in part.Tag as List<BoundingSphere>)
                        result = BoundingSphere.CreateMerged(box, result);
                }
                mesh.Tag = result;
            }
        }
        /// <summary>
        /// Method to transform a bounding box's corners into world space
        /// </summary>
        /// <param name="input">untransformed bounding box</param>
        /// <param name="world">world matrix</param>
        /// <returns>transformed bounding box</returns>
        public static BoundingSphere TransformBox(BoundingSphere input, Matrix world)
        {
            Vector3 scale = Vector3.Zero;
            Vector3 translate = Vector3.Zero;
            Quaternion rotation = Quaternion.Identity;
            world.Decompose(out scale, out rotation, out translate);

            float dist = Math.Max(input.Radius*scale.X,
                Math.Max(input.Radius * scale.Y,
                input.Radius*scale.Z));
            return new BoundingSphere(Vector3.Transform(input.Center,world),
                dist);
        }

        /// <summary>
        /// Method to test if a ray intersects the object
        /// </summary>
        /// <param name="rayStart">Start point of ray</param>
        /// <param name="rayEnd">Cut off point of ray</param>
        /// <param name="intersect">Out paramter that returns the distance of intersection or -1 if not intersected</param>
        /// <returns>True iff intersected</returns>
        public static bool rayIntersect(Vector3 rayStart, Vector3 rayEnd, Model model, Matrix world, out float intersect)
        {
            bool intersected = false;
            float intersection = -1;
            //It is much cheaper to transform the ray into object space than to transform all the objects bounding shapes and triangles to world space:
            Matrix WorldInvert = Matrix.Invert(world);
            Ray ray = new Ray(Vector3.Transform(rayStart,WorldInvert), Vector3.TransformNormal(Vector3.Normalize(rayEnd - rayStart),WorldInvert));
            //Check if the model's bounding box is intersected first, if not then there can be no further BoundingSphere and triangle intersections:
            if (model.Tag is BoundingSphere)
            {                
                if (((BoundingSphere)(model.Tag)).Intersects(ray) == null)
                    goto CheckIntersect;
            }
            else
               throw new Exception("Call the Collision Detection Helper's constructObjectLevelBoundingSphere on model load");
            //Now for each mesh part:
            foreach (ModelMesh mesh in model.Meshes)
            {
                //Check meshes' bounding box for an intersection, if not intersected on this level we don't have to check the mesh's parts at all:
                if (!(mesh.Tag is BoundingSphere))
                    throw new Exception("Call the Collision Detection Helper's constructMeshLevelBoundingSphere on model load");
                if (((BoundingSphere)mesh.Tag).Intersects(ray) != null)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        //Check part's bounding boxes for intersection. If none is found we dont have to do triangle perfect intersection:
                        if (!(part.Tag is List<BoundingSphere>))
                            throw new Exception("Call the Collision Detection Helper's Extract Model Data, ConstructMeshPartBoundingSpherees on model load");
                        int bbIndex = 0;
                        foreach (BoundingSphere bb in part.Tag as List<BoundingSphere>)
                        {
                            float? testIntersection = bb.Intersects(ray);
                            if (testIntersection != null)
                            {
                                intersected = true;
                                intersection = testIntersection.Value;
                                goto CheckIntersect;
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
        /// <summary>
        /// Method to check if a point is inside the bounding box of a model
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <param name="aModel">Model to check</param>
        /// <param name="world">transformation matrix</param>
        /// <returns></returns>
        public static bool isPointInModelsBoundingSphere(Vector3 point, Model aModel, Matrix world)
        {
            if (!(aModel.Tag is BoundingSphere))
                throw new Exception("Call ConstructObjectLevelBoundingSphere first");
            if (TransformBox((BoundingSphere)aModel.Tag,world).Contains(point) == ContainmentType.Contains)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Checks if a collision occured between two objects (only with respect to the BoundingSpherees of each of the model's meshparts)
        /// </summary>
        /// <param name="object1">First object's model's data</param>
        /// <param name="object2">Second object's model's data </param>
        /// <param name="object1Transformation">Transformation of first object into world space</param>
        /// <param name="object2Transformation">Transformation of second object into world space</param>
        /// <returns>True if such a collision is detected, false otherwise</returns>
        public static bool isObjectsCollidingOnMeshPartLevel(Model object1, Model object2, Matrix object1Transformation, Matrix object2Transformation, Boolean testOnlyUptoOverAllMesh)
        {
            
            if (!(object1.Tag is BoundingSphere))
                throw new Exception("Call the Collision Detection Helper's constructObjectLevelBoundingSphere on arg1 load");
            if (!(object2.Tag is BoundingSphere))
                throw new Exception("Call the Collision Detection Helper's constructObjectLevelBoundingSphere on arg2 load");
            
            //Check if outer bounding boxes intersect:
            if (!TransformBox((BoundingSphere)object1.Tag, object1Transformation).Intersects(
               TransformBox((BoundingSphere)object2.Tag, object2Transformation)))
                    return false;
            if (testOnlyUptoOverAllMesh)
                return true;
            //Check now if mesh bounding boxes intersect:
            foreach (ModelMesh mesh1 in object1.Meshes)
            {
                if (!(mesh1.Tag is BoundingSphere))
                    throw new Exception("Call the Collision Detection Helper's constructMeshLevelBoundingSphere on arg1 load");
                foreach (ModelMesh mesh2 in object2.Meshes)
                {
                    if (!(mesh2.Tag is BoundingSphere))
                        throw new Exception("Call the Collision Detection Helper's constructMeshLevelBoundingSphere on arg2 load");
                    if (TransformBox((BoundingSphere)mesh1.Tag, object1Transformation).Intersects(
                       TransformBox((BoundingSphere)mesh2.Tag, object2Transformation)))
                    {
                        //Check now if one of the modelmeshparts' bounding boxes intersected:
                        foreach (ModelMeshPart part1 in mesh1.MeshParts)
                        {
                            if (!(part1.Tag is List<BoundingSphere>))
                                throw new Exception("Call the Collision Detection Helper's Extract Model Data, ConstructMeshPartBoundingSpherees on arg1 load");

                            foreach (ModelMeshPart part2 in mesh2.MeshParts)
                            {
                                if (!(part2.Tag is List<BoundingSphere>))
                                    throw new Exception("Call the Collision Detection Helper's Extract Model Data, ConstructMeshPartBoundingSpherees on arg2 load");
                                //now check each of the bounding boxes in the list of boxes for this part against the bounding boxes in the other part's list (first over the whole part)
                                if (TransformBox((part1.Tag as List<BoundingSphere>).Last(), object1Transformation).Intersects(
                                            TransformBox((part2.Tag as List<BoundingSphere>).Last(), object2Transformation)))
                                {
                                    foreach (BoundingSphere aPart1Box in (part1.Tag as List<BoundingSphere>))
                                    {
                                        if (aPart1Box == (part1.Tag as List<BoundingSphere>).Last()) continue;
                                        foreach (BoundingSphere aPart2Box in (part2.Tag as List<BoundingSphere>))
                                        {
                                            if (aPart2Box == (part2.Tag as List<BoundingSphere>).Last()) continue;
                                            if (TransformBox(aPart1Box, object1Transformation).Intersects(
                                                TransformBox(aPart2Box, object2Transformation)))
                                                return true;
                                        }
                                    }
                                }
                            } //foreach part in mesh of model 2
                        } //foreach part in mesh of model 1
                    } //if meshes intersects
                } //foreach mesh in model 2
            } //foreach mesh in model 1
            return false;
        }
    }
}

