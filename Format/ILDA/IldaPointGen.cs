using System;

using System.Linq;
using System.Windows.Media.Media3D;

namespace ToGeometryConverter.Format.ILDA
{
    class IldaPointGen
    {
        public float width;
        public float height;
        public float angle;
        public float xord;
        public float yord;
        public bool red;
        public bool green;
        public bool blue;

        public IldaPointGen (float Width, float Height, float Angle, bool Red, bool Green, bool Blue)
        {
            this.width = Width;
            this.height = Height;
            this.angle = Angle;
            this.red = Red;
            this.green = Green;
            this.blue = Blue;
        }

        public IldaPointGen(float Width, float Height, float Angle, float Xord, float Yord, bool Red, bool Green, bool Blue)
        {
            this.width = Width;
            this.height = Height;
            this.angle = Angle;
            this.xord = Xord;
            this.yord = Yord;
            this.red = Red;
            this.green = Green;
            this.blue = Blue;
        }
    }
}
