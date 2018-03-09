.PHONY: bootstrap
default: bootstrap

FORTH_FILES := $(filter-out combined.fs, $(wildcard *.fs))

EMULATOR ?= dcpu

combined.fs: $(FORTH_FILES)
	cat $(FORTH_FILES) > $@

zm.rom: boot.dcs combined.fs forth_boot.rom
	rm -f $@
	touch $@
	$(EMULATOR) -turbo -disk combined.fs -script boot.dcs forth_boot.rom

bootstrap: zm.rom

run: bootstrap FORCE
	$(EMULATOR) -turbo -disk Zork1.z3 zm.rom

clean: FORCE
	rm -f zm.rom combined.fs

FORCE:

