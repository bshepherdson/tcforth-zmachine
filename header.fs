\ Helpers for working with the header.

$00 CONSTANT hdr-version
$01 CONSTANT hdr-flags1
$04 CONSTANT hdr-himem
$06 CONSTANT hdr-pc0
$08 CONSTANT hdr-dictionary
$0A CONSTANT hdr-objtable
$0C CONSTANT hdr-globals
$0E CONSTANT hdr-static
$10 CONSTANT hdr-flags2
$18 CONSTANT hdr-abbreviations
$1E CONSTANT hdr-int-number
$1F CONSTANT hdr-int-version
$20 CONSTANT hdr-height-lines
$21 CONSTANT hdr-width-chars
$22 CONSTANT hdr-width-units
$24 CONSTANT hdr-height-units
$26 CONSTANT hdr-font-width
$27 CONSTANT hdr-font-height
$2C CONSTANT hdr-default-bg
$2D CONSTANT hdr-default-fg
$2E CONSTANT hdr-terminators
$32 CONSTANT hdr-standard
$34 CONSTANT hdr-alphabet-table

: version ( -- version ) hdr-version rbba ;

