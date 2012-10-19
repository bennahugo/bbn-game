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
        ParticleSystem smallExplosionParticles;
        ParticleSystem smallExplosionSmokeParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem projectileTrailParticles;
        ParticleSystem mediumMissileParticles;
        ParticleSystem fireParticles;

        //keep record of all active particles
        List<Projectile> projectiles = new List<Projectile>();

        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        //random number for fire effect
        Random random = new Random();

        #endregion

        #region Initialize
        public ParticleController(Game g)
        {
            if (g is BBN_Game.BBNGame)
            {
                graphics = ((BBN_Game.BBNGame)g).Graphics;
            }
            spriteBatch = new SpriteBatch(g.GraphicsDevice);

            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;

            //construct particle engine components
            explosionParticles = new ExplosionParticleSystem(g, g.Content,50,1,150,250);
            mediumMissileParticles = new ExplosionParticleSystem(g, g.Content, 40, 0.2, 20, 65);
            smallExplosionParticles = new ExplosionParticleSystem(g, g.Content,10,0.04,10,30);
            smallExplosionSmokeParticles = new ExplosionSmokeParticleSystem(g, g.Content,2,0.1,10,20);
            explosionSmokeParticles = new ExplosionSmokeParticleSystem(g, g.Content,200,1,100,200);
            projectileTrailParticles = new ProjectileTrailParticleSystem(g, g.Content);            
            fireParticles = new FireParticleSystem(g, g.Content);

            //set draw order so explosions& fire will appear above smoke            
            explosionSmokeParticles.DrawOrder = 100;
            smallExplosionSmokeParticles.DrawOrder = 120;
            projectileTrailParticles.DrawOrder = 200;
            explosionParticles.DrawOrder = 400;
            smallExplosionParticles.DrawOrder = 300;
            mediumMissileParticles.DrawOrder = 400;
            fireParticles.DrawOrder = 500;

            //register the particle system components
            g.Components.Add(explosionParticles);
            g.Components.Add(mediumMissileParticles);
            g.Components.Add(smallExplosionParticles);
            g.Components.Add(explosionSmokeParticles);
            g.Components.Add(smallExplosionSmokeParticles);
            g.Components.Add(projectileTrailParticles);            
            g.Components.Add(fireParticles);            
        }

        public void LoadContent()
        {

        }

        #endregion

        #region Update

        //for testing
        public void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                projectiles.Add(new Projectile(explosionParticles,
                                               explosionSmokeParticles,
                                               projectileTrailParticles, Vector3.Zero,Vector3.Zero, null));

                timeToNextProjectile += TimeSpan.FromSeconds(1);
            }
        }

        //updating missile explosion effects with smoke trial
        public void MissileFiredExplosions(Vector3 position, Vector3 velocity, BBN_Game.Objects.StaticObject parent)
        {
            //create new projectile every time a missile is fired
            projectiles.Add(new Projectile(explosionParticles,
                                            explosionSmokeParticles,
                                            projectileTrailParticles, position, velocity, parent));
        }

        public void ObjectDestroyedExplosion(Vector3 position, Vector3 velocity)
        {
            //explosion effect
            for (int i = 0; i < 35; i++)
                explosionParticles.AddParticle(position, velocity);

            //smoke for after
            for (int i = 0; i < 5; i++)
                explosionSmokeParticles.AddParticle(position, velocity);
        }

        public void mediumMissileExplosion(Vector3 position, Vector3 velocity)
        {
            //explosion effect
            for (int i = 0; i < 35; i++)
                mediumMissileParticles.AddParticle(position, velocity);

            //smoke for after
            for (int i = 0; i < 5; i++)
                smallExplosionSmokeParticles.AddParticle(position, velocity);
        }

        public void smallBulletExplosion(Vector3 position, Vector3 velocity)
        {
            //explosion effect
            for (int i = 0; i < 30; i++)
                smallExplosionParticles.AddParticle(position, velocity);

            //smoke for after
            for (int i = 0; i < 5; i++)
                smallExplosionSmokeParticles.AddParticle(position, velocity);
        }

        //updates the list of active projectiles
        public void UpdateProjectiles(GameTime gameTime, Vector3 pos, Vector3 vel, float dist, BBN_Game.Objects.StaticObject parent)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime,pos,vel,dist, parent))
                    projectiles.RemoveAt(i);//remove projectiles at end of their life
                else
                    i++;//advance to next projectile
            }
        }

        //update smoke plue effect
        public void UpdateSmokePlume()
        {
            //create one new smoke particle per frame
            //smokePlumeParticles.AddParticle(Vector3.Zero, Vector3.Zero);
        }

        //update fire effect
        public void UpdateFire()
        {
            const int fireParticlePerFrame = 20;

            //create a number of fire particles, randomly around circle
            for (int i = 0; i < fireParticlePerFrame; i++)
                fireParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);

            //create one smoke particle per frame
            //smokePlumeParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
        }

        //chooses random location around circle at which a fire particle will be created
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

        public void Draw(Matrix view, Matrix projection,Viewport vp, GameTime gt)
        {
            explosionParticles.SetCamera(view, projection,vp);
            smallExplosionParticles.SetCamera(view, projection, vp);
            explosionSmokeParticles.SetCamera(view, projection,vp);
            smallExplosionSmokeParticles.SetCamera(view, projection, vp);
            projectileTrailParticles.SetCamera(view, projection,vp);
            mediumMissileParticles.SetCamera(view, projection,vp);
            //fireParticles.SetCamera(view, projection,vp);

            explosionParticles.DrawParticles(gt, vp);
            smallExplosionParticles.DrawParticles(gt, vp);
            explosionSmokeParticles.DrawParticles(gt, vp);
            smallExplosionSmokeParticles.DrawParticles(gt, vp);
            projectileTrailParticles.DrawParticles(gt, vp);
            mediumMissileParticles.DrawParticles(gt, vp);
            //fireParticles.DrawParticles(gt, vp);
        }

        #endregion

    }
}
