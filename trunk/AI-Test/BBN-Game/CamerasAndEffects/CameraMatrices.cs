﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;


/////
///
/// Author - Brandon James Talbot
/// 
/// This class contains Varibles to get data and check if a target is visible
////

namespace BBN_Game.Camera
{
    class CameraMatrices
    {
        /// <summary>
        /// Globals
        /// 
        /// View - A matrix that contains the view camera for the game
        /// Projection - A Matrix that contains the Projection for the game
        /// bf - The bounding frustum for the camera
        /// </summary>
        Matrix view;
        Matrix projection;
        BoundingFrustum bf;

        /// <summary>
        /// Getters and setters for the variables
        /// </summary>
        public Matrix View { get { return view; } set { view = value; } }
        public Matrix Projection { get { return projection; } set { projection = value; } }
        public BoundingFrustum getBoundingFrustum
        {
            get
            {
                bf.Matrix = view * projection;
                return bf;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="v">The view Matrix</param>
        /// <param name="p">The projection matrix</param>
        public CameraMatrices (Matrix v, Matrix p)
        {
            view = v;
            projection = p;

            bf = new BoundingFrustum(Matrix.Identity);
        }
    }
}
