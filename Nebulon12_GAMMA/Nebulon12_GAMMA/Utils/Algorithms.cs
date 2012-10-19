using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Utils
{
    static class Algorithms
    {
        /// <summary>
        /// Method to draw a spritebatch 2d line, without primatives
        /// Source: http://www.xnawiki.com/index.php/Drawing_2D_lines_without_using_primitives
        /// </summary>
        /// <param name="width">width of line</param>
        /// <param name="color">color of line</param>
        /// <param name="point1">vertex 1 of ray</param>
        /// <param name="point2">vertex 2 of ray</param>
        /// <param name="spriteBatch">Sprite batch instance</param>
        /// <param name="blankTexture">Blank texture to use as a point</param>
        public static void Draw2DLine(float width, Microsoft.Xna.Framework.Graphics.Color color, Vector2 point1, Vector2 point2, SpriteBatch spriteBatch, Texture2D blankTexture)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            spriteBatch.Draw(blankTexture, point1, null, color,
                       angle, Vector2.Zero, new Vector2(length, width),
                       SpriteEffects.None, 0);
        }
        /// <summary>
        /// Method to draw a line in 3D
        /// </summary>
        /// <param name="color">Color of line</param>
        /// <param name="point1">first point</param>
        /// <param name="point2">second point</param>
        /// <param name="effect">Basic effects object</param>
        /// <param name="gfxDevice">Graphics device</param>
        /// <param name="projection">Projection matrix</param>
        /// <param name="view">view matrix</param>
        /// <param name="world">world matrix</param>
        public static void Draw3DLine(Microsoft.Xna.Framework.Graphics.Color color, Vector3 point1, Vector3 point2, BasicEffect effect, GraphicsDevice gfxDevice, Matrix projection, Matrix view, Matrix world)
        {
            VertexPositionColor[] vertexList = new VertexPositionColor[2];
            vertexList[0] = new VertexPositionColor(point1, color);
            vertexList[1] = new VertexPositionColor(point2, color);
            gfxDevice.VertexDeclaration = new VertexDeclaration(gfxDevice,VertexPositionColor.VertexElements);
            effect.Projection = projection;
            effect.View = view;
            effect.World = world;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gfxDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertexList, 0, 1);
                pass.End();
            }
            effect.End();
        }
        /// <summary>
        /// Method to unproject a point in 3 space into 2D pixel coordinates
        /// </summary>
        /// <param name="Point">Point in 3 space</param>
        /// <param name="gfxDevice">Handle to the graphics device instance in use</param>
        /// <param name="projection">Projection matrix</param>
        /// <param name="view">View matrix</param>
        /// <returns>2d coordinates (floor to get pixel coords)</returns>
        public static Vector2 unprojectPoint(Vector3 Point, GraphicsDevice gfxDevice, Matrix projection, Matrix view)
        {
            Vector3 temp = gfxDevice.Viewport.Project(Point, projection, view, Matrix.Identity);
            return new Vector2(temp.X, temp.Y);
        }
        /// <summary>
        /// Method to check if a user clicked near a 2d ray
        /// Reference: Wolfram Alpha
        /// </summary>
        /// <param name="x">Cursor x</param>
        /// <param name="y">Cursor y</param>
        /// <param name="rayStartPt">First point of Ray</param>
        /// <param name="rayEndPt">Second point of Ray</param>
        /// <returns></returns>
        public static bool clickedNearRay(int x, int y, Vector2 rayStartPt, Vector2 rayEndPt)
        {
            //Get the distance squared from a point to a line
            float rayLength = Vector2.Distance(rayStartPt, rayEndPt);
            float distFromStart = Vector2.Distance(rayStartPt, new Vector2(x, y));
            float distFromEnd = Vector2.Distance(rayEndPt, new Vector2(x, y));

            if (distFromStart < rayLength && distFromEnd < rayLength)
            {
                Vector3 x0 = new Vector3(x, y, 0);
                Vector3 x1 = new Vector3(rayStartPt, 0);
                Vector3 x2 = new Vector3(rayEndPt, 0);
                float d = Vector3.Cross((x2 - x1), (x1 - x0)).Length() / (x2 - x1).Length();
                if (d <= 4)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Gets the distance from a point to a line in 3 space
        /// Reference: Wolfram Alpha
        /// </summary>
        /// <param name="pt">Point in 3 space</param>
        /// <param name="line">Line in 3 space</param>
        /// <returns>minimum distance from point to the line</returns>
        public static float distanceFromPointToLine(Vector3 pt, Ray line)
        {
            //Let two points in space define a line:
            Vector3 x1 = line.Position;
            Vector3 x2 = line.Position + line.Direction;
            
            return Vector3.Cross((x2 - x1),(x1 - pt)).Length()/(x2-x1).Length();
        }
    }
}
