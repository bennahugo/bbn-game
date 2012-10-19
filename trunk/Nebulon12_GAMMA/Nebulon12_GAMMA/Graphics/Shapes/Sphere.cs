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
///
/// @Author : Brandon James Talbot
///

namespace BBN_Game.Graphics.Shapes
{
    class Sphere : IDisposable
    {
        vertexPos[] vertices;
        short[][] indicis;

        int numV;
        int numI;
        int numB;

        VertexDeclaration vDecl;
        VertexBuffer vBuffer;
        IndexBuffer[] iBuffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">Radius of the sphere</param>
        /// <param name="hozFaces">Number of horizontal faces</param>
        /// <param name="vertFaces">Number of verticle faces</param>
        public Sphere (float size, int hozFaces, int vertFaces)
        {
            CreateSphere(size, hozFaces, vertFaces);
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
        public void LoadContent(Game game)
        {
            vBuffer = new VertexBuffer(game.GraphicsDevice, typeof(vertexPos), numV, BufferUsage.WriteOnly);
            iBuffer = new IndexBuffer[numB];
            for (int i = 0; i < numB; i++)
            {
                iBuffer[i] = new IndexBuffer(game.GraphicsDevice, typeof(short), numI, BufferUsage.WriteOnly);
            }

            VertexElement[] elements = new VertexElement[2];
            elements[0] = new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0);
            elements[1] = new VertexElement(0, 12, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0);

            vDecl = new VertexDeclaration(game.GraphicsDevice, elements);

            vBuffer.SetData<vertexPos>(vertices);

            for (int i = 0; i < numB; i++)
            {
                iBuffer[i].SetData<short>(indicis[i]);
            }
        }

        #region "Creation"
        /// <summary>
        /// Scales the spheres texture in horizontal and verticle positions
        /// This tells it how many times teh texture should try paste the image onto it
        /// </summary>
        /// <param name="uScale">horiz repetitions</param>
        /// <param name="vScale">Verticle repetitions</param>
        public void TileUVs(int uScale, int vScale)
        {
            for (int i = 0; i < numV; i++)
            {
                vertices[i].TextureCoordinate.X *= uScale;
                vertices[i].TextureCoordinate.Y *= vScale;
            }
        }

        /// <summary>
        /// Creates the sphere indicis
        /// </summary>
        /// <param name="radius">radius of the sphere</param>
        /// <param name="hFaces">number of horizontal faces</param>
        /// <param name="vFaces">number of verticle faces</param>
        void CreateSphere(float radius, int hFaces, int vFaces)
        {
            numV = hFaces * (vFaces + 1);
            vertices = new vertexPos[numV];
            numI = (vFaces + 1) * 2;
            numB = (hFaces - 1);
            indicis = new short[(hFaces - 1)][];

            int i;
            float PI = (float)Math.PI;
            for (i = 0; i < hFaces; i++)
            {
                float phi = ((float)i / (float)(hFaces - 1) - 0.5f) * (float)PI;
                for (int j = 0; j <= vFaces; j++)
                {
                    float theta = (float)j / (float)vFaces * (float)PI * 2;
                    int n = (i * (vFaces + 1)) + j;
                    float x = (float)(Math.Cos(phi) * Math.Cos(theta));
                    float y = (float)Math.Sin(phi);
                    float z = (float)(Math.Cos(phi) * Math.Sin(theta));

                    vertices[n].Position.X = x * radius;
                    vertices[n].Position.Y = y * radius;
                    vertices[n].Position.Z = z * radius;

                    vertices[n].TextureCoordinate.X = 1.0f - (float)j / (float)vFaces;
                    vertices[n].TextureCoordinate.Y = 1.0f - (float)i / (float)(hFaces - 1);
                }
            }

            for (i = 0; i < hFaces - 1; i++)
            {
                indicis[i] = new short[numI];
                for (int j = 0; j <= vFaces; j++)
                {
                    indicis[i][j * 2 + 0] = (short)(i * (vFaces + 1) + j);
                    indicis[i][j * 2 + 1] = (short)((i + 1) * (vFaces + 1) + j);
                }
            }
        }
        #endregion

        /// <summary>
        /// Draws the Sphere
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="camera">What camera to draw on</param>
        public void draw(GraphicsDevice device, Camera.CameraMatrices camera)
        {
            for (int i = 0; i < numB; i++)
            {
                device.VertexDeclaration = vDecl;
                device.Vertices[0].SetSource(vBuffer, 0, vertexPos.SizeInBytes);
                device.Indices = iBuffer[i];

                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, numV, 0, numI - 2);
            }
        }

    }
}
