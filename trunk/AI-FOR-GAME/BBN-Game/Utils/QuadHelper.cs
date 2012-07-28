
/*
 * REFERENCE: MSDN, http://msdn.microsoft.com/en-us/library/bb464051%28v=XNAGameStudio.10%29.aspx
 * Drawing a quad in XNA
 * (C) Benjamin Hugo, April 2012
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Utils
{
    class QuadHelper
    {
        public VertexPositionNormalTexture[] Vertices;
        public int[] Indices {get; protected set;}
        private QuadHelper() {/* DO NOT INVOKE*/}

        private Texture2D texture;
        private VertexDeclaration quadVertexDecl;
        private GraphicsDevice dev;

        public QuadHelper(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight, float TextureRepeat, 
            String texName,GraphicsDevice dev,Microsoft.Xna.Framework.Content.ContentManager content)
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

            texture = content.Load<Texture2D>(texName);
            

            quadVertexDecl = new VertexDeclaration(dev,
                VertexPositionNormalTexture.VertexElements);
            this.dev = dev;
        }
        public void Draw(Matrix View, Matrix World, Matrix Projection, BasicEffect effectSetup){
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
