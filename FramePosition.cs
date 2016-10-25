using RigourTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RigourTech
{
    /// <summary>
    /// 具有坐标的帧
    /// </summary>
    public class FramePosition
    {
        public FramePosition()
        {

        }

        public FramePosition(Point3D p,string f)
        {
            position = p;
            frameNumber =long.Parse(f);
        }


        long frameNumber;
        Point3D position;
        double radius;
        int direction;

        public long FrameNumber
        {
            get
            {
                return frameNumber;
            }

            set
            {
                frameNumber = value;
            }
        }

        public Point3D Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }

            set
            {
                radius = value;
            }
        }

        public int Direction
        {
            get
            {
                return direction;
            }

            set
            {
                direction = value;
            }
        }

        public static FramePosition Parse(string frame,string strp3d)
        {
            Point3D p = Point3D.Prase(strp3d);
            if (p != null)
            {
                return new FramePosition(p, frame);
            }
            return null;
        }

        public override string ToString()
        {
            return FrameNumber + "," + Position.ToString();
        }
    }
}
