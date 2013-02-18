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
    /// <summary>
    /// Chase camera class
    /// This class genereates the view matrix for the chase camera setting
    /// </summary>
    public class ChaseCamera
    {
        // global variables
        public Vector3 target; // target position
        public Vector3 targetDirection; // the direction the target is moving
        public Vector3 Up = Vector3.UnitY; // the up vector (in this case always up)
        public Vector3 offSet = new Vector3(0, 3.5f, -10f); // the offset we will be using
        public Vector3 targetPos; // the position the camera wants to go
        public Vector3 LookAtOffset = new Vector3(0, 2f, 0); // the lookat offset (slightly above ship for view purposes)
        public Vector3 lookAt; // lookat saved variable
        public float DistanceCoEff = 1000.0f; // distance coeff determines the distance you can move away from target (the camera)
        public float Spunge = 600.0f; // stop oscilation
        public float mass = 100.0f; // mass of the camera
        public Vector3 position; // position of camera
        public Vector3 speed; // speed vector
        public Matrix view; // view matrix to generate
        public Matrix proj; // projection matrix to generate
        public float viewingAnle = 45;

        public float rotateSpeed = 2f;

        public BoundingFrustum frustrum;

        public ChaseCamera(float width, float height)
        {
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(viewingAnle), width / height, 0.1f, 200000.0f);
            frustrum = new BoundingFrustum(Matrix.Identity);
        }

        /// <summary>
        /// Generates the matrix required for the chase cam
        /// </summary>
        private void makeMatrix()
        {
            view = Matrix.CreateLookAt(position, lookAt, Up);
            frustrum.Matrix = view * proj;
        }

        /// <summary>
        /// Update method takes in all requirements and generates the matrix
        /// </summary>
        /// <param name="gameTime">The current game time</param>
        /// <param name="tankPos">The position of the tank</param>
        /// <param name="Anlge">The angle of the tank</param>
        /// <param name="state">Keyboard state for keychecks</param>
        public void update(GameTime gameTime, Vector3 tankPos, Matrix rotation)
        {
            // make the vectors for the calculations
            target = tankPos;
            Matrix transform = rotation;

            targetPos = target +
                Vector3.TransformNormal(offSet, transform);
            lookAt = target +
                Vector3.TransformNormal(LookAtOffset, transform);

            // create a float value for elapsed time
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            this.Up = Vector3.Lerp(this.Up, rotation.Up, rotateSpeed * elapsed);

            //
            Vector3 distanceAwayFromTarget = position - targetPos;
            Vector3 force = -DistanceCoEff * distanceAwayFromTarget - Spunge * speed;
            Vector3 acceleration = force / mass;
            speed += acceleration * elapsed;
            position += speed * elapsed;

            makeMatrix();
        }
    }
}
