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


namespace BBN_Game.Camera
{
    class FirstPersonCam
    {
        public Matrix view; // view matrix to generate
        public Matrix proj; // projection matrix to generate
        public float viewingAnle = 60;
        public BoundingFrustum frustrum;

        public Vector3 Position = new Vector3(0, 0, 0);

        public FirstPersonCam(float width, float height)
        {
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(viewingAnle), width / height, 0.1f, 200000.0f);
            frustrum = new BoundingFrustum(Matrix.Identity);
        }

        public void update(GameTime gameTime, Vector3 position, Matrix rotation, float greatestLength)
        {
            greatestLength = (greatestLength/2) * 0.95f;

            Position = position + Vector3.Transform(new Vector3(0, 0, greatestLength), rotation);


            view = Matrix.CreateLookAt(Position, Position + Vector3.Transform(new Vector3(0, 0, 10), rotation), rotation.Up);
            frustrum.Matrix = view * proj;
        }
    }
}
