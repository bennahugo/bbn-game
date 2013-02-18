using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game.Graphics
{
    class SunDrawer
    {
        BasicEffect e;
       // Graphics.Shapes.QuadDrawer sun;
        Graphics.Shapes.QuadDrawer glare;
        Texture2D tBack;
        Texture2D tFlare;
        public SunDrawer(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,Game game)
        {
            e = new BasicEffect(game.GraphicsDevice, null);
            tBack = game.Content.Load<Texture2D>("sun/sun");
            tFlare = game.Content.Load<Texture2D>("sun/sunglare");
            //sun = new Graphics.Shapes.QuadDrawer(p1 + new Vector3(0.01f, 250, 250), p2 + new Vector3(0.01f, -250, 250), p3 + new Vector3(0.01f, 250, -250), p4 + new Vector3(0.01f, -250, -250),
              //  1, tBack, game.GraphicsDevice);
            glare = new Graphics.Shapes.QuadDrawer(p1 + new Vector3(0.02f, 0, 0), p2 + new Vector3(0.02f, 0, 0), p3 + new Vector3(0.02f, 0, 0), p4 + new Vector3(0.02f, 0, 0), 
                1, tFlare, game.GraphicsDevice);
            e.EmissiveColor = new Vector3(1, 1, 1);
            e.DiffuseColor = new Vector3(1, 1, 1);
            e.SpecularColor = new Vector3(1, 1, 1);
            e.SpecularPower = 2f;
        }
        public void draw(Matrix world, Camera.CameraMatrices cam, GraphicsDevice g)
        {
            e.AmbientLightColor = new Vector3(1f, 1f, 1f);
            //sun.Draw(cam.View, world, cam.Projection, e);
            BlendFunction prev = g.RenderState.BlendFunction;
            g.RenderState.BlendFunction = BlendFunction.Add;
            e.AmbientLightColor = new Vector3(1f, 0.8f, 0.5f);
            e.DiffuseColor = new Vector3(1f, 0.8f, 0.5f);
            //e.SpecularColor = new Vector3(1f, 0.8f, 0.5f);
            glare.Draw(cam.View, world, cam.Projection, e);
            g.RenderState.BlendFunction = prev;
           
        }
    }
}
