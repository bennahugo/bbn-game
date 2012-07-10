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
        /// Modify this const to set the number of bounding spheres per triangle. This may speed up
        /// collision detection, but it can slow down per triangle collision detection if number is too large.
        /// For main game this number should be larger, since collisions are not detected per triangle there.
        /// </summary>
        public const int NUM_TRIANGLES_PER_BOX = 45;
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
            CollisionDetectionHelper.ExtractModelData(model);
            CollisionDetectionHelper.ConstructMeshPartBoundingBoxes(model);
            CollisionDetectionHelper.ConstructObjectLevelBoundingBox(model);
            CollisionDetectionHelper.ConstructMeshLevelBoundingBox(model);
        }

        /// <summary>
        /// Method to extract triangles from mesh
        /// Adapted from http://www.enchantedage.com/vertices-and-bounding-box-from-model-and-vertex-buffer-in-xna-framework
        /// It sets the tag[0] field of each ModelMeshPart in the model to the List of Triangle contained within it.
        /// This can then be transformed and used for triangle-perfect collision detection
        /// </summary>
        /// <param name="mesh">model to be parsed</param>
        public static void ExtractModelData(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Tag is object[] && (part.Tag as object[]).Length >= 2)
                        if ((part.Tag as object[])[0] is List<Triangle>)
                            continue; //already calculated, don't calculate again.
                    List<Triangle> result = new List<Triangle>();
                    Vector3[] vectors = new Vector3[part.NumVertices];
                    mesh.VertexBuffer.GetData<Vector3>(part.StreamOffset + part.BaseVertex * part.VertexStride,
                        vectors, 0, part.NumVertices, part.VertexStride);

                    if (mesh.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
                    {
                        short[] indices = new short[part.PrimitiveCount * 3];
                        mesh.IndexBuffer.GetData<short>(part.StartIndex * 2, indices, 0, part.PrimitiveCount * 3);
                        for (int i = 0; i < part.PrimitiveCount; ++i)
                        {
                            Triangle t = new Triangle();
                            t.v1 = vectors[indices[i * 3 + 0]];
                            t.v2 = vectors[indices[i * 3 + 1]];
                            t.v3 = vectors[indices[i * 3 + 2]];
                            result.Add(t);
                        }
                    }
                    else //32 bits
                    {
                        int[] indices = new int[part.PrimitiveCount * 3];
                        mesh.IndexBuffer.GetData<int>(part.StartIndex * 2, indices, 0, part.PrimitiveCount * 3);
                        for (int i = 0; i < part.PrimitiveCount; ++i)
                        {
                            Triangle t = new Triangle();
                            t.v1 = vectors[indices[i * 3 + 0]];
                            t.v2 = vectors[indices[i * 3 + 1]];
                            t.v3 = vectors[indices[i * 3 + 2]];
                            result.Add(t);
                        }
                    }
                    ConstructCollisionDetectionInfoStore(part);
                    (part.Tag as object[])[0] = result;
                }
        }
        /// <summary>
        /// Construct a bounding box from the model mesh parts.
        /// We need to use a bounding box to accomodate for non-uniform scaling (spheres cannot scale non-uniformly, since they are not spheres then technically).
        /// This method will construct spheres for every few triangles in an object
        /// Stored in modelmeshpart.tag[1] as a List of BoundingBox
        /// </summary>
        /// <param name="model">Model to construct spheres for</param>
        public static void ConstructMeshPartBoundingBoxes(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (!(part.Tag is object[] || (part.Tag as object[]).Length >= 2 || (part.Tag as object[])[0] is List<Triangle>))
                        throw new Exception("Call the Collision Detection Helper's Extract Model Data first");
                    if ((part.Tag as object[])[1] is List<BoundingBox>)
                        continue;                   //if already calculated, don't calculate again
                    List<Triangle> currentList = (part.Tag as object[])[0] as List<Triangle>;
                    List<BoundingBox> results = new List<BoundingBox>();
                    int numberOfBoxes = 0;
                    for (int i = 0;;)
                    {
                        List<Vector3> pointList = new List<Vector3>();
                        //add until we added the correct number of triangles
                        if (i + NUM_TRIANGLES_PER_BOX < currentList.Count)
                            for (int j = 0; j < NUM_TRIANGLES_PER_BOX; j++)
                            {
                                Triangle currentTriangle = currentList.ElementAt(i + j);
                                pointList.Add(currentTriangle.v1);
                                pointList.Add(currentTriangle.v2);
                                pointList.Add(currentTriangle.v3);
                            }
                        else //or until we added to the end of the triangle list
                            for (int j = i; j < currentList.Count; j++)
                            {
                                Triangle currentTriangle = currentList.ElementAt(j);
                                pointList.Add(currentTriangle.v1);
                                pointList.Add(currentTriangle.v2);
                                pointList.Add(currentTriangle.v3);
                            }
                        //now add to model part list
                        results.Add(BoundingBox.CreateFromPoints(pointList));
                        numberOfBoxes++;
                        //increment i by correct amount (number of triangles or the end of the list, whichever comes first)
                        i = Math.Min(i + NUM_TRIANGLES_PER_BOX, currentList.Count);
                        if (i == currentList.Count)
                            break;
                    }
                    (part.Tag as object[])[1] = results;
                }
        }
        /// <summary>
        /// Constructs a bounding box over the object from the boxes of each sub-part of it
        /// Stored in Model.Tag for later collision detection use
        /// </summary>
        /// <param name="model">Model to construct bounding box for</param>
        public static void ConstructObjectLevelBoundingBox(Model model)
        {
            if (model.Tag is BoundingBox)
                return;               //already calculated, don't do it again!
            BoundingBox result = new BoundingBox();
            foreach (ModelMesh mesh in model.Meshes)
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (!(part.Tag is object[] || (part.Tag as object[]).Length >= 2 || (part.Tag as object[])[0] is List<Triangle> || (part.Tag as object[])[1] is List<BoundingSphere>))
                        throw new Exception("Call the Collision Detection Helper's Extract Model Data and ConstructMeshPartBoundingSpheres first");
                    foreach (BoundingBox box in (part.Tag as object[])[1] as List<BoundingBox>)
                    {
                        result = BoundingBox.CreateMerged(box, result);
                    }
                }
            model.Tag = result;
        }
        /// <summary>
        /// Method to construct bounding boxes around each of the model's meshes. Test against this before testing ModelMeshPart bounding boxes.
        /// Bounding box will be stored in mesh.Tag
        /// </summary>
        /// <param name="model"></param>
        public static void ConstructMeshLevelBoundingBox(Model model)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                if (mesh.Tag is BoundingBox)
                    continue;               //already calculated, don't do it again!
                BoundingBox result = new BoundingBox();
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (!(part.Tag is object[] || (part.Tag as object[]).Length >= 2 || (part.Tag as object[])[0] is List<Triangle> || (part.Tag as object[])[1] is List<BoundingSphere>))
                        throw new Exception("Call the Collision Detection Helper's Extract Model Data and ConstructMeshPartBoundingSpheres first");
                    foreach (BoundingBox box in (part.Tag as object[])[1] as List<BoundingBox>)
                    {
                        result = BoundingBox.CreateMerged(box, result);
                    }
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
        public static BoundingBox TransformBox(BoundingBox input, Matrix world)
        {
            Vector3[] points = input.GetCorners();
            for (int i = 0; i < points.Length; ++i)
                points[i] = Vector3.Transform(points[i],world);
            return BoundingBox.CreateFromPoints(points);
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
        /// <summary>
        /// Method to check if a point is inside the bounding box of a model
        /// </summary>
        /// <param name="point">Point to check</param>
        /// <param name="aModel">Model to check</param>
        /// <param name="world">transformation matrix</param>
        /// <returns></returns>
        public static bool isPointInModelsBoundingBox(Vector3 point, Model aModel, Matrix world)
        {
            if (!(aModel.Tag is BoundingBox))
                throw new Exception("Call ConstructObjectLevelBoundingBox first");
            if (TransformBox((BoundingBox)aModel.Tag,world).Contains(point) == ContainmentType.Contains)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Checks if a collision occured between two objects (only with respect to the boundingboxes of each of the model's meshparts)
        /// </summary>
        /// <param name="object1">First object's model's data</param>
        /// <param name="object2">Second object's model's data </param>
        /// <param name="object1Transformation">Transformation of first object into world space</param>
        /// <param name="object2Transformation">Transformation of second object into world space</param>
        /// <returns>True if such a collision is detected, false otherwise</returns>
        public static bool isObjectsCollidingOnMeshPartLevel(Model object1, Model object2, Matrix object1Transformation, Matrix object2Transformation)
        {
            
            if (!(object1.Tag is BoundingBox))
                throw new Exception("Call the Collision Detection Helper's constructObjectLevelBoundingBox on arg1 load");
            if (!(object2.Tag is BoundingBox))
                throw new Exception("Call the Collision Detection Helper's constructObjectLevelBoundingBox on arg2 load");
            
            //Check if outer bounding boxes intersect:
            if (!TransformBox((BoundingBox)object1.Tag, object1Transformation).Intersects(
               TransformBox((BoundingBox)object2.Tag, object2Transformation)))
                    return false;
                
            //Check now if mesh bounding boxes intersect:
            foreach (ModelMesh mesh1 in object1.Meshes)
            {
                if (!(mesh1.Tag is BoundingBox))
                    throw new Exception("Call the Collision Detection Helper's constructMeshLevelBoundingBox on arg1 load");
                foreach (ModelMesh mesh2 in object2.Meshes)
                {
                    if (!(mesh2.Tag is BoundingBox))
                        throw new Exception("Call the Collision Detection Helper's constructMeshLevelBoundingBox on arg2 load");
                    if (TransformBox((BoundingBox)mesh1.Tag, object1Transformation).Intersects(
                       TransformBox((BoundingBox)mesh2.Tag, object2Transformation)))
                    {
                        //Check now if one of the modelmeshparts' bounding boxes intersected:
                        foreach (ModelMeshPart part1 in mesh1.MeshParts)
                        {
                            if (!(part1.Tag is object[] || (part1.Tag as object[]).Length >= 2 || (part1.Tag as object[])[0] is List<Triangle> || (part1.Tag as object[])[1] is List<BoundingBox>))
                                throw new Exception("Call the Collision Detection Helper's Extract Model Data, ConstructMeshPartBoundingBoxes on arg1 load");

                            foreach (ModelMeshPart part2 in mesh2.MeshParts)
                            {
                                if (!(part2.Tag is object[] || (part2.Tag as object[]).Length >= 2 || (part2.Tag as object[])[0] is List<Triangle> || (part2.Tag as object[])[1] is List<BoundingBox>))
                                    throw new Exception("Call the Collision Detection Helper's Extract Model Data, ConstructMeshPartBoundingBoxes on arg2 load");
                                //now check each of the bounding boxes in the list of boxes for this part against the bounding boxes in the other part's list
                                foreach (BoundingBox aPart1Box in ((part1.Tag as object[])[1] as List<BoundingBox>))
                                    foreach (BoundingBox aPart2Box in ((part2.Tag as object[])[1] as List<BoundingBox>))
                                        if (TransformBox(aPart1Box, object1Transformation).Intersects(
                                            TransformBox(aPart2Box, object2Transformation)))
                                            return true;
                            } //foreach part in mesh of model 2
                        } //foreach part in mesh of model 1
                    } //if meshes intersects
                } //foreach mesh in model 2
            } //foreach mesh in model 1
            return false;
        }
    }
}

