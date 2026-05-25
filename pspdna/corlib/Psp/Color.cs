namespace Psp
{
    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        /// <summary>Construct an opaque colour from RGB components (0-255 each).</summary>
        public Color(byte r, byte g, byte b)
        {
            R = r; G = g; B = b; A = 0xFF;
        }

        /// <summary>Construct a colour with explicit alpha (0=transparent, 255=opaque).</summary>
        public Color(byte r, byte g, byte b, byte a)
        {
            R = r; G = g; B = b; A = a;
        }

        // PSP colour encoding: ABGR (alpha in high byte, blue next, green, red in low byte)
        public uint ToNative()
        {
            return (uint)((A << 24) | (B << 16) | (G << 8) | R);
        }

        public static Color FromNative(uint value)
        {
            return FromRGBA(
                       (byte)((value >> 16) & 0xFF),
                       (byte)((value >> 8) & 0xFF),
                       (byte)(value & 0xFF),
                       (byte)((value >> 24) & 0xFF));
        }

        public static Color FromRGBA(byte r, byte g, byte b, byte a = 0xff)
        {
            return new Color
            {
                R = r,
                G = g,
                B = b,
                A = a
            };
        }

        // borrowed the definitions from here:
        // https://www.rapidtables.com/web/color/RGB_Color.html
        public static Color White { get; } = FromRGBA(255, 255, 255);
        public static Color Black { get; } = FromRGBA(0, 0, 0);
        public static Color Red { get; } = FromRGBA(255, 0, 0);
        public static Color Green { get; } = FromRGBA(0, 128, 0);
        public static Color Lime { get; } = FromRGBA(0, 255, 0);
        public static Color Blue { get; } = FromRGBA(0, 0, 255);
        public static Color Cyan { get; } = FromRGBA(0, 255, 255);
        public static Color Yellow { get; } = FromRGBA(255, 255, 0);
        public static Color Magenta { get; } = FromRGBA(255, 0, 255);
        public static Color Silver { get; } = FromRGBA(192, 192, 192);
        public static Color Gray { get; } = FromRGBA(128, 128, 128);
        public static Color Maroon { get; } = FromRGBA(128, 0, 0);
        public static Color Olive { get; } = FromRGBA(128, 128, 0);
        public static Color Purple { get; } = FromRGBA(128, 0, 128);
        public static Color Teal { get; } = FromRGBA(0, 128, 128);
        public static Color Navy { get; } = FromRGBA(0, 0, 128);
    }
}
