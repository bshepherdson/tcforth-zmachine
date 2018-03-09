\ Fundamental data of the system.

CREATE mem $8000 allot

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

: ra>block ( ra -- block ) 10 lshift swap   10 rshift or ;
: ra>index ( ra -- index ) drop 1024 and ;

8 CONSTANT #caches
512 CONSTANT /buffer
CREATE caches #caches allot
CREATE block-buffers   #caches /buffer * allot
VARIABLE next-buffer \ Holds the index of the next buffer in the round-robin.

: cache-buffer ( i -- addr ) /buffer * block-buffers + ;
: next-buffer ( -- i ) next-buffer @
  dup 1+ dup #caches >= IF drop 0 THEN next-buffer ! ;

\ Ensure a particular block is cached, and return its address.
: cache ( blk -- addr )
  #caches 0 DO i caches + @ over = IF i cache-buffer UNLOOP EXIT THEN LOOP
  next-buffer   2dup caches + ! \ Write the block number into the cache.
  cache-buffer dup >R blk@ R> ;

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


\ Initialization
: read-low-mem ( -- ) 64 0 DO i dup /buffer * mem +   blk@ LOOP ;

: zstart ( -- ) read-low-mem S" ZM ready: " type hex mem . cr decimal ;

\ Testing
' zstart (main!)
key drop (bootstrap)

