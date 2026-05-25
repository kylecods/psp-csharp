# pspdna/Makefile
#
# Builds the PSPDNA-based PSP application.
#
# Targets:
#   make managed   — compile all C# to .dll using the .NET SDK / csc
#   make native    — compile C native files with psp-gcc
#   make all       — both above + package into EBOOT.PBP
#   make clean     — remove all generated files
#
# Prerequisites:
#   - pspdev toolchain on PATH (psp-gcc, psp-g++, psp-config, pack-pbp, mksfoex)
#   - .NET SDK 6.0.x (pinned in global.json)
#
# Usage: from the pspdna/ directory, run one of the make targets above.
# The final EBOOT.PBP is placed in build/EBOOT.PBP.

# ── Configuration ─────────────────────────────────────────────────────────
PSPSDK        := $(shell psp-config --pspsdk-path)
TARGET        := pspdna_game
BUILD_DIR     := build
MANAGED_DIR   := $(BUILD_DIR)/managed
NATIVE_DIR    := $(BUILD_DIR)/native

# ── C# compiler ───────────────────────────────────────────────────────────
# We need raw Roslyn csc with -nostdlib, NOT 'dotnet build' (MSBuild).
# 'dotnet build' passes all args to MSBuild; csc flags like -nostdlib are
# unknown to MSBuild and will cause MSB1001 errors.
#
# Install the standalone csc global tool once:
#   dotnet tool install --global Microsoft.Net.Sdk.Compilers
# Then 'csc' is on PATH.  Verify with: csc --version
#
# Alternatively on Linux/WSL, locate the Roslyn binary embedded in the SDK:
#   $(shell dotnet --list-runtimes | ... )
# but the global tool install is simpler.
CSC           := csc
CSC_FLAGS     := -nostdlib -unsafe -optimize+ -langversion:7.3 -warn:3 \
                 -noconfig -nologo

# ── Corlib source discovery ────────────────────────────────────────────────
# Our corlib/ already contains BOTH halves needed for -nostdlib compilation:
#
#   corlib/System/         — PSPDNA primitive types: Object, Int32, Math, …
#   corlib/System.*/       — Collections, IO, Text, Threading, Reflection, …
#   corlib/Psp/            — our PSP wrappers: Audio, Gu, Gum, Controls, …
#   corlib/Properties/     — AssemblyInfo.cs
#
# No external PSPDNA clone is required.  All sources live here.
#
# We use 'find' to collect every .cs under corlib/, excluding MSBuild
# intermediates in obj/ and bin/.
CORLIB_ALL_SRCS := $(shell find corlib -name '*.cs' \
                       -not -path 'corlib/obj/*' \
                       -not -path 'corlib/bin/*')

# ── Native compilation ────────────────────────────────────────────────────
CC            := psp-gcc
CXX           := psp-g++
CFLAGS        := -O2 -G0 -Wall -ffast-math -D__PSP__ \
                 -I$(PSPSDK)/include \
                 -I$(PSPDEV)/psp/include \
                 -Inative

# New native modules introduced by this project
NEW_NATIVE_SRCS := \
    native/Psp.Audio.c     \
    native/Psp.Gu.c        \
    native/Psp.Gum.c

NEW_NATIVE_OBJS := $(patsubst native/%.c,$(NATIVE_DIR)/%.o,$(NEW_NATIVE_SRCS))

# ── Managed assembly paths ────────────────────────────────────────────────
CORLIB_OUT    := $(MANAGED_DIR)/corlib.dll
CUBE3D_SRCS   := samples/Cube3D/Program.cs
CUBE3D_OUT    := $(MANAGED_DIR)/Cube3D.dll
AUDIO_SRCS    := samples/AudioTest/Program.cs
AUDIO_OUT     := $(MANAGED_DIR)/AudioTest.dll

# ── PSP EBOOT packaging ───────────────────────────────────────────────────
PSP_EBOOT_TITLE   := PSPDNA Game
PSP_EBOOT_ICON    := NULL
PSP_EBOOT_PIC0    := NULL

EBOOT         := $(BUILD_DIR)/EBOOT.PBP
PARAM_SFO     := $(BUILD_DIR)/PARAM.SFO

# ── Phony targets ─────────────────────────────────────────────────────────
.PHONY: all managed native clean dirs check-csc

