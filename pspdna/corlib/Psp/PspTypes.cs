// Psp/PspTypes.cs
// Shared value types used across Gu, Gum, and Audio modules.
//
// [StructLayout] is not available without the BCL in DNA builds.
// The DNA runtime uses sequential layout for all structs by default,
// which matches the C layout as long as fields are declared in the
// same order as the C struct. No explicit attribute is needed.

namespace Psp
{
    // ScePspFVector3 — 3-component float vector.
    // Used by sceGumTranslate, sceGumScale, and similar GUM functions.
    public struct PspFVector3
    {
        public float X;
        public float Y;
        public float Z;

        public PspFVector3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        public static readonly PspFVector3 Zero     = new PspFVector3(0f, 0f,  0f);
        public static readonly PspFVector3 One      = new PspFVector3(1f, 1f,  1f);
        public static readonly PspFVector3 Forward  = new PspFVector3(0f, 0f, -1f);
        public static readonly PspFVector3 Up       = new PspFVector3(0f, 1f,  0f);
        public static readonly PspFVector3 Right    = new PspFVector3(1f, 0f,  0f);

        // Vector magnitude (length)
        public float Length()
        {
            return (float)System.Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        // Return normalised copy (unit vector)
        public PspFVector3 Normalised()
        {
            float len = Length();
            if (len < 0.000001f) return Zero;
            return new PspFVector3(X / len, Y / len, Z / len);
        }

        public static PspFVector3 operator+(PspFVector3 a, PspFVector3 b)
            => new PspFVector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static PspFVector3 operator-(PspFVector3 a, PspFVector3 b)
            => new PspFVector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static PspFVector3 operator*(PspFVector3 v, float s)
            => new PspFVector3(v.X * s, v.Y * s, v.Z * s);

        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }
    }

    // Vertex type for position-only (untextured, unlit) 3D drawing.
    // Matches GU_VERTEX_32BITF | GU_TRANSFORM_3D.
    public struct Vertex3D
    {
        public float X;
        public float Y;
        public float Z;

        public Vertex3D(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }
    }

    // Vertex type for textured, coloured 3D drawing.
    // Field order MUST match the GU vertex format flags used in DrawArray:
    //   GU_TEXTURE_32BITF | GU_COLOR_8888 | GU_VERTEX_32BITF | GU_TRANSFORM_3D
    // GU reads texture → colour → position in declaration order.
    public struct VertexTC
    {
        public float U;          // texture coordinate u
        public float V;          // texture coordinate v
        public uint  Color;      // ABGR packed colour (use Color.ToNative())
        public float X;
        public float Y;
        public float Z;

        public VertexTC(float u, float v, uint color, float x, float y, float z)
        {
            U = u; V = v; Color = color;
            X = x; Y = y; Z = z;
        }
    }

    // Vertex type for textured (no colour) 3D drawing.
    // Matches GU_TEXTURE_32BITF | GU_VERTEX_32BITF | GU_TRANSFORM_3D.
    public struct VertexT
    {
        public float U;
        public float V;
        public float X;
        public float Y;
        public float Z;

        public VertexT(float u, float v, float x, float y, float z)
        {
            U = u; V = v;
            X = x; Y = y; Z = z;
        }
    }

    // 2D sprite vertex — screen-space coordinates.
    // Matches GU_VERTEX_32BITF | GU_TRANSFORM_2D.
    public struct Vertex2D
    {
        public float X;
        public float Y;
        public float Z;  // typically 0 for 2D

        public Vertex2D(float x, float y)
        {
            X = x; Y = y; Z = 0f;
        }
    }
}
