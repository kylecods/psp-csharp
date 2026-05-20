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
# Use the csc bundled with .NET SDK 6.0.  'dotnet-script run' cannot
# cross-compile for DNA; we need raw csc with -nostdlib.
CSC           := dotnet-csc
CSC_FLAGS     := -nostdlib -unsafe -optimize+ -langversion:7.3 -warn:3

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

# ── Managed assemblies ────────────────────────────────────────────────────
CORLIB_SRCS   := $(wildcard corlib/Psp/*.cs)
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
.PHONY: all managed native clean dirs

all: dirs managed native package
	@echo ""
	@echo "==================================================="
	@echo "  Build complete: $(EBOOT)"
	@echo "  Copy $(BUILD_DIR)/ to ms0:/PSP/GAME/YourApp/"
	@echo "==================================================="

# ── Directory creation ────────────────────────────────────────────────────
dirs:
	@mkdir -p $(BUILD_DIR) $(MANAGED_DIR) $(NATIVE_DIR)

# ── Managed compilation ───────────────────────────────────────────────────
# Phase 1: compile the corlib (Psp.* types + internal call declarations)
$(CORLIB_OUT): $(CORLIB_SRCS) | dirs
	@echo "[CSC] corlib.dll"
	$(CSC) $(CSC_FLAGS) \
	    -target:library \
	    -out:$@ \
	    $(CORLIB_SRCS)

# Phase 2: compile sample apps referencing corlib
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
