using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices; //for messageboxes
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Audio;

namespace BBN_Game.ParticleEngine
{
    class ParticleController
    {
        #region Instance Variables

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //different particle systems
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem projectileTrailParticles;
        ParticleSystem smokePlumeParticles;
        ParticleSystem fireParticles;

        //keep record of all active particles
        List<Projectile> projectiles = new List<Projectile>();

        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        //random number for fire effect
        Random random = new Random();

        #endregion

        #region Initialize
        public ParticleController(BBN_Game.BBNGame g)
        {
            graphics = g.Graphics;
            spriteBatch = new SpriteBatch(g.GraphicsDevice);

            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;

            //construct particle engine components
            explosionParticles = new ExplosionParticleSystem(g, g.Content);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(g, g.Content);
            projectileTrailParticles = new ProjectileTrailParticleSystem(g, g.Content);
            smokePlumeParticles = new SmokePlumeParticleSystem(g, g.Content);
            fireParticles = new FireParticleSystem(g, g.Content);

            //set draw order so explosions& fire will appear above smoke
            smokePlumeParticles.DrawOrder = 100;
            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionSmokeParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;

            //register the particle system components
            g.Components.Add(explosionParticles);
            g.Components.Add(explosionSmokeParticles);
            g.Components.Add(projectileTrailParticles);
            g.Components.Add(smokePlumeParticles);
            g.Components.Add(fireParticles);
            
        }

        public void LoadContent()
        {

        }

        #endregion

        #region Update

        //TODO
        public void Update()
        {


        }

        //updating explosion effects
        public void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                //create new projectile once per second
                projectiles.Add(new Projectile(explosionParticles,
                                                explosionSmokeParticles,
                                                projectileTrailParticles));
                timeToNextProjectile += TimeSpan.FromSeconds(1);
            }
        }

        //updates the list of active projectiles
        public void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                    projectiles.RemoveAt(i);//remove projectiles at end of their life
                else
                    i++;//advance to next projectile
            }
        }

        //update smoke plue effect
        public void UpdateSmokePlume()
        {
            //create one new smoke particle per frame
            smokePlumeParticles.AddParticle(Vector3.Zero, Vector3.Zero);
        }

        //update fire effect
        public void UpdateFire()
        {
            const int fireParticlePerFrame = 20;

            //create a number of fire particles, randomly around circle
            for (int i = 0; i < fireParticlePerFrame; i++)
                fireParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);

            //create one smoke particle per frame
            smokePlumeParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
        }

        //chooses random location around circle at which a fire particlw will be created
        Vector3 RandomPointOnCircle()
        {
            const float radius = 30;
            const float height = 40;

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }
        #endregion

        #region Draw

        public void Draw(Matrix view, Matrix projection)
        {
            explosionParticles.SetCamera(view, projection);
            explosionSmokeParticles.SetCamera(view, projection);
            projectileTrailParticles.SetCamera(view, projection);
            smokePlumeParticles.SetCamera(view, projection);
            fireParticles.SetCamera(view, projection);
        }

        #endregion

    }
}
