using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Graphics.Shapes
{
    struct vertexPos
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;

        public vertexPos(Vector3 pos, Vector2 texPos)
        {
            Position = pos;
            TextureCoordinate = texPos;
        }

        public static int SizeInBytes
        {
            get
            {
                return (sizeof(float) * 3) + (sizeof(float) * 2);
            }
        }
    }
}
