using System;
using System.Drawing;

namespace ImageProcessing
{
    public struct FloatColor
    {
        public FloatColor(float a, float r, float g, float b)
        {
            _data = new float[4];
            A = a;
            R = r;
            G = g;
            B = b;
        }
        public FloatColor(FloatColor fc)
        {
            _data = new float[4];
            A = fc.A;
            R = fc.R;
            G = fc.G;
            B = fc.B;
        }
        public FloatColor(Color ic)
        {
            _data = new float[4];
            IntColor = ic;
        }
        public void Normalize()
        {
            A = Math.Max(Math.Min(1, A), 0);
            R = Math.Max(Math.Min(1, R), 0);
            G = Math.Max(Math.Min(1, G), 0);
            B = Math.Max(Math.Min(1, B), 0);
        }
        public FloatColor Lerp(FloatColor fcT, float k = 0.5f)
        {
            return this*(1 - k) + fcT*k;
        }

        public override int GetHashCode()
        {
            var a = (int)(A * 255.0f);
            var r = (int)(R * 255.0f);
            var g = (int)(G * 255.0f);
            var b = (int)(B * 255.0f);
            return (a << 24) | (r << 16) | (g << 8) | (b);
        }
        public override bool Equals(object obj)
        {
            // ReSharper disable once BaseObjectEqualsIsObjectEquals
            if (base.Equals(obj))
                return true;
            return obj is FloatColor && GetHashCode() == obj.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format("#{0:X8}", GetHashCode());
        }
        public static bool operator ==(FloatColor a, FloatColor b)
        {
            if ((object)a == null)
                return (object)b == null;
            return a.Equals(b);
        }
        public static bool operator !=(FloatColor a, FloatColor b)
        {
            return !(a == b);
        }

        public static FloatColor operator +(FloatColor a, FloatColor b)
        {
            return new FloatColor(a.A + b.A, a.R + b.R, a.G + b.G, a.B + b.B);
        }
        public static FloatColor operator -(FloatColor a, FloatColor b)
        {
            return new FloatColor(a.A - b.A, a.R - b.R, a.G - b.G, a.B - b.B);
        }
        public static FloatColor operator *(FloatColor a, FloatColor b)
        {
            return new FloatColor(a.A*b.A, a.R*b.R, a.G*b.G, a.B*b.B);
        }
        public static FloatColor operator *(FloatColor a, float b)
        {
            return new FloatColor(a.A*b, a.R*b, a.G*b, a.B*b);
        }

        public float A
        {
            get { return _data[0]; }
            set { _data[0] = value; }
        }
        public float R
        {
            get { return _data[1]; }
            set { _data[1] = value; }
        }
        public float G
        {
            get { return _data[2]; }
            set { _data[2] = value; }
        }
        public float B
        {
            get { return _data[3]; }
            set { _data[3] = value; }
        }
        public Color IntColor
        {
            get
            {
                var a = (int)(A * 255.0f);
                var r = (int)(R * 255.0f);
                var g = (int)(G * 255.0f);
                var b = (int)(B * 255.0f);
                return Color.FromArgb(a, r, g, b);
            }
            set
            {
                A = value.A / 255.0f;
                R = value.R / 255.0f;
                G = value.G / 255.0f;
                B = value.B / 255.0f;
            }
        }

        private float[] _data;
    }
}
