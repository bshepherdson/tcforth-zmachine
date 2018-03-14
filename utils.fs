\ Instruction utilities: for routine calls, returning, storing and branching.

\ Inside a routine, the stack looks like this:
\ (low addresses)
\ previous frames...
\ local N
\ ...
\ local 1
\ old-fp    <---- fp
\ old-sp
\ old-pc-lo
\ old-pc-hi
\ arg-count
\ return-expected  <--- sp


\ First, variables: stack, locals, and globals.

: local ( var -- addr ) zfp @ swap -   zstack + ;
: local@ ( var -- w ) local @ ;
: local! ( w var -- ) local ! ;

: global  ( var -- ba ) 16 - 2*   hdr-globals rwba + ;
: global@ ( var -- w )  global rwba ;
: global! ( w var -- )  global wwba ;


: var@ ( var -- w )
  dup 0= IF drop pop EXIT THEN \ 0 = pop the stack
  dup 16 <= IF local@ ELSE global@ THEN
;

: var! ( w var -- )
  dup 0= IF drop push EXIT THEN
  dup 16 <= IF local! ELSE global! THEN
;


\ Arguments for VAR opcodes are stored in this array.
CREATE varargs 8 allot
VARIABLE next-arg

\ Helper that fetches the arg by number (0-based).
: th ( ix -- w ) varargs + @ ;

\ Returns the number of args provided.
: #args ( -- u ) next-arg @ varargs - ;



\ Stores a value into the location indicated by the "store" byte at PC.
: zstore ( w -- ) pc@+ var! ;


\ Calls and returns.

\ Addresses where the values are stored.
: old-fp ( -- addr ) zfp @ ;
: old-sp ( -- addr ) zfp @ 1+ ;
: old-pc ( -- addr ) zfp @ 2 + ;
: routine-args  ( -- addr ) zfp @ 4 + ;
: return-expected  ( -- addr ) zfp @ 5 + ;


\ Note that our ret-value is an actual value, not a variable reference.
\ So it's safe to mangle the stack; if we're returning a local or stacked value,
\ it's already been read.
: zreturn ( w -- )
  return-expected @
  \ Restore the old PC, SP and FP, in that order. (FP must be last.)
  old-pc 2@ zpc 2!
  old-sp  @ zsp  !
  old-fp  @ zfp  !
  ( value ret? )
  IF zstore ELSE drop THEN
;



\ Helper for zcall. Expects the routine address and number of locals.
\ Copies the default local values at routine+1, etc. into the locals.
\ Returns the adjusted first byte of the routine.
: copy-locals ( ra-routine #locals -- ra-routine' )
  >R 1+ R>
  0 ?DO
    2dup rwra ( ptr w )
    i 1+ local! ( ptr )
    2 ds+
  LOOP
;

: zero-locals ( #locals -- ) 0 ?DO 0 i 1+ local! LOOP ;

: copy-args ( args #args -- ) 0 ?DO dup i + @   i 1+ local! LOOP drop ;


\ Expects the packed address for the routine, an array of args, and a flag for
\ whether the call should return a value.
\ In v5+ , locals are zeroed and args copied. In older versions, locals have
\ default values at the start of the routine.
\ Special case: if the target address is 0, treat it as "return false".
: zcall ( pa-routine args #args return? -- )
  >R >R >R

  \ Special case of routine 0.
  dup 0= IF drop R> R> 2drop R> IF 0 zreturn THEN EXIT THEN

  pa ( ra-routine    R: return? #args args )

  \ First, get the local count.
  2dup rbra >R 1 ds+ R> ( ra-routine+1 #locals )

  \ Next, prepare the stack frame.
  zfp @ >R     \ Save the old FP for a moment.
  zsp @ over +   zfp ! \ Set the new one.

  R>     old-fp  !
  zsp  @ old-sp  !
  zpc 2@ old-pc 2!

  R> return-expected !
  R> routine-args !

  zfp @   5 +   zsp ! \ Set the new SP as well.

  \ Now everything is ready for preparing the locals and copying args.
  dup >R v5+ IF zero-locals ELSE copy-locals THEN
  ( ra-first-instruction    R: args #locals )

  \ Now copy the args onto the locals.
  R> routine-args @ min   R> swap copy-args   ( ra-first-instruction )
  zpc 2! \ PC is set now.

  \ Ready to go!
;



\ Handles a branch, given a condition.
: zbranch ( ? -- )
  0<> \ Real flag, not a value.
  pc@+

  \ If bit 7 is 0, invert our flag.
  dup $80 and not IF >R 0= R> THEN

  \ If bit 6 is 1, single byte 0-63. If 0, two-byte signed 14-bit offset.
  dup $40 and IF
    63 and
  ELSE
    63 and 8 lshift pc@+ or ( branch? unsigned-offset )
    dup $2000 and IF $4000 swap - negate THEN \ Adjust to signed offset.
  THEN

  ( branch? branch-offset )
  swap not IF drop EXIT THEN \ Bail if we're not really branching.

  \ An offset of 0 is return false, 1 is return true.
  dup 1 invert and 0= IF zreturn EXIT THEN

  \ Otherwise, we adjust PC by the offset - 2 .
  2 - pc+
;

: illegal ( n -- ) S" [Illegal: " type . S" ]" type cr ;

