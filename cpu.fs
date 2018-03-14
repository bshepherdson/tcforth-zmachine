\ Main CPU routines for decoding and executing opcodes.

:noname 0 ;          \ 3 - Omitted
:noname pc@+ var@ ;  \ 2 - Variable
:noname pc@+ ;       \ 1 - Small constant
:noname pc@w+ ;      \ 0 - Large constant
CREATE arg-readers , , , ,

\ Reads an argument, given its type. Does nothing on type 3 ("omitted").
: read-arg ( type -- arg | 0 ) arg-readers + @ execute ;


\ Decodes and runs a single opcode in short form.
: short-form ( opcode -- )
  dup $f and                          \ Opcode in bottom 4 bits.
  swap 4 rshift 3 and ( opcode type ) \ Operand type is in bits 4 and 5.
  dup 3 = IF \ Omitted arg, so 0OP.
    drop 0ops
  ELSE \ 1OP
    read-arg swap 1ops
  THEN
  ( [arg] opcode table ) + @ execute
;


\ In long form, it's always 2OP.
\ Bit 6 gives the first operand type, bit 5 the second.
\ 1 = variable, 2 = small constant.
\ Opcode in the bottom 5 bits.
: long-form ( opcode -- )
  dup $1f and >R \ Save the opcode itself for later.
  dup  5 rshift 1 and 1+ read-arg ( opcode a1 )
  swap 4 rshift 1 and 1+ read-arg ( a1 a2 )
  R> 2ops + @ execute
;

\ In variable form, we read the types byte and pull 0-4 arguments into the
\ args array.
: read-varargs ( -- )
  pc@+ 0 6 DO
    dup i rshift 3 and read-arg
    next-arg @ varargs + !
    1 next-arg +!
  -2 +LOOP
;

\ 2OPs can be encoded in variable form, and this word handles that case.
: var-2op ( opcode -- ) >R 0 th 1 th R> $1f and 2ops + @ execute ;

\ Main variable form handler.
\ Bit 5 signals 2OP when clear, VAR when set.
: var-form ( opcode -- )
  read-varargs
  dup $20 and 0= IF var-2op EXIT THEN

  \ VAR instruction.
  \ Special case: v5+ has call_vs2 and call_vn2 (12 and 26). Read another byte.
  dup 26 =   over 12 = and   version 5 >= and IF read-varargs THEN

  \ Now the arguments are ready.
  $1f and varops + @ execute
;

\ Decodes and runs a single opcode.
: exec ( -- )
  pc@+
  dup $3f and $c0 = IF var-form EXIT THEN
  dup $3f and $80 = IF short-form EXIT THEN
  dup $be =   version 5 >= and IF ext-form EXIT THEN
  long-form
;

