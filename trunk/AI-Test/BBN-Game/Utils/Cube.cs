using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

///
/// This creates a sphere 
/// 
/// Referance : spaceShooter sample off MSN Microsoft 
///             http://blogs.msdn.com/b/dawate/archive/2011/01/20/constructing-drawing-and-texturing-a-cube-with-vertices-in-xna-on-windows-phone-7.aspx
///
/// @Author : Brandon James Talbot
///

namespace BBN_Game.Graphics.Shapes
{
    class Cube : IDisposable
    {
        vertexPos[] vertices;
        short[][] indicis;

        int numV;

        VertexDeclaration vDecl;
        VertexBuffer vBuffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">widthOfTheCube</param>
        /// <param name="hozFaces">Number of horizontal faces</param>
        /// <param name="vertFaces">Number of verticle faces</param>
        public Cube(float size, int Repeat)
        {
            CreateCube(size, Repeat);
        }

        #region "Disposal methods"
        protected virtual void Dispose(bool all)
        {
            if (vDecl != null)
            {
                vDecl.Dispose();
                vDecl = null;
            }

            if (vBuffer != null)
            {
                vBuffer.Dispose();
                vBuffer = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// Creates all required variables at load time
        /// </summary>
        /// <param name="game"></param>
        public void LoadContent(GraphicsDevice gd)
        {
            vBuffer = new VertexBuffer(gd, typeof(vertexPos), numV, BufferUsage.WriteOnly);

            VertexElement[] elements = new VertexElement[2];
            elements[0] = new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0);
            elements[1] = new VertexElement(0, 12, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0);

            vDecl = new VertexDeclaration(gd, elements);

            vBuffer.SetData<vertexPos>(vertices);
        }

        #region "Creation"
        ///// <summary>
        ///// Scales the spheres texture in horizontal and verticle positions
        ///// This tells it how many times teh texture should try paste the image onto it
        ///// </summary>
        ///// <param name="uScale">horiz repetitions</param>
        ///// <param name="vScale">Verticle repetitions</param>
        //public void TileUVs(int uScale, int vScale)
        //{
        //    for (int i = 0; i < numV; i++)
        //    {
        //        vertices[i].TextureCoordinate.X *= uScale;
        //        vertices[i].TextureCoordinate.Y *= vScale;
        //    }
        //}

        /// <summary>
        /// Creates the sphere indicis
        /// </summary>
        /// <param name="radius">radius of the sphere</param>
        /// <param name="hFaces">number of horizontal faces</param>
        /// <param name="vFaces">number of verticle faces</param>
        void CreateCube(float size, int Repeat)
        {
            numV = 36;
            vertices = new vertexPos[numV];

            // set working variables
            // Calculate the position of the vertices on the top face.
            Vector3 topLeftFront = new Vector3(-1.0f, 1.0f, -1.0f) * size;
            Vector3 topLeftBack = new Vector3(-1.0f, 1.0f, 1.0f) * size;
            Vector3 topRightFront = new Vector3(1.0f, 1.0f, -1.0f) * size;
            Vector3 topRightBack = new Vector3(1.0f, 1.0f, 1.0f) * size;

            // Calculate the position of the vertices on the bottom face.
            Vector3 btmLeftFront = new Vector3(-1.0f, -1.0f, -1.0f) * size;
            Vector3 btmLeftBack = new Vector3(-1.0f, -1.0f, 1.0f) * size;
            Vector3 btmRightFront = new Vector3(1.0f, -1.0f, -1.0f) * size;
            Vector3 btmRightBack = new Vector3(1.0f, -1.0f, 1.0f) * size;

            Vector2 textureTopLeft = new Vector2(0, 1);
            Vector2 textureBottomLeft = new Vector2(0, 0);
            Vector2 textureTopRight = new Vector2(1, 1);
            Vector2 textureBottomRight = new Vector2(1, 0);


            vertices[0] = new vertexPos(topLeftFront, textureTopLeft * Repeat);
            vertices[1] = new vertexPos(btmLeftFront, textureBottomLeft * Repeat);
            vertices[2] = new vertexPos(topRightFront, textureTopRight * Repeat);
            vertices[3] = new vertexPos(btmLeftFront, textureBottomLeft * Repeat);
            vertices[4] = new vertexPos(btmRightFront, textureBottomRight * Repeat);
            vertices[5] = new vertexPos(topRightFront, textureTopRight * Repeat);

            // Add the vertices for the BACK face.
            vertices[6] = new vertexPos(topLeftBack, textureTopRight * Repeat);
            vertices[7] = new vertexPos(topRightBack, textureTopLeft * Repeat);
            vertices[8] = new vertexPos(btmLeftBack, textureBottomRight * Repeat);
            vertices[9] = new vertexPos(btmLeftBack, textureBottomRight * Repeat);
            vertices[10] = new vertexPos(topRightBack, textureTopLeft * Repeat);
            vertices[11] = new vertexPos(btmRightBack, textureBottomLeft * Repeat);

            // Add the vertices for the TOP face.
            vertices[12] = new vertexPos(topLeftFront, textureBottomLeft * Repeat);
            vertices[13] = new vertexPos(topRightBack, textureTopRight * Repeat);
            vertices[14] = new vertexPos(topLeftBack, textureTopLeft * Repeat);
            vertices[15] = new vertexPos(topLeftFront, textureBottomLeft * Repeat);
            vertices[16] = new vertexPos(topRightFront, textureBottomRight * Repeat);
            vertices[17] = new vertexPos(topRightBack, textureTopRight * Repeat);

            // Add the vertices for the BOTTOM face. 
            vertices[18] = new vertexPos(btmLeftFront, textureTopLeft * Repeat);
            vertices[19] = new vertexPos(btmLeftBack, textureBottomLeft * Repeat);
            vertices[20] = new vertexPos(btmRightBack, textureBottomRight * Repeat);
            vertices[21] = new vertexPos(btmLeftFront, textureTopLeft * Repeat);
            vertices[22] = new vertexPos(btmRightBack, textureBottomRight * Repeat);
            vertices[23] = new vertexPos(btmRightFront, textureTopRight * Repeat);

            // Add the vertices for the LEFT face.
            vertices[24] = new vertexPos(topLeftFront, textureTopRight * Repeat);
            vertices[25] = new vertexPos(btmLeftBack, textureBottomLeft * Repeat);
            vertices[26] = new vertexPos(btmLeftFront, textureBottomRight * Repeat);
            vertices[27] = new vertexPos(topLeftBack, textureTopLeft * Repeat);
            vertices[28] = new vertexPos(btmLeftBack, textureBottomLeft * Repeat);
            vertices[29] = new vertexPos(topLeftFront, textureTopRight * Repeat);

            // Add the vertices for the RIGHT face. 
            vertices[30] = new vertexPos(topRightFront, textureTopLeft * Repeat);
            vertices[31] = new vertexPos(btmRightFront, textureBottomLeft * Repeat);
            vertices[32] = new vertexPos(btmRightBack, textureBottomRight * Repeat);
            vertices[33] = new vertexPos(topRightBack, textureTopRight * Repeat);
            vertices[34] = new vertexPos(topRightFront, textureTopLeft * Repeat);
            vertices[35] = new vertexPos(btmRightBack, textureBottomRight * Repeat); 
        }
        #endregion

        /// <summary>
        /// Draws the Sphere
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="camera">What camera to draw on</param>
        public void draw(GraphicsDevice device)
        {
            device.VertexDeclaration = vDecl;
            device.Vertices[0].SetSource(vBuffer, 0, vertexPos.SizeInBytes);

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, numV / 2);
        }

    }
}
