#region File Description
//-----------------------------------------------------------------------------
// Projectile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace BBN_Game.ParticleEngine
{
    /// <summary>
    /// This class demonstrates how to combine several different particle systems
    /// to build up a more sophisticated composite effect. It implements a rocket
    /// projectile, which arcs up into the sky using a ParticleEmitter to leave a
    /// steady stream of trail particles behind it. After a while it explodes,
    /// creating a sudden burst of explosion and smoke particles.
    /// </summary>
    class Projectile
    {
        #region Constants

        const float trailParticlesPerSecond = 20;
        const int numExplosionParticles = 15;
        const int numExplosionSmokeParticles = 5;
        const float projectileLifespan = 300f;
        const float sidewaysVelocityRange = 60;
        const float verticalVelocityRange = 40;
        const float gravity = 15;

        #endregion

        #region Fields

        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleEmitter trailEmitter;

        Vector3 position;
        Vector3 velocity;
        //float age;
        //float projectileLifespan = 0;

        static Random random = new Random();

        BBN_Game.Objects.StaticObject Parent;

        #endregion


        /// <summary>
        /// Constructs a new projectile.
        /// </summary>
        public Projectile(ParticleSystem explosionParticles,
                          ParticleSystem explosionSmokeParticles,
                          ParticleSystem projectileTrailParticles, Vector3 pos, Vector3 vel, BBN_Game.Objects.StaticObject parent)
        {
            this.explosionParticles = explosionParticles;
            this.explosionSmokeParticles = explosionSmokeParticles;
                        
            position = pos;
            velocity = vel;
            Parent = parent;

            //velocity.X = (float)(random.NextDouble() - 0.5) * sidewaysVelocityRange;
            //velocity.Y = (float)(random.NextDouble() + 0.5) * verticalVelocityRange;
            //velocity.Z = (float)(random.NextDouble() - 0.5) * sidewaysVelocityRange;

            // Use the particle emitter helper to output our trail particles.
            trailEmitter = new ParticleEmitter(projectileTrailParticles,
                                               trailParticlesPerSecond, position);
        }


        /// <summary>
        /// Updates the projectile.
        /// </summary>
        public bool Update(GameTime gameTime, Vector3 pos, Vector3 vel, float dist, BBN_Game.Objects.StaticObject parent)
        {
            if (parent.Equals(Parent))
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Simple projectile physics.
                //position += velocity * elapsedTime;
                //velocity.Y -= elapsedTime * gravity;
                //age += elapsedTime;

                position = pos;
                velocity = vel;

                // Update the particle emitter, which will create our particle trail.
                trailEmitter.Update(gameTime, position);

                // If enough time has passed, explode! Note how we pass our velocity
                // in to the AddParticle method: this lets the explosion be influenced
                // by the speed and direction of the projectile which created it.
                if (dist <= 0)   //age > projectileLifespan
                {
                //    for (int i = 0; i < numExplosionParticles; i++)
                //        explosionParticles.AddParticle(position, velocity);

                //    //for (int i = 0; i < numExplosionSmokeParticles; i++)
                //    //    explosionSmokeParticles.AddParticle(position, velocity);

                    return false;
                }
            }           
                
            return true;
        }
    } 
}
