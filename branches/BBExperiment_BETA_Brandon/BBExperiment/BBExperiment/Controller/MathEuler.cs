using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace BBN_Game
{
    class MathEuler
    {
        //In a 2D grid, returns the angle to a specified point from the +X axis
        public static float ArcTanAngle(float X, float Y)
        {
            if (X == 0)
            {
                if (Y == 1)
                    return (float)MathHelper.PiOver2;
                else
                    return (float)-MathHelper.PiOver2;
            }
            else if (X > 0)
                return (float)Math.Atan(Y / X);
            else if (X < 0)
            {
                if (Y > 0)
                    return (float)Math.Atan(Y / X) + MathHelper.Pi;
                else
                    return (float)Math.Atan(Y / X) - MathHelper.Pi;
            }
            else
                return 0;
        }

        //returns Euler angles that point from one point to another
        public static Vector3 AngleTo(Vector3 target, Vector3 Position)
        {
            Vector3 angle = new Vector3();
            Vector3 v3 = Vector3.Normalize(Position - target);
            angle.X = (float)Math.Asin(v3.Y);
            angle.Y = ArcTanAngle(-v3.Z, -v3.X);
            return angle;
        }

        //converts a Quaternion to Euler angles (X = pitch, Y = yaw, Z = roll)
        public static Vector3 QuaternionToEuler(Quaternion rotation)
        {
            Vector3 rotationaxes = new Vector3();

            Vector3 forward = Vector3.Transform(Vector3.Forward, rotation);
            Vector3 up = Vector3.Transform(Vector3.Up, rotation);
            rotationaxes = AngleTo(new Vector3(), forward);
            if (rotationaxes.X == MathHelper.PiOver2)
            {
                rotationaxes.Y = ArcTanAngle(up.Z, up.X);
                rotationaxes.Z = 0;
            }
            else if (rotationaxes.X == -MathHelper.PiOver2)
            {
                rotationaxes.Y = ArcTanAngle(-up.Z, -up.X);
                rotationaxes.Z = 0;
            }
            else
            {
                up = Vector3.Transform(up, Matrix.CreateRotationY(-rotationaxes.Y));
                up = Vector3.Transform(up, Matrix.CreateRotationX(-rotationaxes.X));
                rotationaxes.Z = ArcTanAngle(up.Y, -up.X);
            }
            return rotationaxes;
        }

        // Y P R output
        public static Vector3 QuaternionToEuler2(Quaternion q)
        {
            Vector3 v = Vector3.Zero;

            v.X = (float)Math.Atan2
            (
                2 * q.Y * q.W - 2 * q.X * q.Z,
                   1 - 2 * Math.Pow(q.Y, 2) - 2 * Math.Pow(q.Z, 2)
            );

            v.Z = (float)Math.Asin
            (
                2 * q.X * q.Y + 2 * q.Z * q.W
            );

            v.Y = (float)Math.Atan2
            (
                2 * q.X * q.W - 2 * q.Y * q.Z,
                1 - 2 * Math.Pow(q.X, 2) - 2 * Math.Pow(q.Z, 2)
            );

            if (q.X * q.Y + q.Z * q.W == 0.5)
            {
                v.X = (float)(2 * Math.Atan2(q.X, q.W));
                v.Y = 0;
            }

            else if (q.X * q.Y + q.Z * q.W == -0.5)
            {
                v.X = (float)(-2 * Math.Atan2(q.X, q.W));
                v.Y = 0;
            }

            return v;
        }  
    }
}
