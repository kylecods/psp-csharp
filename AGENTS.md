# PSP SDK C# Bindings — Agent & Developer Guide

This document is the single authoritative reference for this repository. It covers the
architecture of the C# binding layer, how to build real PSP executables with the native
pspdev toolchain, how to run them on the PPSSPP emulator and on real PSP hardware, and
how the two layers relate to each other.

---

## Table of Contents

1. [What This Repository Is](#1-what-this-repository-is)
2. [Repository Structure](#2-repository-structure)
3. [The C# Binding Layer](#3-the-c-binding-layer)
   - 3.1 [Two-Layer Architecture](#31-two-layer-architecture)
   - 3.2 [Module Inventory](#32-module-inventory)
   - 3.3 [Interop Patterns Reference](#33-interop-patterns-reference)
   - 3.4 [Error Code Strategy](#34-error-code-strategy)
   - 3.5 [Naming Conventions](#35-naming-conventions)
   - 3.6 [Building and Testing the C# Project](#36-building-and-testing-the-c-project)
4. [PSP Hardware Primer](#4-psp-hardware-primer)
5. [Native PSP Toolchain Setup](#5-native-psp-toolchain-setup)
   - 5.1 [Linux and macOS](#51-linux-and-macos)
   - 5.2 [Windows (WSL2)](#52-windows-wsl2)
   - 5.3 [Verifying the Installation](#53-verifying-the-installation)
   - 5.4 [Toolchain Components](#54-toolchain-components)
6. [Anatomy of a PSP C Program](#6-anatomy-of-a-psp-c-program)
   - 6.1 [Required Headers](#61-required-headers)
   - 6.2 [Module Info and Thread Config](#62-module-info-and-thread-config)
   - 6.3 [Exit Callback Pattern](#63-exit-callback-pattern)
   - 6.4 [VRAM Layout and Buffer Allocation](#64-vram-layout-and-buffer-allocation)
   - 6.5 [Display List](#65-display-list)
   - 6.6 [Full Minimal Program](#66-full-minimal-program)
7. [Makefile Build System](#7-makefile-build-system)
   - 7.1 [Makefile Variables Reference](#71-makefile-variables-reference)
   - 7.2 [Minimal Makefile](#72-minimal-makefile)
   - 7.3 [Build Targets](#73-build-targets)
   - 7.4 [Static Libraries](#74-static-libraries)
   - 7.5 [PRX Kernel Modules](#75-prx-kernel-modules)
8. [CMake Build System](#8-cmake-build-system)
9. [EBOOT.PBP Packaging](#9-ebootpbp-packaging)
   - 9.1 [PBP File Structure](#91-pbp-file-structure)
   - 9.2 [Manual Packaging Commands](#92-manual-packaging-commands)
   - 9.3 [Icon and Background Images](#93-icon-and-background-images)
10. [Running on PPSSPP Emulator](#10-running-on-ppsspp-emulator)
    - 10.1 [Installation](#101-installation)
    - 10.2 [Loading a Homebrew](#102-loading-a-homebrew)
    - 10.3 [Useful Emulator Settings for Developers](#103-useful-emulator-settings-for-developers)
11. [Running on Real Hardware](#11-running-on-real-hardware)
    - 11.1 [Custom Firmware Requirements](#111-custom-firmware-requirements)
    - 11.2 [PSP Models and RAM](#112-psp-models-and-ram)
    - 11.3 [Memory Stick Layout](#113-memory-stick-layout)
    - 11.4 [Deploying Your Build](#114-deploying-your-build)
12. [Debugging with PSPLink](#12-debugging-with-psplink)
13. [Rendering Pipeline Walkthrough](#13-rendering-pipeline-walkthrough)
    - 13.1 [One Frame in Detail](#131-one-frame-in-detail)
    - 13.2 [Vertex Format Flags](#132-vertex-format-flags)
    - 13.3 [Texture Setup](#133-texture-setup)
14. [Sample Programs](#14-sample-programs)
    - 14.1 [Hello World (Text on Screen)](#141-hello-world-text-on-screen)
    - 14.2 [Rotating Cube (3D)](#142-rotating-cube-3d)
    - 14.3 [Controller Input](#143-controller-input)
    - 14.4 [Audio Playback](#144-audio-playback)
15. [Mapping C# Bindings to C Code](#15-mapping-c-bindings-to-c-code)
16. [Common Build Problems](#16-common-build-problems)
17. [Library Reference](#17-library-reference)
18. [Running C# on the PSP](#18-running-c-on-the-psp)
    - 18.1 [The Core Problem](#181-the-core-problem)
    - 18.2 [Approach A — Mono on PSP](#182-approach-a--mono-on-psp-proven-path)
    - 18.3 [Approach B — IL2CPP Pipeline](#183-approach-b--il2cpp-pipeline-no-runtime-on-psp)
    - 18.4 [Approach C — NativeAOT with MIPS Backend](#184-approach-c--net-nativeaot-with-mips-backend-research)
    - 18.5 [Recommended Workflow Today](#185-recommended-development-workflow-today)
    - 18.6 [Full Build Pipeline Summary](#186-full-build-pipeline-summary)
19. [Approach D — PSPDNA (DotNetAnywhere on PSP)](#19-approach-d--pspdna-dotnetanywhere-on-psp)
    - 19.1 [What is DotNetAnywhere?](#191-what-is-dotnetanywhere-dna)
    - 19.2 [How PSPDNA Differs from A, B, C](#192-how-pspdna-differs-from-approaches-a-b-c)
    - 19.3 [Architecture Deep Dive](#193-architecture-deep-dive)
    - 19.4 [Internal Calls vs P/Invoke](#194-how-psp-hardware-access-works-internal-calls)
    - 19.5 [Existing PSP Namespace](#195-the-existing-psp-namespace-corlibpsp)
    - 19.6 [Build Setup and Full Workflow](#196-build-setup-and-full-workflow)
    - 19.7 [Writing C# for PSPDNA](#197-writing-c-for-pspdna)
    - 19.8 [Extending PSPDNA with New Bindings](#198-extending-pspdna-with-new-psp-bindings)
    - 19.9 [Adapting PspSdk Bindings to PSPDNA](#199-adapting-the-pspsdk-c-bindings-to-pspdna)
    - 19.10 [Module Mapping Table](#1910-module-mapping-pspsdk-bindings--pspdna-internal-calls)
    - 19.11 [New Game Project Structure](#1911-pspdna-project-structure-for-a-new-game)
    - 19.12 [Known Limitations](#1912-known-limitations-of-pspdna)
    - 19.13 [Comparison: All Four Approaches](#1913-comparison-all-four-approaches)

---

## 1. What This Repository Is

This repository has two related but independent purposes:

**Purpose A — C# P/Invoke Bindings (the `src/PspSdk/` project)**
A .NET 8 class library that declares C# bindings for a subset of the PSP SDK using
Platform Invocation Services (P/Invoke). The library is an educational artifact: it
teaches the C# interop system by mapping real PSP native functions to idiomatic C#.
It is not intended to run on PSP hardware (the PSP does not run the .NET runtime).
It compiles and all pure-C# logic (struct layouts, result types) is tested on desktop.

**Purpose B — Native PSP Development Guide (this document)**
A comprehensive reference for building real PSP executables in C/C++ using the
pspdev toolchain, packaging them as EBOOT.PBP, and running them on PPSSPP or real
PSP hardware.

The C# bindings in Purpose A are a 1:1 mirror of the C SDK covered in Purpose B.
Reading both together teaches the interop patterns while also teaching PSP programming.

---

## 2. Repository Structure

```
psp-csharp/
├── AGENTS.md                      ← this file
├── PspSdk.sln
│
├── src/
│   └── PspSdk/                    ← C# P/Invoke binding library
│       ├── PspSdk.csproj
│       ├── Core/
│       │   ├── PspTypes.cs        PspFVector3, PspFMatrix4 (struct layout)
│       │   ├── PspError.cs        PspErrorCode enum, PspException
│       │   ├── PspResult.cs       PspResult<T> / PspResult (Result pattern)
│       │   └── NativeMemory.cs    GCHandle pinning, PinnedArray<T> RAII
│       ├── Display/
│       │   ├── DisplayEnums.cs    PixelFormat, SyncMode, DisplayMode
│       │   ├── DisplayModule.cs   Safe public API
│       │   └── Native/
│       │       └── DisplayNative.cs   [DllImport] sceDisplay*
│       ├── Ctrl/
│       │   ├── CtrlEnums.cs       [Flags] CtrlButtons, SamplingMode
│       │   ├── CtrlStructs.cs     CtrlData (20 bytes), CtrlLatch (16 bytes)
│       │   ├── CtrlModule.cs      Safe public API
│       │   └── Native/
│       │       └── CtrlNative.cs  [DllImport] sceCtrl*
│       ├── Audio/
│       │   ├── AudioEnums.cs      AudioFormat, AudioChannel constants
│       │   ├── AudioModule.cs     Safe public API + AudioChannelHandle
│       │   └── Native/
│       │       └── AudioNative.cs [DllImport] sceAudio*
│       ├── Gu/
│       │   ├── GuEnums.cs         GuState, GuPixelMode, GuPrimitive, [Flags] enums
│       │   ├── GuModule.cs        Safe public API with DrawArray<TVertex>
│       │   └── Native/
│       │       └── GuNative.cs    [DllImport] sceGu*
│       ├── Gum/
│       │   ├── GumEnums.cs        MatrixMode
│       │   ├── GumModule.cs       Safe public API
│       │   └── Native/
│       │       └── GumNative.cs   [DllImport] sceGum*
│       └── Samples/
│           ├── CtrlSample.cs      Controller input usage example
│           ├── DisplaySample.cs   Display/VSync usage example
│           └── AudioSample.cs     Audio channel RAII usage example
│
└── tests/
    └── PspSdk.Tests/              ← xUnit tests (runs on desktop, no PSP needed)
        ├── PspSdk.Tests.csproj
        ├── Core/
        │   └── PspResultTests.cs  PspResult<T> success/error/TryGetValue
        └── Ctrl/
            └── CtrlStructTests.cs Marshal.SizeOf layout verification
```

For native PSP C/C++ projects, create a separate directory alongside this repository
(or inside it under `native/`) with the structure described in Section 7.

---

## 3. The C# Binding Layer

### 3.1 Two-Layer Architecture

Every module follows a strict two-layer design:

```
Native/XxxNative.cs     internal static class
                        Raw [DllImport] declarations
                        unsafe code is allowed here
                        raw int parameters, void* pointers
                        never called directly by user code

XxxModule.cs            public static class
                        Typed enum parameters (no raw ints)
                        no 'unsafe' at the public call site
                        error codes → PspException or PspResult
                        managed arrays → pinned transparently with 'fixed'
```

The `Native` sub-namespace is the "danger zone." Everything in it is an implementation
detail. The parent namespace is the clean public API surface.

### 3.2 Module Inventory

| Module | C# namespace | PSP functions | Key teaching |
|---|---|---|---|
| Core | `PspSdk.Core` | (infrastructure only) | `PspResult<T>`, `PspException`, `PinnedArray<T>` |
| Display | `PspSdk.Display` | `sceDisplay*` | IntPtr vs void*, int-to-bool, fixed(T* ptr) |
| Ctrl | `PspSdk.Ctrl` | `sceCtrl*` | `[StructLayout]`, `fixed byte[]`, `[Flags]` enum, ref/out |
| Audio | `PspSdk.Audio` | `sceAudio*` | Resource-handle return, `IDisposable` RAII |
| GU | `PspSdk.Gu` | `sceGu*` | Generic `DrawArray<TVertex> where T : unmanaged` |
| GUM | `PspSdk.Gum` | `sceGum*` | `ref PspFVector3` auto-pinned struct pointer |

### 3.3 Interop Patterns Reference

Each pattern below is demonstrated in at least one file. Reading those files in order
teaches the full C# interop system.

**`[DllImport]` with EntryPoint**
```csharp
// The C# method name can differ from the C function name.
// EntryPoint preserves the exact C symbol (case-sensitive).
[DllImport("sceDisplay",
    EntryPoint        = "sceDisplaySetMode",
    CallingConvention = CallingConvention.Cdecl)]
internal static extern int SetMode(int mode, int width, int height);
```
File: `Display/Native/DisplayNative.cs`

---

**`[StructLayout(LayoutKind.Sequential)]` for C struct mapping**
```csharp
// Sequential means fields are laid out in declaration order.
// Without this, the JIT may reorder fields for performance — breaking P/Invoke.
[StructLayout(LayoutKind.Sequential)]
public unsafe struct CtrlData
{
    public uint TimeStamp;
    public uint Buttons;
    public byte Lx, Ly, Rx, Ry;
    public fixed byte Reserved[6];   // embeds 6 bytes inline
}
```
File: `Ctrl/CtrlStructs.cs`

---

**`fixed byte Field[N]` for inline C arrays**
```csharp
// C:  unsigned char reserved[6];
// Without 'fixed', C# stores a reference (8 bytes on 64-bit) — wrong size.
// With 'fixed', 6 bytes are embedded inline — matches the C layout.
public fixed byte Reserved[6];
```
File: `Ctrl/CtrlStructs.cs`

---

**`[Flags]` enum for C bitmasks**
```csharp
[Flags]
public enum CtrlButtons : uint
{
    Cross    = 0x004000,
    Circle   = 0x002000,
    Triangle = 0x001000,
    FaceButtons = Triangle | Circle | Cross | Square,
}
// Usage: pad.PressedButtons.HasFlag(CtrlButtons.Cross)
```
File: `Ctrl/CtrlEnums.cs`

---

**`IntPtr` vs `unsafe void*` dual overloads**
```csharp
// Safe overload: no 'unsafe' at call site
[DllImport(..., EntryPoint = "sceDisplaySetFrameBuf")]
internal static extern int SetFrameBuf(IntPtr topaddr, int bufferwidth, int pixelformat, int sync);

// Unsafe overload: same EntryPoint, accepts raw pointer from 'fixed' block
[DllImport(..., EntryPoint = "sceDisplaySetFrameBuf")]
internal static extern unsafe int SetFrameBuf(void* topaddr, int bufferwidth, int pixelformat, int sync);
```
File: `Display/Native/DisplayNative.cs`

---

**`fixed (T* ptr = arr)` to pin managed arrays**
```csharp
// 'fixed' pins the array so the GC cannot move it during the native call.
// The 'unsafe' is confined inside this method — callers see a clean uint[].
public static unsafe void SetFrameBuf(uint[] pixels, ...)
{
    fixed (uint* ptr = pixels)
    {
        int result = DisplayNative.SetFrameBuf(ptr, ...);
        PspException.ThrowIfError(result, nameof(SetFrameBuf));
    }
}
```
File: `Display/DisplayModule.cs`

---

**`ref T` parameter (auto-pinned struct pointer)**
```csharp
// 'ref' causes the runtime to pass &padData automatically.
// No 'unsafe' keyword at the declaration or call site.
[DllImport(..., EntryPoint = "sceCtrlReadBufferPositive")]
internal static extern int ReadBufferPositive(ref CtrlData padData, int count);
```
File: `Ctrl/Native/CtrlNative.cs`

---

**`[Out] out T` for write-only native output**
```csharp
// [Out] tells the marshaler the struct is written by native code.
// The runtime skips copying managed data into the native buffer on entry.
[DllImport(..., EntryPoint = "sceCtrlReadLatch")]
internal static extern int ReadLatch([Out] out CtrlLatch latch);
```
File: `Ctrl/Native/CtrlNative.cs`

---

**Generic `where T : unmanaged` with `fixed T*`**
```csharp
// 'unmanaged' constraint: TVertex must be a blittable value type with no managed refs.
// Checked at compile time. Allows pinning any user-defined vertex struct.
public static unsafe void DrawArray<TVertex>(
    GuPrimitive prim, GuVertexType vtype, int count, TVertex[] vertices)
    where TVertex : unmanaged
{
    fixed (TVertex* vptr = vertices)
        GuNative.DrawArray((int)prim, (int)vtype, count, null, vptr);
}
```
File: `Gu/GuModule.cs`

---

**`GCHandle.Alloc(Pinned)` RAII**
```csharp
// PinnedArray<T> pins on construction, unpins on Dispose.
// Prevents GC from moving the array during an extended native operation.
using var pinned = NativeMemoryHelper.Pin(myArray);
NativeFunc(pinned.Pointer);
// Unpinned automatically here
```
File: `Core/NativeMemory.cs`

---

**`IDisposable` over a native resource handle**
```csharp
// AudioChannelHandle wraps the native int channel ID.
// Calls sceAudioChRelease on Dispose — automatic cleanup with 'using'.
using var channel = AudioChannelHandle.Reserve(512, AudioFormat.Stereo);
channel.Output(pcmBuffer);
// sceAudioChRelease called here
```
File: `Audio/AudioModule.cs`

---

**int-as-bool conversion**
```csharp
// C functions use int for booleans: 0 = false, non-zero = true.
// Convert at the safe wrapper boundary so callers get a proper C# bool.
public static bool IsVblank() => DisplayNative.IsVblank() != 0;
```
File: `Display/DisplayModule.cs`

### 3.4 Error Code Strategy

PSP native functions encode errors as negative integers. The upper 16 bits identify
the subsystem; the lower 16 bits are the specific error number.

| Native return pattern | C# approach |
|---|---|
| `0` = ok, negative = error, method is effectively void | `PspException.ThrowIfError(result, name)` in safe wrapper |
| `0` = ok, negative = error, caller may inspect | Return `PspResult` |
| Non-negative = resource handle (e.g. channel ID), negative = error | Return `PspResult<int>` (`.Value` IS the handle) |
| Unsigned counter, no error possible | Return value directly (e.g. `uint`) |

### 3.5 Naming Conventions

| Layer | Convention | Example |
|---|---|---|
| Native C# method | Strip `sce` prefix, PascalCase | `sceDisplaySetMode` → `DisplayNative.SetMode` |
| Public module method | No prefix, on module class | `DisplayModule.SetMode(...)` |
| Structs | Strip `Sce` prefix | `SceCtrlData` → `CtrlData` |
| Enums | Descriptive, no prefix | `CtrlButtons`, `PixelFormat`, `GuState` |
| EntryPoint string | Exact C name, always | `"sceDisplaySetMode"` |

### 3.6 Building and Testing the C# Project

Prerequisites: .NET 8 SDK.

```bash
# Build
dotnet build

# Run all tests (runs on desktop, no PSP hardware or emulator needed)
dotnet test

# Build in release mode
dotnet build -c Release
```

Expected test output: `Passed! — Failed: 0, Passed: 20, Skipped: 0`

The tests verify:
- `Marshal.SizeOf<CtrlData>() == 20` — struct layout matches C
- `Marshal.SizeOf<CtrlLatch>() == 16` — struct layout matches C
- All field offsets in `CtrlData` are correct
- `PspResult<T>` success/error/TryGetValue behaviour
- `[Flags]` enum bitwise operations

---

## 4. PSP Hardware Primer

Understanding the hardware is essential for writing correct native code.

| Component | Specification |
|---|---|
| CPU | MIPS R4000 allegrex @ 222–333 MHz |
| RAM (PSP-1000) | 32 MB main RAM |
| RAM (PSP-2000/3000/Go) | 64 MB physical, 32 MB user-accessible by default (64 MB with large-memory flag) |
| VRAM | 4 MB (shared; accessible at physical address `0x04000000`) |
| Screen | 480×272 pixels, 4.3" LCD |
| Buffer stride | 512 pixels (VRAM rows are 512 wide even though screen is 480) |
| GPU | GE (Graphics Engine) — fixed-function with GU library on top |
| Audio | 8 hardware PCM channels, 44100 Hz stereo |
| Storage | Memory Stick Pro Duo (ms0:), UMD disc (disc0:), internal flash (flash0:) |
| Connectivity | USB 1.1, IR, WiFi 802.11b |

**VRAM address space:**

```
Physical: 0x04000000  (used in kernel/PRX code)
Cached:   0x44000000  (CPU cache-coherent, use for display list and buffers in user mode)
```

User-mode programs use `0x44000000` as the VRAM base when calling GU functions.

---

## 5. Native PSP Toolchain Setup

The pspdev toolchain is a cross-compiler that runs on your desktop (Linux, macOS, or
Windows via WSL2) and produces MIPS ELF binaries for the PSP.

### 5.1 Linux and macOS

```bash
# Clone the installer
git clone https://github.com/pspdev/pspdev.git
cd pspdev

# Install OS-level build dependencies
# Ubuntu/Debian:
sudo apt-get install -y build-essential cmake git texinfo flex bison gettext \
    wget libgmp-dev libmpfr-dev libmpc-dev libusb-dev libreadline-dev \
    libarchive-dev libgpgme-dev

# macOS (Homebrew):
brew install cmake gsl gmp mpfr libmpc libusb readline libarchive gpgme bash \
    openssl libtool texinfo flex bison wget gettext

# Build everything (takes 30–60 minutes on first run)
export PSPDEV=/usr/local/pspdev
sudo mkdir -p $PSPDEV && sudo chown $USER $PSPDEV
./build-all.sh
```

**Alternatively**, download a pre-built release:
```bash
# Get the latest release for your OS from:
# https://github.com/pspdev/pspdev/releases
# Extract and add to PATH.

# Example (Linux x86_64):
wget https://github.com/pspdev/pspdev/releases/latest/download/pspdev-ubuntu-latest-x86_64.tar.gz
sudo tar -xzf pspdev-ubuntu-latest-x86_64.tar.gz -C /usr/local
```

**Add to shell profile (`~/.bashrc`, `~/.zshrc`, or `~/.bash_profile`):**
```bash
export PSPDEV=/usr/local/pspdev
export PATH=$PATH:$PSPDEV/bin
```

Reload: `source ~/.bashrc`

### 5.2 Windows (WSL2)

The toolchain does not build natively on Windows. Use WSL2 with Ubuntu 22.04:

```powershell
# In PowerShell (administrator):
wsl --install -d Ubuntu-22.04
```

Then follow the Linux instructions inside the WSL2 terminal. Your project files can
live in `/mnt/c/...` (Windows filesystem) or inside WSL (faster I/O).

### 5.3 Verifying the Installation

```bash
psp-gcc --version
# Expected: psp-gcc (PSPDEV) 13.x.x or similar

psp-config --pspsdk-path
# Expected: /usr/local/pspdev/psp/sdk (or wherever PSPDEV points)

which pack-pbp
# Expected: /usr/local/pspdev/bin/pack-pbp

which mksfoex
# Expected: /usr/local/pspdev/bin/mksfoex
```

### 5.4 Toolchain Components

| Tool | Purpose |
|---|---|
| `psp-gcc` | C compiler (MIPS cross-compiler) |
| `psp-g++` | C++ compiler |
| `psp-ld` | Linker |
| `psp-as` | Assembler |
| `psp-ar` / `psp-ranlib` | Static library tools |
| `psp-strip` | Strip debug symbols from ELF |
| `psp-fixup-imports` | Fix import stubs after linking |
| `psp-prxgen` | Convert ELF to PRX relocatable module |
| `psp-build-exports` | Generate export tables for PRX modules |
| `psp-config` | Query SDK paths and flags |
| `mksfoex` | Create PARAM.SFO metadata file |
| `pack-pbp` | Package files into EBOOT.PBP |
| `bin2o` / `bin2s` | Convert binary files to linkable objects |

---

## 6. Anatomy of a PSP C Program

### 6.1 Required Headers

```c
#include <pspkernel.h>      // PSP_MODULE_INFO, sceKernel*, exit callbacks
#include <pspdisplay.h>     // sceDisplayWaitVblankStart, sceGuDisplay
#include <pspctrl.h>        // sceCtrlReadBufferPositive, SceCtrlData
#include <pspgu.h>          // sceGu* — graphics utility (GU) library
#include <pspgum.h>         // sceGum* — matrix math (GUM) library
#include <pspge.h>          // sceGe* — low-level graphics engine
#include <pspaudio.h>       // sceAudio* — audio playback
#include <psprtc.h>         // sceRtc* — real-time clock
#include <psppower.h>       // scePower* — power management
```

All headers are in `$(PSPDEV)/psp/sdk/include/`.

### 6.2 Module Info and Thread Config

These macros must appear exactly once in the file that contains `main()`.

```c
// Module name (max 27 chars), attributes, major version, minor version.
// PSP_MODULE_USER = standard user-space homebrew.
PSP_MODULE_INFO("MyApp", PSP_MODULE_USER, 1, 0);

// Main thread runs in user mode with VFPU access.
// THREAD_ATTR_VFPU enables the vector FPU for sceGum matrix operations.
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_VFPU | THREAD_ATTR_USER);

// Heap size for malloc/free. Leave room for stack and system overhead.
// PSP-1000: max ~20 MB; PSP-2000/3000 with large-memory: max ~52 MB.
PSP_HEAP_SIZE_KB(20480);  // 20 MB heap

// Optional: override main thread stack size (default is 256 KB)
// PSP_MAIN_THREAD_STACK_SIZE_KB(256);

// Optional: request 64 MB mode on PSP-2000/3000 (CFW required)
// PSP_LARGE_MEMORY(1);
```

### 6.3 Exit Callback Pattern

The PSP firmware sends an exit request when the user presses the Home button. Your
program must register a callback thread to handle this gracefully.

```c
#include <pspkernel.h>

PSP_MODULE_INFO("MyApp", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_VFPU | THREAD_ATTR_USER);

static volatile int g_running = 1;

// Called by the kernel when the user requests exit (Home button, etc.)
static int exit_callback(int arg1, int arg2, void *common)
{
    g_running = 0;
    return 0;
}

// A dedicated thread that registers the exit callback and then sleeps.
// The kernel wakes this thread when an exit event fires.
static int callback_thread(SceSize args, void *argp)
{
    int cbid = sceKernelCreateCallback("Exit Callback", exit_callback, NULL);
    sceKernelRegisterExitCallback(cbid);
    sceKernelSleepThreadCB();    // sleep until a callback fires
    return 0;
}

// Call once from main() before entering the game loop.
static void setup_callbacks(void)
{
    // Priority 0x11 (lower number = higher priority; 0x11 is standard for helpers)
    // Stack size 0xFA0 = 4000 bytes — enough for the callback thread
    int thid = sceKernelCreateThread("cb_thread", callback_thread, 0x11, 0xFA0, 0, 0);
    if (thid >= 0)
        sceKernelStartThread(thid, 0, NULL);
}
```

### 6.4 VRAM Layout and Buffer Allocation

VRAM is 4 MB starting at physical address `0x04000000`. In user-mode programs,
use `0x44000000` (CPU-cached alias) for the GU API. Buffers must be 512 pixels wide
regardless of the 480-pixel display (hardware alignment requirement).

```
VRAM map (typical double-buffered 32-bit setup):
  Offset 0x00000000  — Draw buffer    (fbp0): 512 * 272 * 4 = 557,056 bytes
  Offset 0x00088000  — Display buffer (fbp1): 512 * 272 * 4 = 557,056 bytes
  Offset 0x00110000  — Depth buffer   (zbp):  512 * 272 * 2 = 278,528 bytes (16-bit)
  Offset 0x00154000  — Free for textures and other data
```

Helper to allocate VRAM buffers sequentially:

```c
#define BUF_WIDTH   512   // memory stride (must be 512 for the GU)
#define SCR_WIDTH   480   // visible pixels per row
#define SCR_HEIGHT  272   // visible pixels per column

static unsigned int vram_offset = 0;

// Returns a pointer into VRAM at 0x44000000 + current offset,
// then advances the offset by the buffer's byte size.
static void *vram_alloc(int width, int height, int psm)
{
    int bytes_per_pixel;
    switch (psm) {
        case GU_PSM_4444: case GU_PSM_5650: case GU_PSM_5551:
            bytes_per_pixel = 2; break;
        default:  // GU_PSM_8888
            bytes_per_pixel = 4; break;
    }
    void *ptr = (void *)((unsigned int)0x44000000 + vram_offset);
    vram_offset += width * height * bytes_per_pixel;
    return ptr;
}

// In main():
void *fbp0 = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_8888); // draw buffer
void *fbp1 = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_8888); // display buffer
void *zbp  = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_4444); // depth buffer (16-bit)
```

### 6.5 Display List

The display list is a command buffer that the CPU fills and the GPU executes. Declare
it as a global 16-byte-aligned array. 256 KB (1 MB of 32-bit entries) is standard.

```c
// 16-byte alignment is required by the GU DMA engine.
static unsigned int __attribute__((aligned(16))) display_list[262144];
```

The GU operates in two modes:
- `GU_DIRECT` — commands are submitted directly to the hardware. Use this for most cases.
- `GU_CALL` — commands are recorded into a secondary list and called from the primary list.

### 6.6 Full Minimal Program

```c
#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspgu.h>
#include <pspgum.h>
#include <pspctrl.h>
#include <math.h>

PSP_MODULE_INFO("MinimalApp", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_VFPU | THREAD_ATTR_USER);
PSP_HEAP_SIZE_KB(20480);

#define BUF_WIDTH  512
#define SCR_WIDTH  480
#define SCR_HEIGHT 272

static volatile int   g_running = 1;
static unsigned int   vram_offset = 0;
static unsigned int   __attribute__((aligned(16))) display_list[262144];

static int exit_callback(int arg1, int arg2, void *common)
    { g_running = 0; return 0; }

static int callback_thread(SceSize args, void *argp)
{
    int cbid = sceKernelCreateCallback("Exit Callback", exit_callback, NULL);
    sceKernelRegisterExitCallback(cbid);
    sceKernelSleepThreadCB();
    return 0;
}

static void setup_callbacks(void)
{
    int thid = sceKernelCreateThread("cb_thread", callback_thread, 0x11, 0xFA0, 0, 0);
    if (thid >= 0) sceKernelStartThread(thid, 0, NULL);
}

static void *vram_alloc(int w, int h, int psm)
{
    int bpp = (psm == GU_PSM_4444 || psm == GU_PSM_5650 || psm == GU_PSM_5551) ? 2 : 4;
    void *p = (void *)((unsigned int)0x44000000 + vram_offset);
    vram_offset += w * h * bpp;
    return p;
}

int main(int argc, char *argv[])
{
    setup_callbacks();

    void *fbp0 = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_8888);
    void *fbp1 = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_8888);
    void *zbp  = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_4444);

    sceGuInit();
    sceGuStart(GU_DIRECT, display_list);
        sceGuDrawBuffer(GU_PSM_8888, fbp0, BUF_WIDTH);
        sceGuDispBuffer(SCR_WIDTH, SCR_HEIGHT, fbp1, BUF_WIDTH);
        sceGuDepthBuffer(zbp, BUF_WIDTH);
        sceGuOffset(2048 - SCR_WIDTH / 2, 2048 - SCR_HEIGHT / 2);
        sceGuViewport(2048, 2048, SCR_WIDTH, SCR_HEIGHT);
        sceGuDepthRange(65535, 0);
        sceGuScissor(0, 0, SCR_WIDTH, SCR_HEIGHT);
        sceGuEnable(GU_SCISSOR_TEST);
        sceGuEnable(GU_DEPTH_TEST);
        sceGuDepthFunc(GU_LEQUAL);
        sceGuShadeModel(GU_SMOOTH);
        sceGuEnable(GU_CULL_FACE);
        sceGuFrontFace(GU_CW);
    sceGuFinish();
    sceGuSync(GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);

    sceDisplayWaitVblankStart();
    sceGuDisplay(GU_TRUE);

    sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);

    while (g_running) {
        SceCtrlData pad;
        sceCtrlReadBufferPositive(&pad, 1);
        if (pad.Buttons & PSP_CTRL_START)
            break;

        sceGuStart(GU_DIRECT, display_list);
            sceGuClearColor(0xFF404040);  // ABGR: opaque grey
            sceGuClearDepth(0);
            sceGuClear(GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);
            // --- draw calls go here ---
        sceGuFinish();
        sceGuSync(GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);

        sceDisplayWaitVblankStart();
        sceGuSwapBuffers();
    }

    sceGuTerm();
    sceKernelExitGame();
    return 0;
}
```

---

## 7. Makefile Build System

The pspsdk ships with `$(PSPSDK)/lib/build.mak`, a GNU Makefile that handles all
PSP-specific compilation, linking, and EBOOT.PBP packaging.

### 7.1 Makefile Variables Reference

Set these variables **before** the `include $(PSPSDK)/lib/build.mak` line.

| Variable | Default | Description |
|---|---|---|
| `TARGET` | *(required)* | Output name, no extension. Produces `TARGET.elf` and `TARGET.prx`. |
| `OBJS` | *(required)* | Space-separated list of `.o` files (or `.c`/`.cpp` files). |
| `INCDIR` | empty | Extra include directories. |
| `LIBDIR` | empty | Extra library search directories. |
| `LIBS` | empty | Libraries to link. Order matters (link dependencies after dependents). |
| `CFLAGS` | `-O2 -G0 -Wall` | C compiler flags. `-G0` disables small data optimisation (required for PSP). |
| `CXXFLAGS` | `$(CFLAGS)` | C++ compiler flags. Add `-fno-exceptions -fno-rtti` to reduce binary size. |
| `ASFLAGS` | `$(CFLAGS)` | Assembler flags. |
| `LDFLAGS` | empty | Extra linker flags. |
| `BUILD_PRX` | 0 | Set to `1` to build a `.prx` kernel module instead of a user executable. |
| `PSP_FW_VERSION` | 600 | Target firmware version (600 = 6.00, 661 = 6.61). |
| `PSP_LARGE_MEMORY` | 0 | Set to `1` to request 64 MB RAM on PSP-2000/3000 (CFW only). |
| `PSP_EBOOT_TITLE` | `$(TARGET)` | Title string displayed in XMB (max 127 chars). |
| `PSP_EBOOT_ICON` | `NULL` | Path to `ICON0.PNG` (144×80 pixels, PNG). |
| `PSP_EBOOT_ICON1` | `NULL` | Path to `ICON1.PMF` (animated icon, optional). |
| `PSP_EBOOT_PIC0` | `NULL` | Path to background image `PIC0.PNG` (480×272, PNG). |
| `PSP_EBOOT_PIC1` | `NULL` | Path to secondary background (optional). |
| `PSP_EBOOT_SND0` | `NULL` | Path to boot sound `SND0.AT3` (optional). |
| `EXTRA_TARGETS` | empty | Set to `EBOOT.PBP` to generate the final package. |

### 7.2 Minimal Makefile

```makefile
# Name of your program (no extension)
TARGET = my_app

# Source files to compile
OBJS = main.o

# Additional include paths (beyond SDK defaults)
INCDIR =

# Compiler flags
CFLAGS   = -O2 -G0 -Wall
CXXFLAGS = $(CFLAGS) -fno-exceptions -fno-rtti
ASFLAGS  = $(CFLAGS)

# Library search paths
LIBDIR  =

# Linker flags
LDFLAGS =

# Libraries to link (GU, GUM, Display, Ctrl, Geometry, Math, PSP user, C runtime)
# Order: dependent first, dependency last.
LIBS = -lpspgu -lpspgum -lpspdisplay -lpspctrl -lpspge -lm -lpspuser -lc

# Generate EBOOT.PBP in addition to the ELF
EXTRA_TARGETS = EBOOT.PBP
PSP_EBOOT_TITLE = My PSP Application

# Optional: custom icon and background
# PSP_EBOOT_ICON = icon0.png
# PSP_EBOOT_PIC0 = pic0.png

# Include the PSP build system (must be the last line)
PSPSDK = $(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
```

### 7.3 Build Targets

```bash
# Build everything (ELF + EBOOT.PBP)
make

# Clean all generated files
make clean

# Build only the ELF (no PBP)
make TARGET.elf

# Rebuild from scratch
make clean && make
```

Build artifacts:
```
my_app.elf         Raw MIPS ELF (before fixup) — not for distribution
my_app_fixup.elf   ELF with import stubs fixed — intermediate
PARAM.SFO          System parameter file (metadata)
EBOOT.PBP          Final distributable package
```

### 7.4 Static Libraries

To build a static `.a` library instead of an executable:

```makefile
TARGET_LIB = libmylib.a
OBJS       = foo.o bar.o baz.o
INCDIR     =
CFLAGS     = -O2 -G0 -Wall

PSPSDK = $(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
```

Do not set `EXTRA_TARGETS` for libraries. The library can then be linked into a main
program with `-lmylib` after adding its directory to `LIBDIR`.

### 7.5 PRX Kernel Modules

PRX modules are relocatable position-independent shared objects that can be loaded
into the PSP kernel at runtime. Most homebrew does not need this.

```makefile
TARGET    = mymodule
OBJS      = mymodule.o
BUILD_PRX = 1    # produce mymodule.prx instead of mymodule.elf
LIBS      = -lpspuser -lc

PSPSDK = $(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
```

---

## 8. CMake Build System

The pspdev toolchain ships a CMake toolchain file at:
```
$PSPDEV/psp/share/pspdev.cmake
```

### CMakeLists.txt

```cmake
cmake_minimum_required(VERSION 3.20)
project(MyPspApp C)

# Tell CMake we are cross-compiling for PSP
# This is already set by the toolchain file — just declare sources.
add_executable(my_app main.c)

target_link_libraries(my_app
    pspgu pspgum pspdisplay pspctrl pspge m pspuser c
)

# Generate EBOOT.PBP using the SDK's CMake helper
include(${PSPSDK}/cmake/CreatePBP.cmake)
create_pbp_file(
    TARGET        my_app
    TITLE         "My PSP Application"
    BUILD_PRX     FALSE
)
```

### Building with CMake

```bash
mkdir build && cd build
cmake -DCMAKE_TOOLCHAIN_FILE=$PSPDEV/psp/share/pspdev.cmake ..
make
```

The `PSPDEV` environment variable must be set before running `cmake`.

---

## 9. EBOOT.PBP Packaging

EBOOT.PBP is the standard PSP application container. It is a simple concatenation of
several files with a fixed-format header that the firmware's XMB (cross-media bar) and
game loader understand.

### 9.1 PBP File Structure

```
EBOOT.PBP
├── [header]       Fixed 40-byte header with offsets to each section
├── PARAM.SFO      Metadata: title, version, category, memory flags
├── ICON0.PNG      144×80 icon shown in XMB game list
├── ICON1.PMF      Animated icon (optional; PMF video format)
├── PIC0.PNG       480×272 background shown when game is selected
├── PIC1.PNG       Secondary background (optional)
├── SND0.AT3       Boot sound in ATRAC3 format (optional)
├── DATA.PSP       The actual executable (stripped ELF or PRX)
└── DATA.PSAR      Extra data archive (optional)
```

`DATA.PSP` is the output of `psp-fixup-imports` applied to the linked ELF, optionally
compressed or signed for official firmware (not needed for CFW homebrew).

### 9.2 Manual Packaging Commands

The `build.mak` system does this automatically, but for reference:

```bash
# Step 1: Compile and link to ELF
psp-gcc -O2 -G0 -Wall -I$(PSPDEV)/psp/sdk/include \
    -L$(PSPDEV)/psp/sdk/lib -L$(PSPDEV)/psp/lib \
    main.c -o my_app.elf \
    -lpspgu -lpspgum -lpspdisplay -lpspctrl -lpspge -lm -lpspuser -lc

# Step 2: Fix import stubs (resolves PSP firmware function addresses)
psp-fixup-imports my_app.elf

# Step 3: Strip debug symbols (reduces file size)
psp-strip my_app.elf -o my_app_stripped.elf

# Step 4: Create PARAM.SFO metadata
mksfoex -d MEMSIZE=1 "My PSP Application" PARAM.SFO

# Step 5: Package into EBOOT.PBP
# Arguments: output param_sfo icon0 icon1 pic0 pic1 snd0 data_psp data_psar
pack-pbp EBOOT.PBP PARAM.SFO NULL NULL NULL NULL NULL my_app_stripped.elf NULL
```

### 9.3 Icon and Background Images

| File | Dimensions | Format | Purpose |
|---|---|---|---|
| `ICON0.PNG` | 144×80 | PNG (RGB or RGBA) | Game list icon |
| `ICON1.PMF` | — | PMF video | Animated icon (optional) |
| `PIC0.PNG` | 480×272 | PNG (RGB or RGBA) | Background when selected |
| `PIC1.PNG` | 480×272 | PNG | Secondary background (optional) |
| `SND0.AT3` | — | ATRAC3 | Boot sound (optional) |

To use a custom icon, set in the Makefile:
```makefile
PSP_EBOOT_ICON = icon0.png
PSP_EBOOT_PIC0 = pic0.png
```

---

## 10. Running on PPSSPP Emulator

PPSSPP is a high-accuracy open-source PSP emulator. It is the fastest way to test
homebrew without real hardware.

### 10.1 Installation

| Platform | Method |
|---|---|
| Windows | Download from https://ppsspp.org/downloads.html (installer or zip) |
| macOS | Download the `.dmg` from https://ppsspp.org/downloads.html |
| Linux | `sudo apt install ppsspp` or `flatpak install flathub org.ppsspp.PPSSPP` |
| Android | Install from Google Play or F-Droid |
| iOS | Build from source or use AltStore |

### 10.2 Loading a Homebrew

1. Build your project: `make` → produces `EBOOT.PBP`.
2. Launch PPSSPP.
3. Navigate to your `EBOOT.PBP` in the file browser and select it, **or**
4. Place your build in PPSSPP's memstick folder:
   ```
   Windows:  %APPDATA%\PPSSPP\PSP\GAME\MyApp\EBOOT.PBP
   Linux:    ~/.config/PPSSPP/PSP/GAME/MyApp/EBOOT.PBP
   macOS:    ~/Library/Application Support/PPSSPP/PSP/GAME/MyApp/EBOOT.PBP
   ```
   Then your homebrew appears in the PPSSPP game grid.

### 10.3 Useful Emulator Settings for Developers

```
Settings → System:
  • PSP Model: PSP-2000 (64 MB mode, useful for larger programs)
  • Enable Cheats: off (keeps behaviour closer to hardware)

Settings → Graphics:
  • Backend: Vulkan or OpenGL (Vulkan is faster on modern hardware)
  • Rendering Resolution: 1× (480×272 for accurate pixel testing)
  • Simulate Block Transfer Effects: on

Settings → System → Developer:
  • Show Debug Statistics: useful for frame time profiling
  • Log to file: enable for crash analysis
```

PPSSPP has a built-in debugger (menu → Debug → Disassembly) that shows MIPS
assembly, register state, and memory, which is useful for tracking down crashes.

---

## 11. Running on Real Hardware

### 11.1 Custom Firmware Requirements

The PSP's official firmware (OFW) blocks unsigned homebrews. You need custom firmware
(CFW) or a HEN (Homebrew Enabler) to run your own code.

| CFW/HEN | Models supported | Notes |
|---|---|---|
| **6.61 Infinity** | PSP-1000, 2000, 3000, Go | Permanent CFW; survives reboots. Recommended. |
| **PRO-C Continuum** | PSP-1000, 2000, 3000, Go | Semi-permanent (survives most reboots). |
| **LME** (ME CFW) | PSP-2000, 3000 | Alternative for 2000/3000 models. |
| **6.61 HEN** | All models | Temporary; must re-enable after each cold boot. |

**To install 6.61 Infinity:**
1. Update OFW to 6.61 using Sony's official updater.
2. Run the Infinity installer from the XMB (requires the initial HEN exploit).
3. Reboot; the CFW is now permanent.

Full instructions are at: https://pspunk.com/psp-cfw/

### 11.2 PSP Models and RAM

| Model | Main RAM | VRAM | Notes |
|---|---|---|---|
| PSP-1000 (Fat) | 32 MB | 4 MB | Slowest, largest battery life |
| PSP-2000 (Slim) | 64 MB physical / 32 MB default | 4 MB | `PSP_LARGE_MEMORY(1)` enables 64 MB |
| PSP-3000 | 64 MB physical / 32 MB default | 4 MB | Same as 2000 plus improved screen |
| PSP-N1000 (Go) | 64 MB physical / 32 MB default | 4 MB | No UMD drive; internal storage only |
| PSP-E1000 (Street) | 32 MB | 4 MB | No WiFi; no analogue stick on some revisions |

Homebrew that uses `PSP_LARGE_MEMORY(1)` will crash on PSP-1000 or OFW. Test on
PSP-1000 targets to ensure compatibility across all models.

### 11.3 Memory Stick Layout

The Memory Stick Pro Duo is mounted at `ms0:` in the PSP filesystem.

```
ms0:/
├── PSP/
│   ├── GAME/
│   │   └── MyApp/           ← folder name is arbitrary
│   │       ├── EBOOT.PBP    ← your application (required)
│   │       └── data/        ← any supporting files your program loads at runtime
│   ├── MUSIC/
│   ├── PHOTO/
│   └── SAVEDATA/
└── ISO/                     ← UMD ISOs for CFW users (games, not homebrew)
```

Your program can access the memory stick at `ms0:/PSP/GAME/MyApp/data/` using
standard `sceIo*` file functions.

### 11.4 Deploying Your Build

**Via USB (with psplink installed on the PSP):**

See Section 12.

**Manual copy:**
1. Connect the PSP to your computer via USB (Settings → USB Connection on the PSP).
2. The PSP appears as a removable drive.
3. Create the folder `PSP/GAME/MyApp/` on the memory stick.
4. Copy your `EBOOT.PBP` into that folder.
5. Disconnect USB safely (eject the drive on your OS).
6. On the PSP, navigate to Game → Memory Stick and launch your homebrew.

**For frequent iteration**, use a micro-SD card with an adapter and a fast card reader
on your desktop. Write times are dramatically faster than USB.

---

## 12. Debugging with PSPLink

PSPLink (psplinkusb) lets you launch and debug PSP programs over USB without
copying files to the memory stick each time.

### Installation on your desktop

```bash
git clone https://github.com/pspdev/psplinkusb.git
cd psplinkusb
make
sudo make install
```

### Installation on the PSP

1. Copy `psplink/psplink.prx` and `psplink/usbhostfs.prx` to `ms0:/PSP/GAME/psplink/`.
2. Copy the `psplink` EBOOT.PBP to `ms0:/PSP/GAME/psplink/EBOOT.PBP`.
3. Launch psplink from the XMB; it sets up the USB host connection.

### Workflow

```bash
# On the PSP: launch psplink from XMB.

# On desktop: connect to the PSP via USB.
pspsh

# In the pspsh shell:
host0:/> ./my_app.elf    # load and run your ELF directly from the desktop filesystem

# When your program crashes, pspsh shows the MIPS register dump and fault address.
# Cross-reference with: psp-addr2line -e my_app.elf <fault_address>
```

PSPLink eliminates the copy-to-memstick step. Iteration time drops from ~30 seconds
to ~2 seconds.

### Crash analysis

```bash
# Convert a crash address back to a source file and line number:
psp-addr2line -e my_app.elf 0x08804ABC
# Output: src/main.c:142
```

---

## 13. Rendering Pipeline Walkthrough

### 13.1 One Frame in Detail

```c
// --- CPU phase: fill the display list ---
sceGuStart(GU_DIRECT, display_list);

    // Clear colour (ABGR, not ARGB) and depth buffers
    sceGuClearColor(0xFF404040);   // A=FF, B=40, G=40, R=40 → dark grey
    sceGuClearDepth(0);
    sceGuClear(GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);

    // Set up projection matrix
    sceGumMatrixMode(GU_PROJECTION);
    sceGumLoadIdentity();
    sceGumPerspective(75.0f,                    // field of view (degrees, Y axis)
                      (float)SCR_WIDTH / SCR_HEIGHT,  // aspect ratio (≈1.764 for PSP)
                      0.5f,                     // near clip plane
                      1000.0f);                 // far clip plane

    // Set up view matrix (camera is at origin looking down -Z by default)
    sceGumMatrixMode(GU_VIEW);
    sceGumLoadIdentity();

    // Set up model matrix (object transform)
    sceGumMatrixMode(GU_MODEL);
    sceGumLoadIdentity();
    ScePspFVector3 t = {0.0f, 0.0f, -5.0f};
    sceGumTranslate(&t);
    sceGumRotateY(angle);   // angle in radians

    // Submit vertices (see Section 13.2 for vertex format flags)
    sceGumDrawArray(GU_TRIANGLES,
                    GU_VERTEX_32BITF | GU_TRANSFORM_3D,
                    vertex_count, NULL, vertices);

// --- End list recording ---
sceGuFinish();

// --- GPU phase: execute the list ---
sceGuSync(GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);

// --- Wait for display hardware ---
sceDisplayWaitVblankStart();   // block until vsync; prevents tearing

// --- Swap front and back buffers ---
sceGuSwapBuffers();            // display buffer ↔ draw buffer
```

### 13.2 Vertex Format Flags

The `vtype` parameter of `sceGumDrawArray` / `sceGuDrawArray` is a bitmask that
describes the in-memory layout of each vertex. Compose it with bitwise OR.

**Texture coordinate formats (bits 0–1):**
```c
GU_TEXTURE_8BITF   // 8-bit fixed-point UV (rare)
GU_TEXTURE_16BITF  // 16-bit fixed-point UV
GU_TEXTURE_32BITF  // 32-bit float UV (most common)
```

**Colour formats (bits 2–6):**
```c
GU_COLOR_5650      // 16-bit RGB565, no alpha
GU_COLOR_5551      // 16-bit RGBA5551
GU_COLOR_4444      // 16-bit RGBA4444
GU_COLOR_8888      // 32-bit RGBA8888 (most common)
```

**Normal formats (bits 7–8):**
```c
GU_NORMAL_8BIT     // 8-bit fixed normals
GU_NORMAL_16BIT    // 16-bit fixed normals
GU_NORMAL_32BITF   // 32-bit float normals
```

**Position formats (bits 9–10):**
```c
GU_VERTEX_8BIT     // 8-bit fixed positions
GU_VERTEX_16BIT    // 16-bit fixed positions
GU_VERTEX_32BITF   // 32-bit float positions (most common)
```

**Transform mode (bit 23):**
```c
GU_TRANSFORM_3D    // 0 — vertices go through model/view/projection matrices
GU_TRANSFORM_2D    // 1 << 23 — raw screen-space coordinates (2D sprites, UI)
```

**Example vertex struct matching `GU_VERTEX_32BITF | GU_TRANSFORM_3D`:**
```c
typedef struct {
    float x, y, z;
} Vertex3D;
```

**Example matching `GU_TEXTURE_32BITF | GU_COLOR_8888 | GU_VERTEX_32BITF | GU_TRANSFORM_3D`:**
```c
typedef struct {
    float u, v;
    unsigned int color;   // 0xAABBGGRR (ABGR)
    float x, y, z;
} VertexTC;
```

Fields must appear in the struct in the order: texture → color → normal → position.

### 13.3 Texture Setup

```c
// Upload a texture stored in RAM or VRAM.
// tbp: pointer to texture pixel data (must be 16-byte aligned)
// tbw: texture buffer width in pixels (must be a power of 2)
sceGuTexMode(GU_PSM_8888, 0, 0, 0);   // pixel format, no mipmaps, no swizzle
sceGuTexImage(0,                        // mipmap level
              texture_width,            // must be power of 2 (e.g. 256)
              texture_height,           // must be power of 2 (e.g. 256)
              texture_width,            // buffer stride (same as width if no padding)
              texture_data);            // pointer to pixel data
sceGuTexFunc(GU_TFX_MODULATE,          // blend texture with vertex colour
             GU_TCC_RGBA);             // include alpha channel
sceGuTexFilter(GU_LINEAR, GU_LINEAR);  // bilinear filtering (min, mag)
sceGuTexWrap(GU_REPEAT, GU_REPEAT);    // UV wrap mode

sceGuEnable(GU_TEXTURE_2D);
```

Texture dimensions must be powers of 2 (16, 32, 64, 128, 256, 512). The PSP has no
NPOT (non-power-of-two) texture support in hardware.

---

## 14. Sample Programs

### 14.1 Hello World (Text on Screen)

The simplest visible output uses `pspDebugScreenPrintf`, which bypasses GU entirely
and writes to the display framebuffer directly. It is not suitable for games but is
useful for diagnostic output.

```c
#include <pspkernel.h>
#include <pspdebug.h>

PSP_MODULE_INFO("HelloWorld", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER);

static volatile int g_running = 1;
static int exit_cb(int a, int b, void *c) { g_running = 0; return 0; }
static int cb_thread(SceSize a, void *b) {
    int id = sceKernelCreateCallback("Exit", exit_cb, NULL);
    sceKernelRegisterExitCallback(id);
    sceKernelSleepThreadCB();
    return 0;
}

int main(int argc, char *argv[])
{
    int thid = sceKernelCreateThread("cb", cb_thread, 0x11, 0xFA0, 0, 0);
    if (thid >= 0) sceKernelStartThread(thid, 0, NULL);

    pspDebugScreenInit();
    pspDebugScreenSetTextColor(0x00FF00);   // green text (0x00BBGGRR)

    while (g_running) {
        pspDebugScreenSetXY(0, 0);          // cursor position (column, row)
        pspDebugScreenPrintf("Hello, PSP!\n");
        pspDebugScreenPrintf("Press START to exit.\n");
        sceDisplayWaitVblankStart();
    }

    sceKernelExitGame();
    return 0;
}
```

**Makefile:**
```makefile
TARGET        = hello_world
OBJS          = main.o
CFLAGS        = -O2 -G0 -Wall
LIBS          = -lpspuser -lpspdebug -lc
EXTRA_TARGETS = EBOOT.PBP
PSP_EBOOT_TITLE = Hello World

PSPSDK = $(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
```

### 14.2 Rotating Cube (3D)

A textured rotating cube demonstrating the full GU/GUM pipeline.

```c
#include <pspkernel.h>
#include <pspdisplay.h>
#include <pspgu.h>
#include <pspgum.h>
#include <math.h>
#include <string.h>

PSP_MODULE_INFO("RotatingCube", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_VFPU | THREAD_ATTR_USER);
PSP_HEAP_SIZE_KB(20480);

#define BUF_WIDTH  512
#define SCR_WIDTH  480
#define SCR_HEIGHT 272

static volatile int g_running = 1;
static unsigned int vram_offset = 0;
static unsigned int __attribute__((aligned(16))) dlist[262144];

// Vertex with position only (no texture, no colour)
typedef struct { float x, y, z; } Vertex;

// Cube: 6 faces × 2 triangles × 3 vertices = 36 vertices
static Vertex __attribute__((aligned(16))) cube_verts[36] = {
    // Front face
    {-1,-1, 1}, { 1,-1, 1}, { 1, 1, 1},
    {-1,-1, 1}, { 1, 1, 1}, {-1, 1, 1},
    // Back face
    { 1,-1,-1}, {-1,-1,-1}, {-1, 1,-1},
    { 1,-1,-1}, {-1, 1,-1}, { 1, 1,-1},
    // Left face
    {-1,-1,-1}, {-1,-1, 1}, {-1, 1, 1},
    {-1,-1,-1}, {-1, 1, 1}, {-1, 1,-1},
    // Right face
    { 1,-1, 1}, { 1,-1,-1}, { 1, 1,-1},
    { 1,-1, 1}, { 1, 1,-1}, { 1, 1, 1},
    // Top face
    {-1, 1, 1}, { 1, 1, 1}, { 1, 1,-1},
    {-1, 1, 1}, { 1, 1,-1}, {-1, 1,-1},
    // Bottom face
    {-1,-1,-1}, { 1,-1,-1}, { 1,-1, 1},
    {-1,-1,-1}, { 1,-1, 1}, {-1,-1, 1},
};

static int exit_cb(int a, int b, void *c) { g_running = 0; return 0; }
static int cb_thread(SceSize a, void *b) {
    int id = sceKernelCreateCallback("Exit", exit_cb, NULL);
    sceKernelRegisterExitCallback(id);
    sceKernelSleepThreadCB();
    return 0;
}
static void *vram_alloc(int w, int h, int psm) {
    int bpp = (psm == GU_PSM_4444 || psm == GU_PSM_5650) ? 2 : 4;
    void *p = (void *)((unsigned int)0x44000000 + vram_offset);
    vram_offset += w * h * bpp;
    return p;
}

int main(int argc, char *argv[])
{
    int thid = sceKernelCreateThread("cb", cb_thread, 0x11, 0xFA0, 0, 0);
    if (thid >= 0) sceKernelStartThread(thid, 0, NULL);

    void *fbp0 = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_8888);
    void *fbp1 = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_8888);
    void *zbp  = vram_alloc(BUF_WIDTH, SCR_HEIGHT, GU_PSM_4444);

    sceGuInit();
    sceGuStart(GU_DIRECT, dlist);
        sceGuDrawBuffer(GU_PSM_8888, fbp0, BUF_WIDTH);
        sceGuDispBuffer(SCR_WIDTH, SCR_HEIGHT, fbp1, BUF_WIDTH);
        sceGuDepthBuffer(zbp, BUF_WIDTH);
        sceGuOffset(2048 - SCR_WIDTH/2, 2048 - SCR_HEIGHT/2);
        sceGuViewport(2048, 2048, SCR_WIDTH, SCR_HEIGHT);
        sceGuDepthRange(65535, 0);
        sceGuScissor(0, 0, SCR_WIDTH, SCR_HEIGHT);
        sceGuEnable(GU_SCISSOR_TEST);
        sceGuEnable(GU_DEPTH_TEST);
        sceGuDepthFunc(GU_LEQUAL);
        sceGuFrontFace(GU_CW);
        sceGuShadeModel(GU_SMOOTH);
        sceGuEnable(GU_CULL_FACE);
    sceGuFinish();
    sceGuSync(GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);
    sceDisplayWaitVblankStart();
    sceGuDisplay(GU_TRUE);

    float angle = 0.0f;
    while (g_running) {
        sceGuStart(GU_DIRECT, dlist);
            sceGuClearColor(0xFF202040);
            sceGuClearDepth(0);
            sceGuClear(GU_COLOR_BUFFER_BIT | GU_DEPTH_BUFFER_BIT);

            sceGumMatrixMode(GU_PROJECTION);
            sceGumLoadIdentity();
            sceGumPerspective(75.0f, (float)SCR_WIDTH / SCR_HEIGHT, 0.5f, 1000.0f);

            sceGumMatrixMode(GU_VIEW);
            sceGumLoadIdentity();

            sceGumMatrixMode(GU_MODEL);
            sceGumLoadIdentity();
            ScePspFVector3 t = {0.0f, 0.0f, -4.0f};
            sceGumTranslate(&t);
            sceGumRotateY(angle);
            sceGumRotateX(angle * 0.7f);

            // GU_COLOR_8888: add a solid white colour per vertex
            sceGuColor(0xFFFFFFFF);
            sceGumDrawArray(GU_TRIANGLES,
                            GU_VERTEX_32BITF | GU_TRANSFORM_3D,
                            36, NULL, cube_verts);
        sceGuFinish();
        sceGuSync(GU_SYNC_FINISH, GU_SYNC_WHAT_DONE);

        sceDisplayWaitVblankStart();
        sceGuSwapBuffers();

        angle += 0.02f;
    }

    sceGuTerm();
    sceKernelExitGame();
    return 0;
}
```

**Makefile:**
```makefile
TARGET        = rotating_cube
OBJS          = main.o
CFLAGS        = -O2 -G0 -Wall -ffast-math
LIBS          = -lpspgu -lpspgum -lpspdisplay -lpspge -lm -lpspuser -lc
EXTRA_TARGETS = EBOOT.PBP
PSP_EBOOT_TITLE = Rotating Cube

PSPSDK = $(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
```

### 14.3 Controller Input

```c
#include <pspkernel.h>
#include <pspdebug.h>
#include <pspctrl.h>

PSP_MODULE_INFO("CtrlDemo", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER);

static volatile int g_running = 1;
static int exit_cb(int a, int b, void *c) { g_running = 0; return 0; }
static int cb_thread(SceSize a, void *b) {
    int id = sceKernelCreateCallback("Exit", exit_cb, NULL);
    sceKernelRegisterExitCallback(id);
    sceKernelSleepThreadCB();
    return 0;
}

int main(int argc, char *argv[])
{
    int thid = sceKernelCreateThread("cb", cb_thread, 0x11, 0xFA0, 0, 0);
    if (thid >= 0) sceKernelStartThread(thid, 0, NULL);

    pspDebugScreenInit();

    // Enable analog sampling for the left stick
    sceCtrlSetSamplingMode(PSP_CTRL_MODE_ANALOG);

    SceCtrlData prev_pad = {0};

    while (g_running) {
        SceCtrlData pad;
        sceCtrlReadBufferPositive(&pad, 1);

        // Edge detection: buttons newly pressed this frame
        unsigned int newly_pressed = ~prev_pad.Buttons & pad.Buttons;

        pspDebugScreenSetXY(0, 0);
        pspDebugScreenPrintf("Controller Demo\n");
        pspDebugScreenPrintf("Buttons held: 0x%08X\n", pad.Buttons);
        pspDebugScreenPrintf("Left stick:   X=%3d Y=%3d\n", pad.Lx, pad.Ly);

        if (newly_pressed & PSP_CTRL_CROSS)
            pspDebugScreenPrintf("Cross JUST pressed!\n");
        if (pad.Buttons & PSP_CTRL_LTRIGGER && pad.Buttons & PSP_CTRL_RTRIGGER)
            pspDebugScreenPrintf("Both triggers held\n");
        if (pad.Buttons & PSP_CTRL_START)
            break;

        prev_pad = pad;
        sceDisplayWaitVblankStart();
    }

    sceKernelExitGame();
    return 0;
}
```

**Button constants (`pspctrl.h`):**

| Constant | Value | Button |
|---|---|---|
| `PSP_CTRL_SELECT` | 0x000001 | Select |
| `PSP_CTRL_START` | 0x000008 | Start |
| `PSP_CTRL_UP` | 0x000010 | D-pad Up |
| `PSP_CTRL_RIGHT` | 0x000020 | D-pad Right |
| `PSP_CTRL_DOWN` | 0x000040 | D-pad Down |
| `PSP_CTRL_LEFT` | 0x000080 | D-pad Left |
| `PSP_CTRL_LTRIGGER` | 0x000100 | L trigger |
| `PSP_CTRL_RTRIGGER` | 0x000200 | R trigger |
| `PSP_CTRL_TRIANGLE` | 0x001000 | Triangle |
| `PSP_CTRL_CIRCLE` | 0x002000 | Circle |
| `PSP_CTRL_CROSS` | 0x004000 | Cross |
| `PSP_CTRL_SQUARE` | 0x008000 | Square |
| `PSP_CTRL_HOME` | 0x010000 | Home (firmware-intercepted) |
| `PSP_CTRL_HOLD` | 0x020000 | Hold switch |

### 14.4 Audio Playback

```c
#include <pspkernel.h>
#include <pspaudio.h>
#include <math.h>

PSP_MODULE_INFO("AudioDemo", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_USER);
PSP_HEAP_SIZE_KB(4096);

#define SAMPLE_COUNT 512   // samples per buffer (must be multiple of 64, max 65472)

static volatile int g_running = 1;
static int exit_cb(int a, int b, void *c) { g_running = 0; return 0; }
static int cb_thread(SceSize a, void *b) {
    int id = sceKernelCreateCallback("Exit", exit_cb, NULL);
    sceKernelRegisterExitCallback(id);
    sceKernelSleepThreadCB();
    return 0;
}

// Generate a single-frequency sine wave into a stereo 16-bit PCM buffer.
static void gen_sine(short *buf, int samples, float freq, float *phase)
{
    for (int i = 0; i < samples; i++) {
        short v = (short)(sinf(*phase) * 28000.0f);
        buf[i * 2]     = v;   // left
        buf[i * 2 + 1] = v;   // right
        *phase += 2.0f * 3.14159f * freq / 44100.0f;
        if (*phase > 2.0f * 3.14159f) *phase -= 2.0f * 3.14159f;
    }
}

int main(int argc, char *argv[])
{
    int thid = sceKernelCreateThread("cb", cb_thread, 0x11, 0xFA0, 0, 0);
    if (thid >= 0) sceKernelStartThread(thid, 0, NULL);

    // Reserve channel 0, stereo, SAMPLE_COUNT samples per callback
    int ch = sceAudioChReserve(0, SAMPLE_COUNT, PSP_AUDIO_FORMAT_STEREO);
    if (ch < 0) sceKernelExitGame();

    // Stereo buffer: SAMPLE_COUNT × 2 channels × 2 bytes per sample
    static short __attribute__((aligned(64))) audio_buf[SAMPLE_COUNT * 2];
    float phase = 0.0f;

    while (g_running) {
        gen_sine(audio_buf, SAMPLE_COUNT, 440.0f, &phase);
        // Blocking output: waits until previous buffer is consumed before returning.
        sceAudioOutputBlocking(ch, PSP_AUDIO_VOLUME_MAX, audio_buf);
    }

    sceAudioChRelease(ch);
    sceKernelExitGame();
    return 0;
}
```

**Audio constraints:**
- Sample count must be a multiple of 64 and in the range [64, 65472].
- Audio buffer must be 64-byte aligned.
- `PSP_AUDIO_VOLUME_MAX` = `0x8000` (32768).
- 8 hardware PCM channels (0–7); pass `-1` to auto-assign.

---

## 15. Mapping C# Bindings to C Code

Every C# declaration in this repository corresponds directly to a C SDK function.
This table shows the mapping for the most commonly used functions.

| C (native PSP) | C# Native layer | C# Public API |
|---|---|---|
| `sceDisplaySetMode(int,int,int)` | `DisplayNative.SetMode(int,int,int)` | `DisplayModule.SetMode(DisplayMode,int,int)` |
| `sceDisplaySetFrameBuf(void*,int,int,int)` | `DisplayNative.SetFrameBuf(void*,...)` | `DisplayModule.SetFrameBuf(uint[],...)` |
| `sceDisplayWaitVblankStart()` | `DisplayNative.WaitVblankStart()` | `DisplayModule.WaitVblankStart()` → `PspResult` |
| `sceDisplayIsVblank()` → int | `DisplayNative.IsVblank()` → int | `DisplayModule.IsVblank()` → bool |
| `sceCtrlSetSamplingMode(int)` | `CtrlNative.SetSamplingMode(int)` | `CtrlModule.SetSamplingMode(SamplingMode)` |
| `sceCtrlReadBufferPositive(SceCtrlData*,int)` | `CtrlNative.ReadBufferPositive(ref CtrlData,int)` | `CtrlModule.ReadBufferPositive()` → `PspResult<CtrlData>` |
| `sceCtrlReadLatch(SceCtrlLatch*)` | `CtrlNative.ReadLatch(out CtrlLatch)` | `CtrlModule.ReadLatch()` → `PspResult<CtrlLatch>` |
| `sceAudioChReserve(int,int,int)` → channelId | `AudioNative.ChReserve(int,int,int)` → int | `AudioModule.ReserveChannel(...)` → `PspResult<int>` |
| `sceAudioOutputBlocking(int,int,void*)` | `AudioNative.OutputBlocking(int,int,IntPtr)` | `AudioModule.OutputBlocking(int,IntPtr,int)` |
| `sceGuInit()` | `GuNative.Init()` | `GuModule.Init()` |
| `sceGuDrawArray(int,int,int,void*,void*)` | `GuNative.DrawArray(int,int,int,void*,void*)` | `GuModule.DrawArray<TVertex>(...)` |
| `sceGumMatrixMode(int)` | `GumNative.MatrixMode(int)` | `GumModule.MatrixMode(MatrixMode)` |
| `sceGumTranslate(ScePspFVector3*)` | `GumNative.Translate(ref PspFVector3)` | `GumModule.Translate(PspFVector3)` |

**Reading a C header alongside a C# Native file:**

The C declaration:
```c
int sceCtrlReadBufferPositive(SceCtrlData *pad_data, int count);
```

Becomes, in `CtrlNative.cs`:
```csharp
[DllImport("sceCtrl",
    EntryPoint        = "sceCtrlReadBufferPositive",
    CallingConvention = CallingConvention.Cdecl)]
internal static extern int ReadBufferPositive(ref CtrlData padData, int count);
```

The only transformation rules are:
1. `SceCtrlData*` → `ref CtrlData` (safe) or `CtrlData*` (unsafe)
2. `void*` → `IntPtr` (safe) or `void*` (unsafe)
3. `int` return → check with `PspException.ThrowIfError` or wrap in `PspResult`
4. C function name → `EntryPoint` string; C# method name is free

---

## 16. Common Build Problems

**`psp-config: command not found`**
The toolchain is not on `PATH`. Ensure `export PATH=$PATH:$PSPDEV/bin` is in your
shell profile and you have opened a new terminal session after editing it.

**`Cannot open linker script file prxspecs: No such file`**
`$(PSPSDK)` is empty. Run `psp-config --pspsdk-path` to verify, and make sure
`PSPDEV` is set before invoking `make`.

**Undefined reference to `sceGuInit` (or any `sce*` function)**
The required library is missing from `LIBS`. Check Section 17 for which library
each function lives in. Common mistake: linking `-lpspgu` but forgetting `-lpspge`.

**`EBOOT.PBP` not generated**
`EXTRA_TARGETS` is not set to `EBOOT.PBP` in the Makefile.

**Black screen on PPSSPP / hardware**
1. Confirm `sceGuDisplay(GU_TRUE)` is called after the initial `sceGuSync`.
2. Confirm `sceDisplayWaitVblankStart` is inside the game loop before `sceGuSwapBuffers`.
3. Check that VRAM buffer addresses are not overlapping (each buffer needs its own
   offset; the `vram_alloc` helper in Section 6.4 tracks this).

**Crash at startup on hardware (works in PPSSPP)**
PPSSPP is more lenient with unaligned memory access and stack overflows.
1. Check `PSP_MAIN_THREAD_STACK_SIZE_KB` — the default (256 KB) may not be enough.
2. Check that the display list array has `__attribute__((aligned(16)))`.
3. Check that vertex arrays have `__attribute__((aligned(16)))`.
4. Ensure VRAM offset does not exceed 4 MB.

**Struct size mismatch (`Marshal.SizeOf` wrong in C# tests)**
Run `dotnet test` to check. If `CtrlData` is not 20 bytes, a `fixed byte` field is
missing or the wrong size. See `Ctrl/CtrlStructs.cs` for the reference layout.

**`PSP_LARGE_MEMORY` program crashes on PSP-1000**
PSP-1000 has only 32 MB. Remove `PSP_LARGE_MEMORY(1)` or add a runtime model check
with `sceKernelGetModel()` before allocating large buffers.

---

## 17. Library Reference

Link libraries in the order shown (dependents before dependencies).

| Library flag | Functions | Include |
|---|---|---|
| `-lpspgu` | `sceGu*` — graphics utility | `<pspgu.h>` |
| `-lpspgum` | `sceGum*` — matrix math | `<pspgum.h>` |
| `-lpspdisplay` | `sceDisplay*` | `<pspdisplay.h>` |
| `-lpspge` | `sceGe*` — graphics engine | `<pspge.h>` |
| `-lpspctrl` | `sceCtrl*` — controller | `<pspctrl.h>` |
| `-lpspaudio` | `sceAudio*` — audio | `<pspaudio.h>` |
| `-lpsprtc` | `sceRtc*` — real-time clock | `<psprtc.h>` |
| `-lpsppower` | `scePower*` — power management | `<psppower.h>` |
| `-lpspwlan` | `sceNet*` / WiFi | `<pspwlan.h>` |
| `-lpspusb` | `sceUsb*` — USB | `<pspusb.h>` |
| `-lpsputility` | `sceUtility*` — system dialogs | `<psputility.h>` |
| `-lpspumd` | `sceUmd*` — UMD drive | `<pspumd.h>` |
| `-lpspdebug` | `pspDebugScreen*` | `<pspdebug.h>` |
| `-lm` | `sinf`, `cosf`, `sqrtf`, etc. | `<math.h>` |
| `-lpspuser` | core kernel (threads, memory, I/O) | `<pspkernel.h>` |
| `-lc` | C standard library | `<stdio.h>`, etc. |

Always put `-lpspuser` and `-lc` last in the `LIBS` list. `-lm` should come before
`-lc`. Other libraries are order-independent relative to each other unless one
depends on another.

---

---

## 18. Running C# on the PSP

This is the section the rest of the document left implicit. Everything in Sections 1–17
describes either the C# binding layer (which runs on your desktop) or native PSP C code
(which runs on the PSP). This section closes that gap: **how to compile C# source and
actually execute it on PSP hardware or PPSSPP**.

### 18.1 The Core Problem

The PSP ships no .NET runtime. `[DllImport]` and P/Invoke are features of the Common
Language Runtime (CLR). For a P/Invoke call like

```csharp
DisplayModule.SetMode(DisplayMode.Lcd, 480, 272);
```

to reach the real `sceDisplaySetMode` function inside the PSP firmware, a .NET runtime
must first be running on the PSP itself — receiving the managed call, marshaling
parameters, and issuing a native MIPS function call.

There are three viable approaches to achieve this, ordered from least to most work:

| Approach | Runtime on PSP? | C# version support | Performance | Effort |
|---|---|---|---|---|
| A — Mono interpreted | Yes (Mono) | .NET Framework 2–4 / Mono 6 | Moderate (JIT) | Medium |
| B — IL2CPP pipeline | No (AOT native binary) | Most C# features | Native MIPS speed | High |
| C — NativeAOT + MIPS backend | No (AOT native binary) | Full .NET 8 | Native MIPS speed | Very high (research) |

---

### 18.2 Approach A — Mono on PSP (Proven Path)

Mono is the open-source .NET runtime. It has been cross-compiled for PSP's MIPS R4000
processor. Your C# assembly (`.dll`) is copied to the memory stick alongside a stripped
Mono interpreter binary; Mono JITs or interprets the IL at boot time on the PSP.

#### How P/Invoke works in this model

```
Your C# code
    ↓  compiles to .NET IL (.dll)
Mono runtime (running on PSP)
    ↓  at runtime, marshals P/Invoke calls
PSP kernel (sceDisplay*, sceGu*, sceCtrl*, …)
```

Mono's P/Invoke engine reads the `[DllImport("sceDisplay")]` attribute, locates the
native export in the PSP firmware, marshals parameters from managed to native calling
convention, and invokes the function. This is exactly the same mechanism used on Linux
where Mono calls into `libc.so`.

#### Step 1 — Build Mono for MIPS PSP

The canonical starting point is the `mono-psp` fork. Because it targets old Mono
versions, build it inside an older Ubuntu or Debian container to avoid toolchain
compatibility issues.

```bash
# Prerequisites on the build host (the pspdev toolchain must already be installed)
export PSPDEV=/usr/local/pspdev
export PATH=$PATH:$PSPDEV/bin

# Clone the PSP Mono port
git clone https://github.com/espes/mono-psp.git
cd mono-psp

# The repository contains a patch set and build script.
# Follow its README; the rough sequence is:
./autogen.sh
./configure \
    --host=psp \
    --prefix=$PSPDEV/psp \
    --disable-shared \
    --enable-static \
    --with-gc=none \
    --without-sigaltstack \
    --disable-parallel-mark
make
make install
```

This produces:
- `$PSPDEV/psp/bin/mono` — the Mono interpreter ELF (to be packaged into EBOOT.PBP)
- `$PSPDEV/psp/lib/mono/` — the base class library DLLs (`mscorlib.dll`, etc.)

> **Note:** Building Mono for PSP requires the pspdev cross-toolchain plus autoconf,
> libtool, and pkg-config on the host. The process can take 30–90 minutes.

#### Step 2 — Compile your C# project

On your desktop, compile the binding library and your game project to .NET IL:

```bash
# Build the PspSdk binding library
dotnet build src/PspSdk/PspSdk.csproj -c Release -o out/

# Build your game (which references PspSdk)
dotnet build src/MyGame/MyGame.csproj -c Release -o out/
```

This produces `out/PspSdk.dll` and `out/MyGame.dll` (standard .NET IL assemblies).

#### Step 3 — Write the C bootstrap

Mono needs a thin C entry point that initialises the runtime and hands off to your C#
assembly. This C file is compiled with `psp-gcc` and becomes the native EBOOT.PBP
executable; it then boots Mono, which JITs and runs your C# `Main` method.

```c
// bootstrap.c — compiled with psp-gcc, links mono-psp
#include <pspkernel.h>
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-config.h>

PSP_MODULE_INFO("MyGame", PSP_MODULE_USER, 1, 0);
PSP_MAIN_THREAD_ATTR(THREAD_ATTR_VFPU | THREAD_ATTR_USER);
PSP_HEAP_SIZE_KB(49152);   // 48 MB — Mono needs substantial heap

static volatile int g_running = 1;
static int exit_cb(int a, int b, void *c) { g_running = 0; return 0; }
static int cb_thread(SceSize a, void *b) {
    int id = sceKernelCreateCallback("Exit", exit_cb, NULL);
    sceKernelRegisterExitCallback(id);
    sceKernelSleepThreadCB();
    return 0;
}

int main(int argc, char *argv[])
{
    int thid = sceKernelCreateThread("cb", cb_thread, 0x11, 0xFA0, 0, 0);
    if (thid >= 0) sceKernelStartThread(thid, 0, NULL);

    // Tell Mono where to find the base class libraries on the memory stick
    mono_set_dirs("ms0:/PSP/GAME/MyGame/lib",   // lib path
                  "ms0:/PSP/GAME/MyGame/etc");   // config path

    // Initialise the JIT domain
    MonoDomain *domain = mono_jit_init("ms0:/PSP/GAME/MyGame/MyGame.dll");
    if (!domain) sceKernelExitGame();

    // Load and execute the assembly's Main method
    MonoAssembly *assembly = mono_domain_assembly_open(
        domain, "ms0:/PSP/GAME/MyGame/MyGame.dll");
    if (!assembly) { mono_jit_cleanup(domain); sceKernelExitGame(); }

    mono_jit_exec(domain, assembly, argc, argv);
    mono_jit_cleanup(domain);

    sceKernelExitGame();
    return 0;
}
```

#### Step 4 — Build the bootstrap with psp-gcc

```makefile
# Makefile for the bootstrap
TARGET        = MyGame
OBJS          = bootstrap.o

INCDIR        = $(PSPDEV)/psp/include/mono-2.0
LIBDIR        = $(PSPDEV)/psp/lib
CFLAGS        = -O2 -G0 -Wall

# Link order: mono first, then PSP modules, then C runtime
LIBS = -lmono-2.0 \
       -lpspgu -lpspgum -lpspdisplay -lpspctrl -lpspge \
       -lpspaudio -lm -lpspuser -lc

EXTRA_TARGETS = EBOOT.PBP
PSP_EBOOT_TITLE = My C# Game
PSP_HEAP_SIZE   = 50331648   # 48 MB in bytes

PSPSDK = $(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
```

```bash
make
```

#### Step 5 — Assemble the memory stick layout

```
ms0:/PSP/GAME/MyGame/
├── EBOOT.PBP              ← bootstrap binary (mono launcher)
├── MyGame.dll             ← your compiled C# assembly
├── PspSdk.dll             ← the C# binding library
├── lib/
│   ├── mscorlib.dll       ← Mono base class library (from mono-psp build)
│   ├── System.dll
│   └── System.Core.dll
└── etc/
    └── mono/
        └── config         ← Mono DllMap config (see below)
```

#### Step 6 — DllMap configuration

Mono's `DllMap` tells the P/Invoke engine how to resolve library names on PSP. The
PSP firmware functions are not in separate `.so` files — they are kernel exports.
You create a thin native stub library that re-exports the kernel symbols so Mono can
find them, or use a `config` file that maps the logical name to the PRX module name.

```xml
<!-- ms0:/PSP/GAME/MyGame/etc/mono/config -->
<configuration>
    <dllmap dll="sceDisplay"  target="ms0:/PSP/GAME/MyGame/native/display_stub.prx"/>
    <dllmap dll="sceCtrl"     target="ms0:/PSP/GAME/MyGame/native/ctrl_stub.prx"/>
    <dllmap dll="sceGu"       target="ms0:/PSP/GAME/MyGame/native/gu_stub.prx"/>
    <dllmap dll="sceGum"      target="ms0:/PSP/GAME/MyGame/native/gum_stub.prx"/>
    <dllmap dll="sceAudio"    target="ms0:/PSP/GAME/MyGame/native/audio_stub.prx"/>
</configuration>
```

Each stub PRX is a minimal C file compiled as a PRX module that simply re-exports the
kernel functions under the same names, making them discoverable as a shared library:

```c
// display_stub.c — compiled as PRX, exports sceDisplay* functions
#include <pspdisplay.h>
// No code needed — the linker re-exports the kernel stubs
// when BUILD_PRX=1 and the PSP SDK is linked.
```

```makefile
TARGET    = display_stub
OBJS      = display_stub.o
BUILD_PRX = 1
LIBS      = -lpspdisplay -lpspuser -lc
PSPSDK    = $(shell psp-config --pspsdk-path)
include $(PSPSDK)/lib/build.mak
```

Build all stub PRXs, place them in `ms0:/PSP/GAME/MyGame/native/`.

#### Step 7 — Your C# game code

With the infrastructure above in place, your C# code calls PSP functions through the
binding layer exactly as shown in Section 14, but now it genuinely runs on the PSP:

```csharp
// MyGame/Program.cs
using PspSdk.Display;
using PspSdk.Ctrl;
using PspSdk.Gu;
using PspSdk.Gum;
using PspSdk.Core;

// Entry point called by mono_jit_exec via bootstrap.c
class Program
{
    static void Main(string[] args)
    {
        DisplayModule.SetMode(DisplayMode.Lcd, 480, 272);

        GuModule.Init();
        // ... full GU setup matching Section 6.6 ...

        CtrlModule.SetSamplingMode(PspSdk.Ctrl.SamplingMode.Analog);

        float angle = 0f;

        while (true)
        {
            if (!CtrlModule.ReadBufferPositive().TryGetValue(out var pad))
                continue;
            if (pad.IsPressed(PspSdk.Ctrl.CtrlButtons.Start))
                break;

            GuModule.Clear(GuClearFlags.Color | GuClearFlags.Depth);

            GumModule.MatrixMode(PspSdk.Gum.MatrixMode.Model);
            GumModule.LoadIdentity();
            GumModule.Translate(new PspFVector3(0, 0, -4f));
            GumModule.RotateY(angle);

            // ... draw calls ...

            DisplayModule.WaitVblankStart();
            GuModule.SwapBuffers();
            angle += 0.02f;
        }

        GuModule.Term();
    }
}
```

Compile targeting a Mono-compatible profile:
```bash
# .NET Framework 4.x profile — compatible with Mono 6 on PSP
dotnet build -c Release -f net48 src/MyGame/MyGame.csproj -o out/
```

---

### 18.3 Approach B — IL2CPP Pipeline (No Runtime on PSP)

IL2CPP converts .NET IL bytecode to C++ source code. That C++ is then compiled with
`psp-g++`. The result is a fully native MIPS binary — no runtime on the PSP, maximum
performance, but no reflection or dynamic code generation.

Unity uses IL2CPP for PlayStation Vita (another Sony MIPS-based handheld), proving the
concept is sound. Setting it up for PSP requires using IL2CPP as a standalone tool.

#### Pipeline overview

```
C# source (.cs)
    ↓  dotnet build → .NET IL (.dll)
IL2CPP tool
    ↓  IL → C++ source files
psp-g++ (from pspdev toolchain)
    ↓  C++ → MIPS ELF
pack-pbp
    ↓  MIPS ELF → EBOOT.PBP
```

#### Step 1 — Obtain IL2CPP as a standalone tool

IL2CPP is distributed as part of the Unity Editor. Extract it from a Unity installation:

```
# Windows Unity install path:
C:\Program Files\Unity\Hub\Editor\<version>\Editor\Data\il2cpp\

# macOS Unity install path:
/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/il2cpp/

# Linux Unity install path:
~/Unity/Hub/Editor/<version>/Editor/Data/il2cpp/
```

Key binaries:
- `il2cpp.exe` / `il2cpp` — the IL-to-C++ converter
- `libil2cpp/` — the runtime support library (C++ source, must be compiled for PSP)

Alternatively, use the open-source **il2cpp_rs** or **il2cpptool** community projects,
though coverage of C# features is more limited.

#### Step 2 — Compile your C# assembly to IL

```bash
dotnet publish src/MyGame/MyGame.csproj \
    -c Release \
    -r linux-x64 \            # any RID; we only need the IL, not the native output
    --self-contained false \
    -o publish/
```

This produces `publish/MyGame.dll` (pure .NET IL).

#### Step 3 — Run IL2CPP to generate C++ source

```bash
# Path to IL2CPP from Unity installation
IL2CPP=/path/to/unity/il2cpp/build/deploy/net471/il2cpp.exe

mono $IL2CPP \
    --convert-to-cpp \
    --generatedcppdir=generated_cpp \
    --assembly=publish/MyGame.dll \
    --assembly=publish/PspSdk.dll \
    --dotnetprofile=unityaot
```

This writes C++ source files to `generated_cpp/`.

#### Step 4 — Compile libil2cpp for PSP

The IL2CPP runtime support library must be compiled for MIPS:

```bash
# Point at the libil2cpp source from Unity
LIBIL2CPP_SRC=/path/to/unity/il2cpp/libil2cpp

mkdir build_libil2cpp && cd build_libil2cpp

# Cross-compile every .cpp file in libil2cpp for PSP
psp-g++ -O2 -G0 -fno-exceptions -fno-rtti \
    -I$LIBIL2CPP_SRC \
    -I$PSPDEV/psp/sdk/include \
    -DPSP -DRUNTIME_IL2CPP \
    $(find $LIBIL2CPP_SRC -name "*.cpp") \
    -c

psp-ar rcs libil2cpp.a *.o
```

#### Step 5 — Compile the generated C++ and link

```bash
# Compile IL2CPP-generated C++
psp-g++ -O2 -G0 -fno-exceptions -fno-rtti \
    -I$LIBIL2CPP_SRC \
    -I$PSPDEV/psp/sdk/include \
    -DPSP \
    generated_cpp/*.cpp \
    -c

# Link everything together
psp-gcc \
    *.o \
    build_libil2cpp/libil2cpp.a \
    bootstrap.o \           # same exit-callback bootstrap from Approach A
    -L$PSPDEV/psp/sdk/lib \
    -L$PSPDEV/psp/lib \
    -lpspgu -lpspgum -lpspdisplay -lpspctrl -lpspge \
    -lpspaudio -lm -lpspuser -lc \
    -o MyGame.elf

psp-fixup-imports MyGame.elf
psp-strip MyGame.elf -o MyGame_stripped.elf
mksfoex -d MEMSIZE=1 "My C# Game" PARAM.SFO
pack-pbp EBOOT.PBP PARAM.SFO NULL NULL NULL NULL NULL MyGame_stripped.elf NULL
```

#### IL2CPP limitations on PSP

| Feature | Status |
|---|---|
| Value types, structs, enums | ✓ Fully supported |
| Generics | ✓ Supported (AOT-specialized) |
| Delegates, events | ✓ Supported |
| `unsafe` code, `fixed` | ✓ Supported |
| Reflection (`typeof`, `GetType`) | ⚠ Limited — IL2CPP strips unused metadata |
| `Assembly.Load`, `Activator.CreateInstance` | ✗ Not supported (no dynamic loading) |
| `async`/`await` | ⚠ Supported but generates significant code |
| P/Invoke (`[DllImport]`) | ✓ Fully supported — this is how the bindings work |

Because P/Invoke is fully supported, **all of the `[DllImport]` declarations in the
`Native/` sub-namespaces work unchanged** with IL2CPP. The binding layer requires no
modification for this approach.

---

### 18.4 Approach C — .NET NativeAOT with MIPS Backend (Research)

.NET 8's NativeAOT compiler (`dotnet publish -r <rid> --aot`) compiles C# directly to
native machine code using LLVM. PSP's MIPS R4000 is a valid LLVM target (`mipsel`).
However, .NET's NativeAOT does not currently have an official MIPS Runtime Identifier
or a ported `System.Runtime.InteropServices` P/Invoke stack for MIPS.

This is a research-level effort. The outline:

1. Add `mipsel-unknown-elf` as a new RID in the .NET source tree.
2. Port the NativeAOT PAL (Platform Abstraction Layer) to PSP's MIPS ABI.
3. Implement the PSP-specific `Thread`, `GC`, and `DllImport` PAL stubs.
4. Build the .NET runtime libraries for the new RID.
5. Publish with `dotnet publish -r mipsel-psp --aot`.

The resulting binary would be a fully native MIPS ELF with zero managed overhead.
This approach would produce the best performance and fully modern C# support, but
it requires contributing to the .NET runtime itself.

---

### 18.5 Recommended Development Workflow (Today)

Given the current state of tooling, the most productive workflow is:

```
┌─────────────────────────────────────┐
│  Design phase (desktop)             │
│  Write & test logic in C# (dotnet)  │
│  Use PspResult<T>, CtrlModule, etc. │
│  Run: dotnet test                   │
└──────────────┬──────────────────────┘
               │ translate to C
               ▼
┌─────────────────────────────────────┐
│  Implementation phase (PSP C)       │
│  Mirror the C# logic in C/C++       │
│  The C# bindings are your API map   │
│  Build: make → EBOOT.PBP            │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  Test phase                         │
│  Load EBOOT.PBP in PPSSPP           │
│  Iterate; deploy to hardware        │
│  Debug with PSPLink + psp-addr2line │
└─────────────────────────────────────┘
```

Use the C# project for:
- Rapid prototyping of game logic (runs instantly on desktop)
- Struct layout verification (`Marshal.SizeOf` tests)
- API documentation (the `[DllImport]` declarations are a 1:1 map of the C headers)
- Build tooling (asset processors, level editors, code generators)

Translate to C when you need the code to run on PSP hardware today without a
Mono or IL2CPP pipeline in place.

---

### 18.6 Full Build Pipeline Summary

The table below shows every step for each approach, from C# source to EBOOT.PBP.

| Step | Approach A (Mono) | Approach B (IL2CPP) | Approach C (NativeAOT) |
|---|---|---|---|
| 1 | `dotnet build` → `.dll` (IL) | `dotnet build` → `.dll` (IL) | `dotnet publish --aot` |
| 2 | Copy `.dll` to memstick | `il2cpp --convert-to-cpp` → C++ | — (done in step 1) |
| 3 | `psp-gcc bootstrap.c -lmono` → ELF | `psp-g++ generated_cpp/` → `.o` | N/A (no MIPS target yet) |
| 4 | `pack-pbp` → `EBOOT.PBP` | Link + `pack-pbp` → `EBOOT.PBP` | — |
| 5 | Copy `mscorlib.dll` etc. to memstick | Only `EBOOT.PBP` needed | — |
| Runtime on PSP | Mono JIT (interprets IL) | None (native binary) | None (native binary) |
| P/Invoke works? | Yes (Mono P/Invoke engine) | Yes (IL2CPP native P/Invoke) | Yes (NativeAOT P/Invoke) |
| C# feature coverage | .NET Framework / Mono 6 | Most features (no reflection) | Full .NET 8 (theoretical) |

---

---

## 19. Approach D — PSPDNA (DotNetAnywhere on PSP)

PSPDNA is the most practical path available today for running real C# code on PSP
hardware without using Mono or IL2CPP. It is a port of the
[DotNetAnywhere (DNA)](https://github.com/chrisdunelm/DotNetAnywhere) micro .NET
runtime to the PSP, maintained at https://github.com/memsom/PSPDNA.

---

### 19.1 What is DotNetAnywhere (DNA)?

DNA is a .NET Common Language Runtime (CLR) written entirely in portable C. Unlike
Mono, which reimplements the full CLR specification, DNA was designed from the start
to run on severely resource-constrained embedded devices where Mono cannot fit.

| Property | DNA | Mono | IL2CPP |
|---|---|---|---|
| Runtime written in | Pure C | C + platform assembly | C++ (ahead-of-time) |
| Execution model | Interpreter (direct-threaded) | JIT + AOT | AOT compiled binary |
| On-device binary size | ~500 KB | ~20–30 MB | ~5–10 MB |
| RAM footprint | ~1–2 MB minimum | ~8 MB minimum | No runtime needed |
| C# features | Subset (C# 7.3, no multi-dim arrays) | Full .NET 4 | Most features |
| Portability | Very high (only needs C compiler) | Medium | Tied to IL2CPP toolchain |
| Original purpose | Embedded / IoT devices | General purpose | Game engine (Unity) |

DNA pioneered the idea of interpreting .NET CIL (Common Intermediate Language)
bytecode in a direct-threaded interpreter. Interestingly, DNA's approach to running
IL in a browser context later influenced the early Blazor prototype.

**PSPDNA** is memsom's consolidation of DNA improvements (including patches from the
Blazor fork) with a PSP-specific port layer. It adds:
- A `corlib/Psp/` namespace exposing PSP hardware through C# APIs
- PSP-specific native implementations in `native/Psp.*.c`
- A `pspbuild.mak` that integrates with the pspdev toolchain
- Working sample applications: `testSimple`, `tet` (Tetris), `flappy`, `rockbound2`

---

### 19.2 How PSPDNA Differs from Approaches A, B, C

| | A (Mono) | B (IL2CPP) | C (NativeAOT) | **D (PSPDNA)** |
|---|---|---|---|---|
| Runtime on PSP | Mono JIT | None | None | DNA interpreter |
| C# → PSP pipeline | Copy `.dll`, ship Mono | Generate C++, compile | Theoretical | Compile `.dll`, ship DNA |
| Runtime size | 20–30 MB | 0 (baked in) | 0 (baked in) | ~500 KB |
| Setup complexity | High (build Mono for MIPS) | Very high (IL2CPP toolchain) | Research | Medium |
| Toolchain needed | pspdev + Mono cross-build | pspdev + Unity IL2CPP | pspdev + .NET source | pspdev + .NET SDK |
| Supported C# | Mono-level | Most C# features | Full .NET 8 (theoretical) | C# 7.3 subset |
| Self-contained | No (separate DLLs on memstick) | Yes | Yes | Semi (DNA binary + DLL) |
| Status today | Complex, old Mono port | Unity license required | Not viable yet | Works, active samples |

PSPDNA is the sweet spot for "I want to write and run real C# on PSP right now."

---

### 19.3 Architecture Deep Dive

The PSPDNA system has four layers. Understanding each is required for extending it.

```
┌─────────────────────────────────────────────────────────┐
│  Your C# code  (.cs files, compiled to .dll CIL)        │
├─────────────────────────────────────────────────────────┤
│  corlib  (custom standard library, compiled to .dll)    │
│  System.*, System.Collections.Generic.*, corlib/Psp/*  │
├─────────────────────────────────────────────────────────┤
│  DNA runtime  (compiled to MIPS native by psp-gcc)      │
│  CLIFile → MetaData → JIT → JIT_Execute → Heap          │
├─────────────────────────────────────────────────────────┤
│  PSP kernel  (sceGu*, sceCtrl*, sceDisplay*, etc.)      │
└─────────────────────────────────────────────────────────┘
```

#### Layer 1 — CIL assembly loading (`CLIFile.c`)

When the EBOOT.PBP boots, the DNA runtime opens your `.dll` from the memory stick.
`CLIFile.c` reads the PE/COFF + CLI header, locates the metadata streams (strings heap,
blob heap, GUID heap, typed data tables), and maps the entire assembly into DNA's heap.
The `.dll` is a standard .NET assembly — produced by `dotnet build` or `mcs` with
`-nostdlib` — so any .NET SDK on your desktop can produce it.

#### Layer 2 — Metadata parsing (`MetaData.c`)

DNA walks the CLI metadata tables (TypeDef, MethodDef, FieldDef, MemberRef, etc.) to
build its internal type system. All type information is resolved at load time, not at
execution time, which is why DNA can be an interpreter without a full class-loader on
every method call.

#### Layer 3 — JIT compilation (`JIT.c`)

Despite the name, DNA's "JIT" does not emit machine code. It performs:
1. **Stack type analysis** — tracks the managed type of every value on the evaluation
   stack at every bytecode offset
2. **Opcode annotation** — embeds type information directly into an internal opcode
   representation, eliminating type checks at runtime
3. **Combined opcode generation** (optional, off by default) — merges common opcode
   pairs into single dispatch entries for speed

The result is a typed internal bytecode stored in DNA's heap, ready for interpretation.

#### Layer 4 — Interpreter (`JIT_Execute.c`)

The execution engine uses **direct-threaded interpretation** with computed gotos:

```c
// Each opcode implementation ends with GO_NEXT(), which jumps directly
// to the implementation of the next opcode via a pre-computed table of
// labels — no switch statement, no bounds check, no function call.
#define GO_NEXT() goto **(void**)pCurOp

// Stack operations use typed macros:
PUSH_U32(value);      // push a 32-bit value
POP_U64();            // pop a 64-bit value
DUP();                // duplicate top-of-stack
```

This is the fastest interpretation technique available in standard C. It avoids the
branch misprediction overhead of a switch-based interpreter while remaining fully
portable across MIPS, x86, and ARM.

#### Garbage collector (`Heap.c`)

DNA uses a simple mark-and-sweep collector. Weak references and undeletable objects
are supported. Finalizers run during allocation in the calling thread (not a dedicated
finalizer thread) — a known deviation from the CLR specification.

---

### 19.4 How PSP Hardware Access Works: Internal Calls

PSPDNA uses two mechanisms to let C# code call PSP kernel functions:

#### Mechanism 1 — Internal Calls (primary, recommended)

An **internal call** is a C# method declared `extern` with
`[MethodImpl(MethodImplOptions.InternalCall)]`. At runtime, DNA looks up the method's
fully-qualified name in a static dispatch table (`InternalCall.c`) and calls the
corresponding C function pointer directly. No P/Invoke marshaling overhead.

**C# side:**
```csharp
// corlib/Psp/Display.cs
namespace Psp
{
    public static class Display
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WaitVblankStart();
    }
}
```

**C side (`native/Psp.Display.c`):**
```c
#include <pspdisplay.h>
#include "Psp.Display.h"

// Called by DNA when managed code invokes Psp.Display.WaitVblankStart()
void Psp_Display_WaitVblankStart(HEAP_PTR pThis_)
{
    sceDisplayWaitVblankStart();
}
```

**Registration in `InternalCall.c`:**
```c
// Every internal call must be listed in this table.
// Key: "Namespace.Type::MethodName" exactly as the CLR sees it.
// Value: function pointer to the C implementation.
static const tInternalCall internalCalls[] = {
    { "Psp.Display::WaitVblankStart",  Psp_Display_WaitVblankStart  },
    { "Psp.Controls::PollPad",         Psp_Controls_PollPad         },
    { "Psp.BasicGraphics2::Init",      Psp_BasicGraphics2_Init      },
    // ... all other internal calls
    { NULL, NULL }   // sentinel
};
```

When DNA's interpreter encounters `call Psp.Display::WaitVblankStart`, it scans
`internalCalls`, finds the entry, and invokes `Psp_Display_WaitVblankStart` directly.

#### Mechanism 2 — P/Invoke (secondary, has gaps)

DNA also supports standard `[DllImport]` declarations. This is the same mechanism our
`PspSdk` binding library uses. DNA's P/Invoke engine resolves the library name and
function name at runtime. **However**, the PSPDNA fork inherited P/Invoke gaps from
the Blazor modifications and may partially revert to the original DNA implementation.

This means **our existing `[DllImport]` bindings may or may not work unchanged**.
Section 19.7 explains how to convert them to internal calls as a reliable fallback.

---

### 19.5 The Existing PSP Namespace (corlib/Psp/)

PSPDNA ships a ready-made PSP hardware abstraction in `corlib/Psp/`. These are the
types your C# game code calls:

#### `Psp.BasicGraphics2` — 2D / 3D rendering

```csharp
public static class BasicGraphics2
{
    // Initialise GU + allocate framebuffers
    public static extern void Init();

    // Present the back buffer to the display
    public static extern void SwapBuffers();

    // Clear the back buffer with a solid colour
    public static extern void Clear(Color color);

    // Filled rectangle
    public static extern void DrawRect(int x, int y, int w, int h, Color color);

    // Text rendering (uses an embedded bitmap font)
    public static extern void DrawText(int x, int y, string text, Color color);

    // Bitmap surface loading from memory stick (BMP/PNG)
    public static extern Surface LoadSurface(string filename);

    // Convert a Surface to a hardware texture
    public static extern Texture CreateTexture(Surface surface);

    // Textured quad
    public static extern void DrawTexture(Texture tex, int x, int y, int w, int h);
}
```

All of these are internal calls backed by C implementations in `native/Psp.BasicGraphics2.c`,
which calls `sceGu*` functions from the PSP SDK internally.

#### `Psp.Controls` — controller input

```csharp
public static class Controls
{
    // Read raw pad state (call once per frame)
    public static extern void PollPad();

    // Read latch state (pressed / released since last poll)
    public static extern void PollLatch();

    // Button state queries
    public static extern bool IsKeyDown(PspCtrlButtons button);   // newly pressed
    public static extern bool IsKeyHeld(PspCtrlButtons button);   // held down
    public static extern bool IsKeyUp(PspCtrlButtons button);     // just released

    // Analog stick (-128 to 127)
    public static extern int GetJoystickX();
    public static extern int GetJoystickY();
}

[Flags]
public enum PspCtrlButtons : uint
{
    Select   = 0x000001,
    Start    = 0x000008,
    Up       = 0x000010,
    Right    = 0x000020,
    Down     = 0x000040,
    Left     = 0x000080,
    LTrigger = 0x000100,
    RTrigger = 0x000200,
    Triangle = 0x001000,
    Circle   = 0x002000,
    Cross    = 0x004000,
    Square   = 0x008000,
}
```

#### `Psp.Display` — vsync

```csharp
public static class Display
{
    public static extern void WaitVblankStart();
}
```

#### `Psp.Kernel` — lifecycle

```csharp
public static class Kernel
{
    public static extern void ExitGame();
}
```

---

### 19.6 Build Setup and Full Workflow

#### Prerequisites

- pspdev toolchain (Section 5) — provides `psp-gcc`, `psp-g++`, `pack-pbp`, etc.
- .NET SDK 6.0.x (pinned in `global.json`)
- `csc` (C# compiler, comes with .NET SDK or Mono)

```bash
# Verify
psp-gcc --version      # must be present
dotnet --version       # must be 6.x
```

#### Clone PSPDNA

```bash
git clone https://github.com/memsom/PSPDNA.git
cd PSPDNA
```

#### Repository layout

```
PSPDNA/
├── Makefile           ← top-level orchestrator
├── pspbuild.mak       ← PSP SDK packaging rules
├── global.json        ← pins .NET SDK to 6.0.*
├── corlib/            ← custom standard library (compiled to corlib.dll)
│   ├── corlib.csproj
│   ├── Psp/           ← PSP hardware wrappers (Display.cs, Controls.cs, …)
│   └── System/        ← Core types (Object, String, Int32, List<T>, …)
├── native/            ← DNA runtime C/C++ source (~40 files)
│   ├── CLIFile.c/h    ← assembly loader
│   ├── MetaData.c/h   ← metadata parser
│   ├── JIT.c/h        ← bytecode analyser
│   ├── JIT_Execute.c  ← direct-threaded interpreter
│   ├── Heap.c/h       ← garbage collector
│   ├── InternalCall.c ← internal call dispatch table
│   ├── Psp.Display.c  ← WaitVblankStart → sceDisplayWaitVblankStart
│   ├── Psp.Controls.c ← PollPad → sceCtrlReadBufferPositive
│   └── Psp.BasicGraphics2.c  ← Init/Clear/Draw → sceGu*
├── testSimple/        ← hello-world sample
│   ├── testSimple.csproj
│   └── Program.cs
└── tet/               ← Tetris demo
    ├── tet.csproj
    └── Tet.cs
```

#### Build commands

```bash
# Step 1: compile all C# projects to .dll (runs on your desktop via dotnet/csc)
make managed

# Step 2: compile the DNA runtime + PSP glue in C (runs psp-gcc)
make native

# Step 3: package everything into EBOOT.PBP
make all
```

`make all` performs both steps and then calls `pspbuild.mak` which invokes `mksfoex`
and `pack-pbp` to produce the final `build/EBOOT.PBP`.

#### Memory stick layout after build

```
ms0:/PSP/GAME/MyGame/
├── EBOOT.PBP          ← DNA runtime (native MIPS) + bootstrap
├── corlib.dll         ← compiled standard library
├── MyGame.dll         ← your compiled C# assembly
└── data/              ← any assets loaded at runtime (images, levels, etc.)
```

---

### 19.7 Writing C# for PSPDNA

#### Project file

```xml
<!-- MyGame/MyGame.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- DNA uses .NET 2.0 assembly format -->
    <TargetFramework>net40</TargetFramework>

    <!-- No standard library — DNA ships its own corlib -->
    <NoStdLib>true</NoStdLib>
    <NoCompilerStandardLib>true</NoCompilerStandardLib>

    <!-- Output a plain .dll, not a self-contained exe -->
    <OutputType>Exe</OutputType>

    <!-- Unsafe is required for low-level work -->
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>

    <!-- C# 7.3 is the tested language version -->
    <LangVersion>7.3</LangVersion>
  </PropertyGroup>

  <!-- Reference the custom corlib instead of the BCL -->
  <ItemGroup>
    <Reference Include="corlib">
      <HintPath>../corlib/bin/Release/net40/corlib.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

#### Minimal game loop

```csharp
// MyGame/Program.cs
using Psp;
using System;

class Program
{
    static void Main()
    {
        // Initialise GU, allocate framebuffers, set display mode
        BasicGraphics2.Init();

        float x = 240f;
        float y = 136f;

        while (true)
        {
            // Read controller state
            Controls.PollPad();
            Controls.PollLatch();

            // Exit on Start button
            if (Controls.IsKeyHeld(PspCtrlButtons.Start))
                Kernel.ExitGame();

            // Move a square with the D-pad
            if (Controls.IsKeyHeld(PspCtrlButtons.Right)) x += 2f;
            if (Controls.IsKeyHeld(PspCtrlButtons.Left))  x -= 2f;
            if (Controls.IsKeyHeld(PspCtrlButtons.Down))  y += 2f;
            if (Controls.IsKeyHeld(PspCtrlButtons.Up))    y -= 2f;

            // Render
            BasicGraphics2.Clear(new Color(30, 30, 60));
            BasicGraphics2.DrawRect((int)x - 16, (int)y - 16, 32, 32,
                                    new Color(255, 200, 0));
            BasicGraphics2.DrawText(5, 5, "D-pad to move, START to quit",
                                    new Color(255, 255, 255));

            // Vsync + flip
            Display.WaitVblankStart();
            BasicGraphics2.SwapBuffers();
        }
    }
}
```

This compiles on your desktop (.NET SDK) and runs on PSP via DNA — no change to the
source required.

#### What you can use from corlib

| Feature | Available | Notes |
|---|---|---|
| `int`, `float`, `bool`, `char`, `string` | ✓ | Full support |
| `object`, `ValueType`, `Enum` | ✓ | Full support |
| `List<T>`, `Dictionary<K,V>`, `Queue<T>`, `Stack<T>` | ✓ | Full support |
| `Math.Sin`, `Math.Cos`, `Math.Sqrt` | ✓ | Delegates to C `sinf`/`cosf` |
| `DateTime`, `TimeSpan` | ✓ | Limited formatting |
| `Exception` hierarchy | ✓ | 20+ exception types |
| LINQ (`Select`, `Where`, `OrderBy`) | ⚠ | Most work; `.ThenBy` broken |
| `async`/`await` | ✗ | Not supported |
| Multi-dimensional arrays (`T[,]`) | ✗ | Jagged arrays (`T[][]`) work |
| `Attribute` classes | ✗ | Parsed but not retrievable at runtime |
| `Assembly.Load`, `Type.GetType` | ⚠ | Very limited reflection |
| `Console.WriteLine` | ✓ | Writes to PSP debug screen / serial |
| `File.ReadAllBytes` | ✓ | Reads from memory stick |

---

### 19.8 Extending PSPDNA with New PSP Bindings

The existing `corlib/Psp/` layer covers display, input, vsync, and exit. To expose
additional PSP SDK functions (audio, 3D GU state, RTC, power management), follow
this pattern for each new binding.

#### Step-by-step: Adding `sceAudioChReserve` to PSPDNA

**1. Write the C# declaration in `corlib/Psp/Audio.cs`:**

```csharp
// corlib/Psp/Audio.cs
using System.Runtime.CompilerServices;

namespace Psp
{
    public static class Audio
    {
        public const int FormatStereo = 0x00;
        public const int FormatMono   = 0x10;
        public const int VolumeMax    = 0x8000;

        /// <summary>Reserve a PSP audio channel.</summary>
        /// <param name="channel">Channel 0–7, or -1 to auto-assign.</param>
        /// <param name="sampleCount">Samples per buffer. Multiple of 64, max 65472.</param>
        /// <param name="format">Audio.FormatStereo or Audio.FormatMono.</param>
        /// <returns>Channel ID on success, negative on error.</returns>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int ChReserve(int channel, int sampleCount, int format);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int ChRelease(int channel);

        /// <summary>Output audio samples (blocks until previous buffer consumed).</summary>
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern unsafe int OutputBlocking(int channel, int vol, short* buf);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int SetChannelDataLen(int channel, int sampleCount);
    }
}
```

**2. Write the C implementation in `native/Psp.Audio.c`:**

```c
// native/Psp.Audio.c
#include <pspaudio.h>
#include "Psp.Audio.h"
#include "Types.h"
#include "Heap.h"

// DNA passes arguments via the evaluation stack.
// pThis_ is NULL for static methods.
// Arguments are popped from the managed stack in declaration order.

void Psp_Audio_ChReserve(HEAP_PTR pThis_, I32 channel, I32 sampleCount, I32 format)
{
    // Return value is pushed onto the managed stack by DNA after this returns.
    // For int return, DNA reads the return value from the C return register.
    // The function signature seen by DNA: static extern int ChReserve(int, int, int)
    // so we must return an int via the standard C calling convention.
}

// DNA calls internal calls with a unified signature.
// Use the INTERNAL_CALL macro from InternalCall.h to wrap the real function:
I32 Psp_Audio_ChReserve_impl(I32 channel, I32 sampleCount, I32 format)
{
    return sceAudioChReserve(channel, sampleCount, format);
}

I32 Psp_Audio_ChRelease_impl(I32 channel)
{
    return sceAudioChRelease(channel);
}

// For pointer arguments, DNA passes the heap pointer (or a raw pointer for unsafe code).
I32 Psp_Audio_OutputBlocking_impl(I32 channel, I32 vol, PTR buf)
{
    return sceAudioOutputBlocking(channel, vol, (void*)buf);
}

I32 Psp_Audio_SetChannelDataLen_impl(I32 channel, I32 sampleCount)
{
    return sceAudioSetChannelDataLen(channel, sampleCount);
}
```

**3. Write the header `native/Psp.Audio.h`:**

```c
// native/Psp.Audio.h
#pragma once
#include "Types.h"

I32 Psp_Audio_ChReserve_impl(I32 channel, I32 sampleCount, I32 format);
I32 Psp_Audio_ChRelease_impl(I32 channel);
I32 Psp_Audio_OutputBlocking_impl(I32 channel, I32 vol, PTR buf);
I32 Psp_Audio_SetChannelDataLen_impl(I32 channel, I32 sampleCount);
```

**4. Register in `native/InternalCall.c`:**

```c
#include "Psp.Audio.h"

// Add these entries to the internalCalls[] array:
{ "Psp.Audio::ChReserve",         (fnInternalCall)Psp_Audio_ChReserve_impl         },
{ "Psp.Audio::ChRelease",         (fnInternalCall)Psp_Audio_ChRelease_impl         },
{ "Psp.Audio::OutputBlocking",    (fnInternalCall)Psp_Audio_OutputBlocking_impl    },
{ "Psp.Audio::SetChannelDataLen", (fnInternalCall)Psp_Audio_SetChannelDataLen_impl },
```

**5. Add to the Makefile:**

```makefile
# In native/Makefile or the main Makefile NATIVE_OBJS list:
NATIVE_OBJS += Psp.Audio.o
```

**6. Link `pspaudio` in the PSP link step (`pspbuild.mak`):**

```makefile
LIBS += -lpspaudio
```

**7. Use from C#:**

```csharp
using Psp;
using System;

class AudioDemo
{
    const int SampleCount = 512;

    static unsafe void Main()
    {
        BasicGraphics2.Init();

        int ch = Audio.ChReserve(-1, SampleCount, Audio.FormatStereo);
        if (ch < 0) { Kernel.ExitGame(); return; }

        // Allocate a stereo buffer: SampleCount × 2 channels × 2 bytes
        short[] buf = new short[SampleCount * 2];
        float phase = 0f;

        while (true)
        {
            Controls.PollLatch();
            if (Controls.IsKeyDown(PspCtrlButtons.Start))
                break;

            // Generate a 440 Hz sine wave
            for (int i = 0; i < SampleCount; i++)
            {
                short v = (short)(Math.Sin(phase) * 28000.0);
                buf[i * 2]     = v;
                buf[i * 2 + 1] = v;
                phase += 2.0f * 3.14159f * 440f / 44100f;
                if (phase > 2.0f * 3.14159f) phase -= 2.0f * 3.14159f;
            }

            fixed (short* p = buf)
                Audio.OutputBlocking(ch, Audio.VolumeMax, p);
        }

        Audio.ChRelease(ch);
        Kernel.ExitGame();
    }
}
```

---

### 19.9 Adapting the PspSdk C# Bindings to PSPDNA

The `PspSdk` library in this repository uses `[DllImport]` (standard P/Invoke). PSPDNA
supports P/Invoke but has known gaps. The reliable path is to convert each module's
`Native/` layer from `[DllImport]` to `[MethodImpl(InternalCall)]` and back the calls
from the C side.

#### Conversion pattern

**Before (P/Invoke, works on desktop CLR):**
```csharp
// Display/Native/DisplayNative.cs
[DllImport("sceDisplay", EntryPoint = "sceDisplayWaitVblankStart",
    CallingConvention = CallingConvention.Cdecl)]
internal static extern int WaitVblankStart();
```

**After (internal call, works in PSPDNA):**
```csharp
// Display/Native/DisplayNative.cs
[MethodImpl(MethodImplOptions.InternalCall)]
internal static extern int WaitVblankStart();
```

**Plus:** add a C implementation to `native/` and register it in `InternalCall.c`.

#### Dual-mode approach (compile-time switch)

If you want one codebase that compiles for both desktop testing and PSPDNA, use a
conditional compilation symbol:

```csharp
// Display/Native/DisplayNative.cs
internal static partial class DisplayNative
{
#if PSPDNA
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern int WaitVblankStart();
#else
    [DllImport("sceDisplay", EntryPoint = "sceDisplayWaitVblankStart",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int WaitVblankStart();
#endif
}
```

Define `PSPDNA` in `MyGame.csproj` when building for PSP:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Psp'">
    <DefineConstants>PSPDNA</DefineConstants>
    <NoStdLib>true</NoStdLib>
    <LangVersion>7.3</LangVersion>
</PropertyGroup>
```

Build for desktop (full CLR): `dotnet build -c Debug`
Build for PSP (PSPDNA): `dotnet build -c Psp`

The `PspResult<T>`, `PspException`, struct layouts (`CtrlData`, `CtrlLatch`), and
enums in the binding library require **no modification** — they are pure C# and work
identically in PSPDNA's DNA runtime.

---

### 19.10 Module Mapping: PspSdk Bindings → PSPDNA Internal Calls

The table below shows the exact C internal-call function name for every binding in
the `PspSdk` library, following the `Namespace_Type_Method_impl` naming convention.

| C# module | C# method | Internal call registration key | C function name |
|---|---|---|---|
| `DisplayModule` | `SetMode` | `"PspSdk.Display.DisplayNative::SetMode"` | `PspSdk_Display_SetMode_impl` |
| `DisplayModule` | `WaitVblankStart` | `"PspSdk.Display.DisplayNative::WaitVblankStart"` | `PspSdk_Display_WaitVblankStart_impl` |
| `DisplayModule` | `IsVblank` | `"PspSdk.Display.DisplayNative::IsVblank"` | `PspSdk_Display_IsVblank_impl` |
| `CtrlModule` | `SetSamplingMode` | `"PspSdk.Ctrl.CtrlNative::SetSamplingMode"` | `PspSdk_Ctrl_SetSamplingMode_impl` |
| `CtrlModule` | `ReadBufferPositive` | `"PspSdk.Ctrl.CtrlNative::ReadBufferPositive"` | `PspSdk_Ctrl_ReadBufferPositive_impl` |
| `CtrlModule` | `ReadLatch` | `"PspSdk.Ctrl.CtrlNative::ReadLatch"` | `PspSdk_Ctrl_ReadLatch_impl` |
| `AudioModule` | `ChReserve` | `"PspSdk.Audio.AudioNative::ChReserve"` | `PspSdk_Audio_ChReserve_impl` |
| `AudioModule` | `OutputBlocking` | `"PspSdk.Audio.AudioNative::OutputBlocking"` | `PspSdk_Audio_OutputBlocking_impl` |
| `GuModule` | `Init` | `"PspSdk.Gu.GuNative::Init"` | `PspSdk_Gu_Init_impl` |
| `GuModule` | `Start` | `"PspSdk.Gu.GuNative::Start"` | `PspSdk_Gu_Start_impl` |
| `GuModule` | `DrawArray` | `"PspSdk.Gu.GuNative::DrawArray"` | `PspSdk_Gu_DrawArray_impl` |
| `GumModule` | `MatrixMode` | `"PspSdk.Gum.GumNative::MatrixMode"` | `PspSdk_Gum_MatrixMode_impl` |
| `GumModule` | `Translate` | `"PspSdk.Gum.GumNative::Translate"` | `PspSdk_Gum_Translate_impl` |
| `GumModule` | `RotateY` | `"PspSdk.Gum.GumNative::RotateY"` | `PspSdk_Gum_RotateY_impl` |

Each C function follows the same implementation pattern as shown in Section 19.8 —
one line that calls the real PSP SDK function.

---

### 19.11 PSPDNA Project Structure for a New Game

Starting a new C# game project on top of PSPDNA:

```
MyPspGame/
├── Makefile                 ← delegates to PSPDNA make
├── pspbuild.mak             ← copied from PSPDNA
├── global.json              ← pins .NET SDK 6.0.x
│
├── pspdna/                  ← PSPDNA as a git submodule
│   ├── corlib/
│   ├── native/
│   └── pspbuild.mak
│
├── game/                    ← your C# game
│   ├── game.csproj
│   └── src/
│       ├── Program.cs
│       ├── GameState.cs
│       ├── Renderer.cs
│       └── InputHandler.cs
│
├── extensions/              ← new PSP bindings (internal calls)
│   ├── native/
│   │   ├── Psp.Audio.c      ← new C implementations
│   │   ├── Psp.Audio.h
│   │   └── Psp.Rtc.c
│   └── managed/
│       ├── Psp.Audio.cs     ← new C# declarations (InternalCall)
│       └── Psp.Rtc.cs
│
└── assets/
    ├── sprites/
    └── sounds/
```

**Top-level Makefile:**

```makefile
PSPDNA_DIR = pspdna
GAME_PROJ  = game/game.csproj
EXT_NATIVE = extensions/native/*.c

# Include PSPDNA's PSP build rules
include $(PSPDNA_DIR)/pspbuild.mak

# Additional extension objects
NATIVE_OBJS += $(patsubst extensions/native/%.c,build/%.o,$(wildcard $(EXT_NATIVE)))

managed:
    dotnet build $(GAME_PROJ) -c Release
    dotnet build $(PSPDNA_DIR)/corlib/corlib.csproj -c Release

native: $(NATIVE_OBJS)
    $(MAKE) -C $(PSPDNA_DIR)/native

all: managed native
    $(MAKE) package
```

---

### 19.12 Known Limitations of PSPDNA

These limitations are documented in PSPDNA's `native/Bugs.txt` and must be understood
before committing to this approach:

| Limitation | Impact | Workaround |
|---|---|---|
| Generic methods in generic interfaces fail | `LINQ.ThenBy()` broken | Use `OrderBy()` only; avoid chained sort |
| Nested class fields referencing outer class break type sizing | Crash at JIT time | Keep nested classes field-free; use static members |
| Finalizers run in calling thread | Non-standard GC behaviour | Avoid finalizers; use explicit `Dispose()` |
| Memory leak (location unknown) | Long-running programs grow | Restart every N minutes if needed |
| Multi-dimensional arrays (`T[,]`) not supported | | Use jagged arrays `T[][]` |
| No `async`/`await` | | Use callbacks or coroutine patterns |
| `[Attribute]` classes not retrievable at runtime | | Do not use runtime attribute inspection |
| P/Invoke gaps from Blazor fork | Some `[DllImport]` fail silently | Convert to internal calls (Section 19.9) |
| Pinned to C# 7.3 | No records, no default interface members | Use older patterns |
| `.NET SDK 6.0.x` pinned in `global.json` | Newer SDK may fail | Match SDK version exactly |

---

### 19.13 Comparison: All Four Approaches

| | A — Mono | B — IL2CPP | C — NativeAOT | **D — PSPDNA** |
|---|---|---|---|---|
| C# → PSP today | Hard | Hard + Unity license | Not viable | **Yes, works now** |
| Runtime size | 20–30 MB | 0 | 0 | **~500 KB** |
| Performance | JIT | Native | Native | Interpreted |
| Full C# support | .NET 4 / Mono | Most features | Full .NET 8 (theoretical) | C# 7.3 subset |
| P/Invoke from C# | Yes (full) | Yes (full) | Yes (full) | Partial (prefer internal calls) |
| Our PspSdk bindings | Work as-is | Work as-is | Work as-is | Convert to InternalCall |
| GC included | Yes | Yes (Boehm) | Yes | Yes (mark/sweep) |
| Setup time | Days | Days | Weeks+ | **Hours** |
| Sample apps | Few | None (Unity only) | None | Tetris, Flappy, more |
| Best for | Max compatibility | Max performance | Future / research | **Learning + real projects now** |

**Recommendation:** Use Approach D (PSPDNA) when you want to ship C# code on PSP
without a Mono build pipeline or Unity licence. The runtime is small, the samples
prove it works, and extending it requires only C and basic Makefile changes.

---

*End of AGENTS.md*
