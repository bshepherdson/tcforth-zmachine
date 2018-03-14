\ 2OP instructions

CREATE 2ops 32 allot

\ There's no 0.

\ je a b ?(label) - Jump if the arguments are equal.
:noname = zbranch ; 2ops $01 + !

\ jl a b ?(label) - Jump if a < b, signed.
:noname < zbranch ; 2ops $02 + !

\ jg a b ?(label) - Jump if a > b, signed.
:noname > zbranch ; 2ops $03 + !

\ dec_chk (var) min
\   - Decrement the variable and branch if it's now less than the minimum.
:noname >R dup var@ 1- dup >R swap var! R> R> < zbranch ; 2ops $04 + !

\ inc_chk (var) max
\   - Increment the variable and branch if it's now greater than the maximum.
:noname >R dup var@ 1+ dup >R swap var! R> R> > zbranch ; 2ops $05 + !


\ jin obj1 obj2 ?(label) - Jump if obj1 is a child of obj2.
:noname >R zobject parent rel@ R> = zbranch ; 2ops $06 + !

\ test bitmask flags ?(label) - Jump if all of the bits in the mask are set.
:noname dup >R and R> = zbranch ; 2ops $07 + !

\ or a b -> (result) - Bitwise OR
:noname or zstore ; 2ops $08 + !

\ and a b -> (result) - Bitwise AND
:noname and zstore ; 2ops $09 + !

\ test_attr obj attr ?(label) - Brach if obj has the attribute.
:noname >R zobject R> attr? zbranch ; 2ops $0a + !

\ set_attr obj attr - Sets attr on this object.
:noname >R zobject R> attr+ ; 2ops $0b + !

\ clear_attr obj attr - Clears attr on this object.
:noname >R zobject R> attr- ; 2ops $0c + !

\ store (var) value - Stores the value into the variable whose number is given.
:noname swap var! ; 2ops $0d + !

\ insert_obj obj dest - Removes obj from the tree, and then makes it the first
\   child of dest.
:noname
  over zobject remove-obj  \ Remove the moving object from the tree.
  dup zobject child rel@   \ Get the current child of the destination.
  >R over zobject sibling R> swap rel! \ And make that the sibling of obj.
  2dup swap zobject parent rel!   \ Make dest the parent of obj.
  zobject child rel!              \ And obj the child of dest.
; 2ops $0e + !

\ loadw array index - Store the word at the index in the array.
:noname 2* + rwba zstore ; 2ops $0f + !

\ loadb array index - Store the byte at the index in the array.
:noname + rbba zstore ; 2ops $10 + !

\ get_prop obj prop -> (result)
\   - Get the property from this object (or the default), and store it.
\     If the size if wrong, reads the first two bytes of it.
:noname
  >R zobject R@ prop ( ba-prop )
  dup 0= IF drop R> prop-default zstore EXIT THEN
  R> drop
  dup >data swap prop-len
  1 = IF rbba ELSE rwba THEN
  zstore
; 2ops $11 + !

\ get_prop_addr obj prop -> (result)
\   - Get the byte address of the property data for this property, or 0 if the
\     object doesn't have this property.
:noname >R zobject R> prop ( ba-prop | 0 ) dup IF >data THEN zstore ; 2ops $12 + !

\ get_next_prop obj prop -> (result)
\   - Gets the number of the next property the object provides, or 0 if not found.
\     If prop is 0, return the first property of the object.
:noname
  dup 0= IF drop zobject first-prop zstore EXIT THEN  \ Special case for get 0.
  >R zobject R> prop next-prop prop-num zstore
; 2ops $13 + !


\ add a b -> (result)
:noname + zstore ; 2ops $14 + !
\ sub a b -> (result)
:noname - zstore ; 2ops $15 + !
\ mul a b -> (result)
:noname * zstore ; 2ops $16 + !
\ div a b -> (result)
:noname / zstore ; 2ops $17 + !
\ mod a b -> (result)
:noname mod zstore ; 2ops $18 + !

VARIABLE c2arg
\ call_2s routine arg1 -> (result)
:noname c2arg ! c2arg 1 true zcall ; 2ops $19 + !
\ call_2n routine arg1
:noname c2arg ! c2arg 1 false zcall ; 2ops $1a + !

\ set_colour fg bg - Not implemented.
\ TODO Handle colour output where possible.
:noname 2drop ; 2ops $1b + !

\ throw value stack-frame - Return as though from the routine that called catch.
\   The stack-frame is a value of zfp. We set that, then zreturn.
:noname zfp ! zreturn ; 2ops $1c + !

\ $1d-f are not defined.

