.PHONY: bootstrap
default: bootstrap

FORTH_FILES := output.fs base.fs header.fs strings.fs main.fs

EMULATOR ?= dcpu

combined.fs: $(FORTH_FILES)
	cat $(FORTH_FILES) > $@

zm.rom: boot.dcs combined.fs forth_boot.rom
	rm -f $@
	touch $@
	$(EMULATOR) -turbo -disk combined.fs -script boot.dcs forth_boot.rom

bootstrap: zm.rom

run: bootstrap FORCE
	$(EMULATOR) -turbo -disk Zork1.z5 zm.rom

direct: run.dcs combined.fs forth_boot.rom
	$(EMULATOR) -turbo -script run.dcs -disk combined.fs forth_boot.rom

clean: FORCE
	rm -f zm.rom combined.fs

FORCE:

