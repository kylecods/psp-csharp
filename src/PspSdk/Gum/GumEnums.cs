namespace PspSdk.Gum;

/// <summary>
/// Matrix stack selector (GU_PROJECTION, GU_VIEW, GU_MODEL, GU_TEXTURE).
/// GUM maintains four independent matrix stacks; sceGumMatrixMode switches
/// which one subsequent push/pop/transform operations affect.
/// </summary>
public enum MatrixMode : int
{
    Projection = 0,
    View       = 1,
    Model      = 2,
    Texture    = 3,
}
