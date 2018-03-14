\ Fundamental data of the system.

CREATE mem $8000 allot

\ NB: 2VARIABLEs are stored big-endian.
: 2VARIABLE CREATE 2 allot ;

2VARIABLE zpc

: (pc+) ( u -- ) zpc ds+! ;
: (pc-) ( u -- ) >R zpc 2@ R> ds- zpc 2! ;
: pc+ ( delta -- ) dup 0< IF negate (pc-) ELSE (pc+) THEN ;

VARIABLE (version)

: version (version) @ ;

\ Z-machine stack is empty-ascending.
CREATE zstack 64 allot
VARIABLE zsp    zstack zsp !
VARIABLE zfp    0      zfp !

: push ( w -- ) zsp @ !   1 zsp +! ;
: pop  ( -- w ) 1 zsp -!   zsp @ @ ;
: peek ( -- w ) zsp @ @ ;

: >byte ( w addr_lo -- b ) 1 and IF 255 and ELSE 8 rshift THEN ;

: (byte>hi) ( b w -- w ) 255   and   swap 8   lshift or ;
: (byte>lo) ( b w -- w ) $ff00 and   swap 255 and    or ;

\ The first 64KB are always loaded into memory, so byte addresses are always in
\ memory.

\ Generic byte read and write.
: rb ( byte-index base -- b ) swap dup >R   1 rshift + @   R> >byte ;
: wb ( b byte-index base -- ) over 1 rshift + dup >R @ ( b bi w    R: addr )
  swap 1 and IF (byte>lo) ELSE (byte>hi) THEN R> ! ;

: (rw-split) ( byte-index base -- w )
  2dup rb 8 rshift >R ( byte-index base    R: hi )
  >R 1+ R> rb R> or ;
: rw ( byte-index base -- w )
  over 1 and IF (rw-split) ELSE swap 1 rshift + @ THEN ;

: (ww-split) ( w byte-index base -- )
  >R over 8 rshift   over R@ wb
  >R 255 and R> 1+ R> wb ;
: ww ( w byte-index base -- )
  over 1 and IF (ww-split) ELSE swap 1 rshift + ! THEN ;

\ Byte addresses are always in mem.
: rbba ( ba -- b ) mem rb ;
: wbba ( b ba -- ) mem wb ;
: rwba ( ba -- w ) mem rw ;
: wwba ( w ba -- ) mem ww ;

: ra>block ( ra -- block ) 6 lshift swap   10 rshift or ;
: ra>index ( ra -- index ) drop 1023 and ;

8 CONSTANT #caches
512 CONSTANT /buffer
CREATE caches #caches allot
CREATE block-buffers   #caches /buffer * allot
VARIABLE next-buffer \ Holds the index of the next buffer in the round-robin.

0 next-buffer !

: cache-buffer ( i -- addr ) /buffer * block-buffers + ;
: next-buffer ( -- i ) next-buffer @
  dup 1+ dup #caches >= IF drop 0 THEN next-buffer ! ;

\ Ensure a particular block is cached, and return its address.
: cache ( blk -- addr )
  #caches 0 DO i caches + @ over = IF drop i cache-buffer UNLOOP EXIT THEN LOOP
  next-buffer   ( blk ix-cache ) 2dup caches + ! ( blk ix-cache )
  cache-buffer ( blk cache ) dup >R blk@ R> ;

\ Real addresses are double-cell values.
: ra> ( ra -- byte-index base ) 2dup ra>block cache >R ra>index R> ;

\ If the top word is 0, it's in low memory. Otherwise use the disk.
\ Writing to the disk is not allowed, so that's a no-op.
: rbra ( ra -- b ) dup 0= IF drop mem rb ELSE ra> rb THEN ;
: rwra ( ra -- w ) dup 0= IF drop mem rw ELSE ra> rw THEN ;

: wbra ( b ra -- ) dup 0= IF drop mem wb ELSE drop 2drop THEN ;
: wwra ( w ra -- ) dup 0= IF drop mem ww ELSE drop 2drop THEN ;


: wa ( wa -- ra ) dup 1 lshift swap 15 rshift ;

VARIABLE pa-shift
: pa ( pa -- ra ) dup pa-shift @ lshift   swap 16 pa-shift @ - rshift or ;


\ Peeks the current value at PC.
: pc@  ( -- b ) zpc 2@ rbra ;
: pc@w ( -- w ) zpc 2@ rwra ;

\ Gets the value at PC, and advances PC past it.
: pc@+  ( -- b ) pc@  1 pc+ ;
: pc@w+ ( -- w ) pc@w 2 pc+ ;


\ Move and fill for Z-machine values. All address are byte addresses.

\ Copies forward.
: zmove+ ( src dst u -- ) 0 DO over i + rbba over i + wbba LOOP 2drop ;
\ Copies backward.
: zmove- ( src dst u -- )
  1- 0 swap DO over i + rbba over i + wbba -1 +LOOP 2drop ;

\ Copies so as to avoid corruption.
: zmove ( src dst u -- )
  >R 2dup < R> swap ( src dst u lower? )
  IF zmove- ELSE zmove+ THEN
;

: zfill ( buf u b -- )
  -rot over + swap ( b end start )
  DO dup i wbba LOOP drop
;

