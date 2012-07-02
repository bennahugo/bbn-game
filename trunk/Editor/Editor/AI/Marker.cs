using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBN_Game.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
namespace BBN_Game.AI
{
    /// <summary>
    /// Basic map marker. Markers should not be visible on the map during game play and serve only as
    /// holders of position data.
    /// </summary>
    class Marker:StaticObject
    {
        /// <summary>
        /// The model should only be displayed during map editing
        /// </summary>
        private const String MODEL_USED_FOR_MAP_EDITOR = "Models/marker";
        /// <summary>
        /// Default attributes have to be set as a marker is not part of the general toolbox object set
        /// </summary>
        private void SetDefaultAttributes()
        {
            base.addAttribute("x", Convert.ToString(base.Position.X));
            base.addAttribute("y", Convert.ToString(base.Position.Y));
            base.addAttribute("z", Convert.ToString(base.Position.Z));
            base.addAttribute("yaw", Convert.ToString(base.yaw));
            base.addAttribute("pitch", Convert.ToString(base.pitch));
            base.addAttribute("roll", Convert.ToString(base.roll));
            base.addAttribute("scaleX", Convert.ToString(base.Scale.X));
            base.addAttribute("scaleY", Convert.ToString(base.Scale.Y));
            base.addAttribute("scaleZ", Convert.ToString(base.Scale.Z));
            base.addAttribute("modelName", MODEL_USED_FOR_MAP_EDITOR);
            base.addAttribute("visible",Convert.ToString(base.visible));
        }
        /// <summary>
        /// Construtors to accomodate the fact that we are inheriting from the Static Object datastructures
        /// </summary>
        public Marker() :
            base()
        { SetDefaultAttributes();}
        public Marker(Vector3 pos, Model m):
            base(pos,m)
        { SetDefaultAttributes(); }
        public Marker(Vector3 pos, ContentManager m) :
            base(pos, m)
        {
            modelName = MODEL_USED_FOR_MAP_EDITOR;
            SetDefaultAttributes(); 
            loadModel(); 
        }
    }
}
