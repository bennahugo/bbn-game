using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

/////
///
/// Author - Benjamin Hugo
/// 
/// Draws a quad at a specified location with specified texture
///
/////
namespace BBN_Game.Graphics.Shapes
{
        class QuadDrawer
        {
            /// <summary>
            /// Global Variables
            /// </summary>
            public VertexPositionNormalTexture[] Vertices;
            public int[] Indices { get; protected set; }

            private Texture2D texture;
            private VertexDeclaration quadVertexDecl;
            private GraphicsDevice dev;


            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="topLeft">Top Left Co-ord for quad</param>
            /// <param name="topRight">Top right co-ord for quad</param>
            /// <param name="bottomLeft">Bottom left co-ord for quad</param>
            /// <param name="bottomRight">Bottom right co-ord for quad</param>
            /// <param name="TextureRepeat">The texture reapeat value</param>
            /// <param name="tex">The texture</param>
            /// <param name="dev">The graphics device</param>
            public QuadDrawer(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, float TextureRepeat,
                Texture2D tex, GraphicsDevice dev)
            {

                Vertices = new VertexPositionNormalTexture[4];
                Vertices[0].Position = bottomLeft; Vertices[0].TextureCoordinate = new Vector2(0.0f * TextureRepeat, 1.0f * TextureRepeat);
                Vertices[1].Position = topLeft; Vertices[1].TextureCoordinate = new Vector2(0.0f * TextureRepeat, 0.0f * TextureRepeat);
                Vertices[2].Position = bottomRight; Vertices[2].TextureCoordinate = new Vector2(1.0f * TextureRepeat, 1.0f * TextureRepeat);
                Vertices[3].Position = topRight; Vertices[3].TextureCoordinate = new Vector2(1.0f * TextureRepeat, 0.0f * TextureRepeat);

                //clockwise winding so that the normal can point outward:
                Indices = new int[6];
                Indices[0] = 0;
                Indices[1] = 1;
                Indices[2] = 2;
                Indices[3] = 2;
                Indices[4] = 1;
                Indices[5] = 3;

                texture = tex;


                quadVertexDecl = new VertexDeclaration(dev,
                    VertexPositionNormalTexture.VertexElements);
                this.dev = dev;
            }

            /// <summary>
            /// Draws the Quad
            /// </summary>
            /// <param name="View">The view Matrix</param>
            /// <param name="World">The World matrix</param>
            /// <param name="Projection">The projection Matrix</param>
            /// <param name="effectSetup">A Basic Effect to draw with</param>
            public void Draw(Matrix View, Matrix World, Matrix Projection, BasicEffect effectSetup)
            {
                effectSetup.Texture = texture;
                effectSetup.View = View;
                effectSetup.World = World;
                effectSetup.Projection = Projection;
                effectSetup.TextureEnabled = true;
                effectSetup.VertexColorEnabled = false;
                dev.VertexDeclaration = this.quadVertexDecl;

                effectSetup.Begin();
                foreach (EffectPass pass in effectSetup.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    dev.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(
                        PrimitiveType.TriangleList, this.Vertices, 0, 4, this.Indices, 0, 2);

                    pass.End();
                }
                effectSetup.End();
                effectSetup.TextureEnabled = false;
                effectSetup.VertexColorEnabled = true;
            }
        }
    }


