\ Words for working with strings and text on the Z-machine.4 zstr-finger +!


\ Note that we never really need a decoded string in memory. We can actually
\ stream characters to an output function without retaining them.

\ Since string decoding can be nested, care is required using global variables.
\ Fortunately, the nesting cannot be very deep, since abbreviations cannot
\ themselves make use of abbreviations.

\ So we have a stack with room for 12 words, or three levels of decoding.

CREATE zstr-stack 12 allot

VARIABLE zstr-finger \ Points at the current block of the stack we're using for
                     \ the below.

: zstr-ptr   zstr-finger @ ;     \ Long pointer to the next word.
: zstr-word  zstr-finger @ 2 + ; \ Last word read from the string.
: zstr-index zstr-finger @ 3 + ; \ Index into that word of the next character.


\ Shift is not stored on the stack, since abbreviations both start and end with
\ it set to 0.
VARIABLE shift


\ Tries to pop up a level in the decoding. If we're already at the top, return
\ true to signal we've exhausted the string.
: zstr-?pop ( -- exhausted? )
  zstr-finger @   zstr-stack = IF true EXIT THEN
  -4 zstr-finger +!
  0 shift !
  false
;

\ Reads the next word from the string. Returns true if we've run out of string.
: zstr-grab-word ( -- exhausted? )
  \ If this string has run out, pop if possible. If pop returns false, we're
  \ ready to continue reading it. If pop returns exhaustion as well, then we're
  \ really out of strings.
  zstr-word @ $8000 and IF
    zstr-?pop IF true EXIT THEN
    \ If we're still here, check the index. If nonzero, just EXIT with false.
    zstr-index @ IF false EXIT THEN \ Still mid-word on the old Z-string.
    \ Now if we're still here, we need to read a new word.
  THEN
  \ Otherwise, read the next word.
  zstr-ptr 2@ rwra   zstr-word !   15 zstr-index !
  2 zstr-ptr ds+! \ Advance zstr-ptr.
  false
;



\ Reads the next single character from the string, or -1 if it's run out.
: zstr-next ( -- zc | -1 )
  zstr-index @ 0= IF zstr-grab-word IF -1 EXIT THEN THEN

  \ We're ready to get the next character.
  zstr-word @   zstr-index @ 5 -  ( word index' )
  dup zstr-index ! \ Save the new index.
  rshift 31 and ( zchar )
  dup (log)
;


\ Reads the next two characters, which are the high and low 5 bits of a 10-bit
\ ZSCII literal.
: longhand-literal ( -- )
  zstr-next 5 lshift   zstr-next or   emit
  0 shift !
;

CREATE alphabets 26 2 * allot
0 , 0 , char 0 , char 1 , char 2 , char 3 , char 4 , char 5 , char 6 ,
char 7 , char 8 , char 9 , char . , char , , char ! , char ? , char _ ,
char # , char ' , char " , \ "
char / , char \ , char - , char : , char ( , char ) ,

: init-alphabets
  S" abcdefghijklmnopqrstuvwxyz" alphabets      swap move
  S" ABCDEFGHIJKLMNOPQRSTUVWXYZ" alphabets 26 + swap move
;
init-alphabets


: zstr-basic ( zc -- )
  6 -   shift @ 26 * alphabets +   + @ emit
  0 shift !
;


: zstr-emit-space ( zc -- ) drop 32 emit ;

\ Runs an abbreviation, with the table number given and the abbreviation number
\ next up.
: zstr-abbrev ( zc -- )
  1- 5 lshift   zstr-next +   ( abbreviation-number )
  2* \ Convert to words
  hdr-abbreviations rwba + rwba ( wa-string ) wa ( ra-string )

  4 zstr-finger +! \ Move the finger to the next block.
  ( ra ) zstr-ptr 2!
  0      zstr-index !
  0      zstr-word !
  0      shift !
;

: zstr-shift ( zc -- ) 3 - shift ! ;


CREATE special-chars
  ' zstr-emit-space ,
  ' zstr-abbrev dup , dup , ,
  ' zstr-shift  dup , ,


\ Main printing driver. Given a character, acts on it.
: zstr-decode ( zc -- )
  \ A2's 6 is a longhand literal; the next two give a literal ZSCII code.
  \ A2's 7 is a newline, which is a separate word from "EMIT".
  shift @ 2 = IF
    dup 6 = IF drop longhand-literal EXIT THEN
    dup 7 = IF drop cr EXIT THEN
  THEN

  dup 5 > IF zstr-basic EXIT THEN \ Print a simple character.

  \ If we're still here, it's special.
  dup special-chars + @ execute
;

\ Keeps decoding until we run out of characters.
: zstr-loop ( -- ) BEGIN zstr-next dup -1 <> WHILE zstr-decode REPEAT drop ;

: print-ra ( ra -- )
  zstr-stack  zstr-finger !
    zstr-ptr 2!
  0 zstr-word  !
  0 zstr-index !
  zstr-loop
;

: print-ba ( ba -- ) 0  print-ra ;
: print-pa ( pa -- ) pa print-ra ;
: print-pc ( -- )
  zpc 2@ print-ra \ zstr-ptr is now aimed at the byte after the string.
  zstr-ptr 2@ zpc 2!
;
