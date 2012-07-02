using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Collision_Detection
{
    public static class CollisionDetectionHelper
    {
        /// <summary>
        /// Modify this const to set the number of bounding spheres per triangle. This may speed up
        /// collision detection, but it can slow down per triangle collision detection if number is too large.
        /// For main game this number should be larger, since collisions are not detected per triangle there.
        /// </summary>
        public const int NUM_TRIANGLES_PER_BOX = 3;
        /// <summary>
        /// Each model part will have several datastrutures associated with it so we need an array of objects to store them all
        /// </summary>
        /// <param name="part">Model mesh part under consideration</param>
        private static void ConstructCollisionDetectionInfoStore(ModelMeshPart part)
        {
            part.Tag = new object[2];
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
                    for (int i = 0;;)
                    {
                        List<Vector3> pointList = new List<Vector3>();
                        if (i + NUM_TRIANGLES_PER_BOX < currentList.Count)
                            for (int j = 0; j < NUM_TRIANGLES_PER_BOX; j++)
                            {
                                Triangle currentTriangle = currentList.ElementAt(i + j);
                                pointList.Add(currentTriangle.v1);
                                pointList.Add(currentTriangle.v2);
                                pointList.Add(currentTriangle.v3);
                            }
                        else
                            for (int j = i; j < currentList.Count; j++)
                            {
                                Triangle currentTriangle = currentList.ElementAt(j);
                                pointList.Add(currentTriangle.v1);
                                pointList.Add(currentTriangle.v2);
                                pointList.Add(currentTriangle.v3);
                            }
                        results.Add(BoundingBox.CreateFromPoints(pointList));
                        if (++i == currentList.Count)
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
    }
}