all: dirs managed native package
	@echo ""
	@echo "==================================================="
	@echo "  Build complete: $(EBOOT)"
	@echo "  Copy $(BUILD_DIR)/ to ms0:/PSP/GAME/YourApp/"
	@echo "==================================================="

# ── Directory creation ────────────────────────────────────────────────────
dirs:
	@mkdir -p $(BUILD_DIR) $(MANAGED_DIR) $(NATIVE_DIR)

# ── csc prerequisite check ────────────────────────────────────────────────
check-csc:
ifeq ($(shell command -v $(CSC) 2>/dev/null),)
	@echo ""
	@echo "ERROR: '$(CSC)' not found on PATH."
	@echo ""
	@echo "  Install the standalone Roslyn compiler tool once:"
	@echo "    dotnet tool install --global Microsoft.Net.Sdk.Compilers"
	@echo "  Then 'csc' is available system-wide."
	@echo ""
	@exit 1
endif

# ── Managed compilation ───────────────────────────────────────────────────
# Phase 1: compile corlib (all sources in corlib/ in one -nostdlib shot).
#
# Why one compilation?
#   -nostdlib strips ALL predefined types.  csc special-cases System.Object:
#   it must be compiled IN THE SAME invocation as code that depends on it,
#   not in a referenced .dll.  Since our corlib/ already contains both the
#   System.* primitives (PSPDNA half) and the Psp.* wrappers (our half),
#   we just feed every .cs file in one go.
$(CORLIB_OUT): check-csc $(CORLIB_ALL_SRCS) | dirs
	@echo "[CSC] corlib.dll  ($(words $(CORLIB_ALL_SRCS)) source files)"
	$(CSC) $(CSC_FLAGS) \
	    -target:library \
	    -out:$@ \
	    $(CORLIB_ALL_SRCS)

# Phase 2: compile sample apps referencing corlib.
# Sample code only references Psp.* types, so corlib.dll is the only ref.
$(CUBE3D_OUT): $(CUBE3D_SRCS) $(CORLIB_OUT) | dirs
	@echo "[CSC] Cube3D.dll"
	$(CSC) $(CSC_FLAGS) \
	    -target:exe \
	    -r:$(CORLIB_OUT) \
	    -out:$@ \
	    $(CUBE3D_SRCS)

$(AUDIO_OUT): $(AUDIO_SRCS) $(CORLIB_OUT) | dirs
	@echo "[CSC] AudioTest.dll"
	$(CSC) $(CSC_FLAGS) \
	    -target:exe \
	    -r:$(CORLIB_OUT) \
	    -out:$@ \
	    $(AUDIO_SRCS)

managed: $(CORLIB_OUT) $(CUBE3D_OUT) $(AUDIO_OUT)
	@echo "[managed] All C# assemblies compiled."

# ── Native compilation ────────────────────────────────────────────────────
$(NATIVE_DIR)/%.o: native/%.c | dirs
	@echo "[CC] $<"
	$(CC) $(CFLAGS) -c $< -o $@

native: $(NEW_NATIVE_OBJS)
	@echo "[native] New PSP module objects compiled."
	@echo ""
	@echo "NOTE: These objects must be linked into the PSPDNA native binary."
	@echo "      See AGENTS.md Section 19.6 for the full link command."
	@echo "      Add the entries from native/InternalCall.Additions.c into"
	@echo "      PSPDNA's native/InternalCall.c before building the runtime."

# ── Packaging ─────────────────────────────────────────────────────────────
# Requires the DNA runtime ELF from the PSPDNA build.
# Substitute PSPDNA_ELF with the actual path to the compiled DNA binary.
PSPDNA_ELF ?= ../pspdna_runtime/EBOOT.ELF

package: native managed
	@echo "[SFO] PARAM.SFO"
	mksfoex -d MEMSIZE=1 '$(PSP_EBOOT_TITLE)' $(PARAM_SFO)
	@echo "[PBP] EBOOT.PBP"
	pack-pbp $(EBOOT) $(PARAM_SFO) \
	    $(PSP_EBOOT_ICON) NULL $(PSP_EBOOT_PIC0) NULL NULL \
	    $(PSPDNA_ELF) NULL
	@echo "[PKG] Copying managed assemblies..."
	cp $(MANAGED_DIR)/*.dll $(BUILD_DIR)/

# ── Clean ─────────────────────────────────────────────────────────────────
clean:
	@echo "[clean] Removing $(BUILD_DIR)/"
	rm -rf $(BUILD_DIR)
