// samples/Cube3D/Program.cs
// Rotating textured cube using Psp.Gu (3D pipeline) and Psp.Gum (matrix math).
//
// This sample demonstrates the complete PSPDNA rendering pipeline:
//   1. Gu.Init()              — allocate VRAM buffers, enable display
//   2. Gum.Setup3D()          — set projection + identity view/model
//   3. Gum.MatrixMode(Model)  — select model matrix
//   4. Gum.RotateY(angle)     — rotate the cube each frame
//   5. Gu.DrawVertices(...)   — submit vertex array
//   6. Gu.EndFrame()          — flush, vsync, swap
//
// Controller:
//   D-pad Left/Right — rotate around Y axis
//   D-pad Up/Down    — rotate around X axis
//   Cross            — toggle auto-rotation
//   START            — exit

using System;
using Psp;

class Program
{
    // ── Cube geometry ─────────────────────────────────────────────────────
    // 6 faces × 2 triangles × 3 vertices = 36 vertices, position only.
    // Declared at class level so the GC does not relocate the array
    // between the 'fixed' pin and the native draw call.

    static readonly Vertex3D[] s_cube = new Vertex3D[]
    {
        // Front face (+Z)
        new Vertex3D(-1f, -1f,  1f), new Vertex3D( 1f, -1f,  1f), new Vertex3D( 1f,  1f,  1f),
        new Vertex3D(-1f, -1f,  1f), new Vertex3D( 1f,  1f,  1f), new Vertex3D(-1f,  1f,  1f),
        // Back face (-Z)
        new Vertex3D( 1f, -1f, -1f), new Vertex3D(-1f, -1f, -1f), new Vertex3D(-1f,  1f, -1f),
        new Vertex3D( 1f, -1f, -1f), new Vertex3D(-1f,  1f, -1f), new Vertex3D( 1f,  1f, -1f),
        // Left face (-X)
        new Vertex3D(-1f, -1f, -1f), new Vertex3D(-1f, -1f,  1f), new Vertex3D(-1f,  1f,  1f),
        new Vertex3D(-1f, -1f, -1f), new Vertex3D(-1f,  1f,  1f), new Vertex3D(-1f,  1f, -1f),
        // Right face (+X)
        new Vertex3D( 1f, -1f,  1f), new Vertex3D( 1f, -1f, -1f), new Vertex3D( 1f,  1f, -1f),
        new Vertex3D( 1f, -1f,  1f), new Vertex3D( 1f,  1f, -1f), new Vertex3D( 1f,  1f,  1f),
        // Top face (+Y)
        new Vertex3D(-1f,  1f,  1f), new Vertex3D( 1f,  1f,  1f), new Vertex3D( 1f,  1f, -1f),
        new Vertex3D(-1f,  1f,  1f), new Vertex3D( 1f,  1f, -1f), new Vertex3D(-1f,  1f, -1f),
        // Bottom face (-Y)
        new Vertex3D(-1f, -1f, -1f), new Vertex3D( 1f, -1f, -1f), new Vertex3D( 1f, -1f,  1f),
        new Vertex3D(-1f, -1f, -1f), new Vertex3D( 1f, -1f,  1f), new Vertex3D(-1f, -1f,  1f),
    };

    static void Main()
    {
        // ── Init ─────────────────────────────────────────────────────────
        Controls.SetSamplingMode(PspCtrlMode.Analog);
        Gu.Init();       // VRAM allocation + GU setup + display enable

        // Set default render states
        Gu.StartFrame();
            Gu.Enable(GuState.CullFace);
            Gu.SetFrontFace(GuFrontFace.Cw);
            Gu.Enable(GuState.DepthTest);
            Gu.SetDepthFunc(GuDepthFunc.LEqual);
            Gu.SetShadeModel(GuShadeModel.Smooth);
        Gu.EndFrame();

        // ── Game state ───────────────────────────────────────────────────
        float angleY     = 0f;
        float angleX     = 0f;
        float rotSpeedY  = 0.02f;
        float rotSpeedX  = 0.01f;
        bool  autoRotate = true;

        // Per-face colours — one colour per 6 vertices (2 triangles)
        uint[] faceColours = new uint[]
        {
            new Color(220,  60,  60).ToNative(),  // front  — red
            new Color( 60, 220,  60).ToNative(),  // back   — green
            new Color( 60,  60, 220).ToNative(),  // left   — blue
            new Color(220, 220,  60).ToNative(),  // right  — yellow
            new Color( 60, 220, 220).ToNative(),  // top    — cyan
            new Color(220,  60, 220).ToNative(),  // bottom — magenta
        };

        // Build a coloured vertex array: VertexTC (tex + colour + pos)
        // We skip texture coords here (set to 0) and use only colour + pos.
        VertexTC[] coloured = new VertexTC[36];
        for (int face = 0; face < 6; face++)
        {
            uint col = faceColours[face];
            for (int v = 0; v < 6; v++)
            {
                int i = face * 6 + v;
                Vertex3D src = s_cube[i];
                coloured[i] = new VertexTC(0f, 0f, col, src.X, src.Y, src.Z);
            }
        }

        // ── Main loop ────────────────────────────────────────────────────
        while (true)
        {
            // Input
            Controls.PollPad();
            Controls.PollLatch();

            if (Controls.IsKeyDown(PspCtrlButtons.Start)) break;

            if (Controls.IsKeyDown(PspCtrlButtons.Cross))
                autoRotate = !autoRotate;

            if (!autoRotate)
            {
                if (Controls.IsKeyHeld(PspCtrlButtons.Right)) angleY += rotSpeedY;
                if (Controls.IsKeyHeld(PspCtrlButtons.Left))  angleY -= rotSpeedY;
                if (Controls.IsKeyHeld(PspCtrlButtons.Down))  angleX += rotSpeedX;
                if (Controls.IsKeyHeld(PspCtrlButtons.Up))    angleX -= rotSpeedX;
            }
            else
            {
                angleY += rotSpeedY;
                angleX += rotSpeedX * 0.6f;
            }

            // Keep angles in [0, 2π)
            float twoPi = 2f * 3.14159265f;
            if (angleY > twoPi) angleY -= twoPi;
            if (angleX > twoPi) angleX -= twoPi;

            // Render
            Gu.StartFrame();

            Gu.ClearColor(new Color(20, 20, 50));
            Gu.ClearDepth(0);
            Gu.Clear(GuClearFlags.ColorBuffer | GuClearFlags.DepthBuffer);

            // Set up matrices
            Gum.Setup3D(75f, 0.5f, 100f);

            // Model transform: translate back so cube is visible, then rotate
            PspFVector3 pos = new PspFVector3(0f, 0f, -4f);
            Gum.Translate(pos);
            Gum.RotateY(angleY);
            Gum.RotateX(angleX);

            // Draw the coloured cube
            Gu.DrawVerticesTC(GuPrimitive.Triangles, coloured, coloured.Length);

            // HUD text using BasicGraphics2 (uses the same active framebuffer)
            string mode = autoRotate ? "AUTO  " : "MANUAL";
            BasicGraphics2.DrawText(8, 8,
                "PSPDNA 3D Cube  [X]=toggle  [START]=exit",
                new Color(255, 255, 255));
            BasicGraphics2.DrawText(8, 20,
                "Mode: " + mode + "  Y=" + ((int)(angleY * 57.3f)) + "deg",
                new Color(200, 200, 100));

            Gu.EndFrame();
        }

        // ── Cleanup ──────────────────────────────────────────────────────
        Gu.Term();
        Kernel.ExitGame();
    }
}
