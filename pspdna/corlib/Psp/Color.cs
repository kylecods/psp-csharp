// Psp/Color.cs
// PSP uses ABGR byte order (not the more common ARGB).
// ToNative() converts to the packed uint the hardware expects.
// All sceGu* colour parameters must be in ABGR format.

namespace Psp
{
    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b)
        {
            R = r; G = g; B = b; A = 255;
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r; G = g; B = b; A = a;
        }

        // Convenience constructors from 0–255 ints
        public Color(int r, int g, int b)
        {
            R = (byte)r; G = (byte)g; B = (byte)b; A = 255;
        }

        public Color(int r, int g, int b, int a)
        {
            R = (byte)r; G = (byte)g; B = (byte)b; A = (byte)a;
        }

        /// <summary>
        /// Convert to PSP-native ABGR packed uint.
        /// PSP memory layout: byte0=R, byte1=G, byte2=B, byte3=A.
        /// When viewed as a little-endian uint this is 0xAABBGGRR.
        /// </summary>
        public uint ToNative()
        {
            return (uint)((A << 24) | (B << 16) | (G << 8) | R);
        }

        // Predefined colours
        public static readonly Color Black   = new Color(0,   0,   0);
        public static readonly Color White   = new Color(255, 255, 255);
        public static readonly Color Red     = new Color(255, 0,   0);
        public static readonly Color Green   = new Color(0,   255, 0);
        public static readonly Color Blue    = new Color(0,   0,   255);
        public static readonly Color Yellow  = new Color(255, 255, 0);
        public static readonly Color Cyan    = new Color(0,   255, 255);
        public static readonly Color Magenta = new Color(255, 0,   255);

        public override string ToString()
        {
            return "Color(" + R + "," + G + "," + B + "," + A + ")";
        }
    }
}
