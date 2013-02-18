#region File Description
//-----------------------------------------------------------------------------
// ExplosionParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace BBN_Game.ParticleEngine
{
    /// <summary>
    /// Custom particle system for creating the fiery part of the explosions.
    /// </summary>
    class ExplosionParticleSystem : ParticleSystem
    {
        int maxParticles;
        int minEndSize;
        int maxEndSize;
        double particleDuration;

        public ExplosionParticleSystem(Game game, ContentManager content,int max, double duration,int minSize,int maxSize)
            : base(game, content)
        {
            maxParticles = max;
            maxEndSize = maxSize;
            minEndSize = minSize;
            particleDuration = duration;
        }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "explosion";

            settings.MaxParticles = maxParticles;

            settings.Duration = TimeSpan.FromSeconds(particleDuration);
            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = 20;
            settings.MaxHorizontalVelocity = 30;

            settings.MinVerticalVelocity = -20;
            settings.MaxVerticalVelocity = 20;

            settings.EndVelocity = 0;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.MinRotateSpeed = -1;
            settings.MaxRotateSpeed = 1;

            settings.MinStartSize = 10;
            settings.MaxStartSize = 10;

            settings.MinEndSize = minEndSize;
            settings.MaxEndSize = maxEndSize;

            // Use additive blending.
            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;
        }
    }
}
