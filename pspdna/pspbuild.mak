# pspdna/pspbuild.mak
# PSP SDK packaging rules, compatible with build.mak conventions.
#
# Include this file in your project Makefile AFTER setting:
#   PSP_EBOOT_TITLE   — title string shown in XMB (max 127 chars)
#   PSP_EBOOT_ICON    — path to ICON0.PNG (144×80) or NULL
#   PSP_EBOOT_PIC0    — path to PIC0.PNG (480×272) or NULL
#   PSP_ELF           — path to the PSP ELF to package (DNA runtime + new modules)
#   BUILD_DIR         — output directory (default: build)
#
# Produced files:
#   $(BUILD_DIR)/PARAM.SFO  — metadata
#   $(BUILD_DIR)/EBOOT.PBP  — final distributable
#
# The managed assemblies (.dll) must be copied to the build directory
# separately — see the 'package' target in Makefile.

BUILD_DIR     ?= build
PARAM_SFO     ?= $(BUILD_DIR)/PARAM.SFO
EBOOT_PBP     ?= $(BUILD_DIR)/EBOOT.PBP

PSP_EBOOT_TITLE  ?= PSP Application
PSP_EBOOT_ICON   ?= NULL
PSP_EBOOT_ICON1  ?= NULL
PSP_EBOOT_PIC0   ?= NULL
PSP_EBOOT_PIC1   ?= NULL
PSP_EBOOT_SND0   ?= NULL
PSP_ELF          ?= $(BUILD_DIR)/$(TARGET).elf

MKSFO   := mksfoex
PACKPBP := pack-pbp

$(PARAM_SFO): | $(BUILD_DIR)
	$(MKSFO) -d MEMSIZE=1 '$(PSP_EBOOT_TITLE)' $@

$(EBOOT_PBP): $(PARAM_SFO) $(PSP_ELF)
	$(PACKPBP) $@ $(PARAM_SFO) \
	    $(PSP_EBOOT_ICON) $(PSP_EBOOT_ICON1) \
	    $(PSP_EBOOT_PIC0) $(PSP_EBOOT_PIC1) \
	    $(PSP_EBOOT_SND0) \
	    $(PSP_ELF) NULL

$(BUILD_DIR):
	mkdir -p $@

.PHONY: pbp
pbp: $(EBOOT_PBP)
