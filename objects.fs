\ Z-machine objects helpers.

\ The structure of an object depends on the version.
\ v1-3 uses: 32 attributes, 1-byte relatives, props pointer.
\ v4+  uses: 48 attributes, 2-byte relatives, props pointer.

: v3- ( -- ? ) version 3 <= ;

: obj-size v3- IF 9 ELSE 14 THEN ;

\ There are 31 properties in older versions, 63 in later ones.
: max-prop ( -- n ) v3- IF 31 ELSE 63 THEN ;

\ Returns the default value for a property.
: prop-default ( num -- w ) 1- 2 *   hdr-objtable rwba + rwba ;

\ Returns the byte address of the first object in the table.
: objtable ( -- ba ) hdr-objtable rwba   max-prop 2 * + ;

\ Returns the byte address of the object, given its number.
: zobject ( num -- ba ) 1- obj-size *   objtable + ;


\ Working with relatives:
\ parent, sibling and child adjust an object pointer to point at the relative.
\ Since the size differs between versions, use rel@ and rel! to change them.
: parent  ( ba -- ba ) v3- IF 4 ELSE  6 THEN + ;
: sibling ( ba -- ba ) v3- IF 5 ELSE  8 THEN + ;
: child   ( ba -- ba ) v3- IF 6 ELSE 10 THEN + ;

: rel@ ( ba -- num ) v3- IF rbba ELSE rwba THEN ;
: rel! ( num ba -- ) v3- IF wbba ELSE wwba THEN ;


VARIABLE ro-num

\ Helper for remove-obj: gets the number of the sibling of ro-num.
: younger ( -- num ) ro-num @ zobject sibling rel@ ;

\ Helper for remove-obj: handles the first-child case.
: ro-first ( ba-parent -- )
  >R   younger   R> child rel! ;

\ Scans the sibling chain until the sibling of this object equals ro-num.
: elder ( ba -- ba )
  BEGIN
    dup sibling rel@   dup ro-num @ <>
  WHILE ( ba-elder num-younger )
    nip zobject ( ba )
  REPEAT
  drop ( ba-elder )
;

: ro-younger ( ba-parent -- )
  child rel@ zobject elder ( ba-elder )
  >R younger R> sibling rel!
;

\ Removes this object from the tree - no parent, no siblings, keeps its children.
: remove-obj ( num -- )
  dup ro-num ! \ Save the number for use later.
  zobject parent rel@ dup 0= IF drop EXIT THEN \ Bail if it's already out.
  ( parent-num )

  \ Two cases: first child, or later child.
  zobject   dup child rel@   ro-num @ = IF ro-first ELSE ro-younger THEN
;


\ Working with attributes.

\ Splits an attribute number into its byte offset and mask.
: (attr) ( attr -- ba-offset mask )
  dup >R   3 rshift
  7   R> 3 and -
  1 swap lshift
;

\ Checks the attribute on this object.
: attr? ( ba-obj attr -- ? ) (attr) >R + rbba   R> and ;
\ Sets an attribute.
: attr+ ( ba-obj attr -- ) (attr) >R +   dup rbba R> or           swap wbba ;
\ Clears an attribute.
: attr- ( ba-obj attr -- ) (attr) >R +   dup rbba R> invert and   swap wbba ;


\ Working with properties.

\ Returns an object's property table.
: prop-table ( ba -- ba ) v3- IF 7 ELSE 12 THEN + rwba ;

\ Returns the address of an object's short name.
: short-name ( ba -- ) prop-table 1+ ;

\ Prints an object, given a pointer to it.
: print-obj ( ba -- ) short-name print-ba ;


\ Returns the address of the (header of) the first property, given an object.
: first-prop ( ba-obj -- ba-prop ) prop-table dup rbba 2 * + 1+ ;


\ Given a pointer to a property's data region, work backward to point at its
\ first header byte.
\ In v3, it's always a single byte. In v4+ it's two if bit 7 is set.
: data>prop ( ba -- ba )
  v3- IF 1- EXIT THEN

  \ Handle the more complex case of v4+.
  dup rbba ( ba b )
  $80 and IF 1- THEN
;

\ Given a property header, advances the pointer to its data field.
\ Headers in v3- are always 1 byte, in v4+ it depends on the top bit.
: >data ( ba-prop -- ba )
  v3- IF 1+ EXIT THEN
  dup rbba $80 and IF 1+ THEN
  1+
;

\ Given a pointer to the property header, returns its number of data bytes.
\ In v3, the single byte is 32 * (len - 1) + number.
\ In v4 it's either: 1-nnnnnn 1-ssssss treating size 0 as 64, or
\   0wnnnnnn with w=1 indicating a word, w=0 a byte.
: prop-len ( ba -- u )
  v3- IF rbba 5 rshift 1+ EXIT THEN

  dup rbba dup $80 and IF ( ba b ) \ Long form.
    drop 1+ rbba $3f and
    dup 0= IF drop 64 THEN \ Special case: length 0 is 64.
  ELSE
    nip $40 and IF 2 ELSE 1 THEN
  THEN
;

\ Given a property header's address, returns the next one.
\ Since the property list is ended by a byte giving "property 0", that's what
\ signals there are no more properties.
: next-prop ( ba-prop -- ba-prop ) dup prop-len swap >data + ;

\ Given a property header, returns its number.
\ In v3- that's the bottom 5 bits, in v4+ it's the bottom 6.
: prop-num ( ba -- u ) rbba v3- IF $1f ELSE $3f THEN and ;

\ Gets the property address by its number.
: prop ( ba-obj num -- ba-prop | 0 )
  >R first-prop
  BEGIN dup prop-num R@ > WHILE next-prop REPEAT ( ba-prop    R: num )
  \ We've either found the 0 at the bottom, a smaller prop, or the correct one.
  dup prop-num R> <> IF drop 0 THEN \ Doesn't match, so return 0.
;

