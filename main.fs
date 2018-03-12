\ Main launching words for the interpreter.

: read-low-mem ( -- ) 64 0 DO i dup /buffer * mem +   blk@ LOOP ;

: zstart ( -- )
  read-low-mem
  $7878 1 print-ra
;

\ Bootstrapping
\ ' zstart (main!)
\ key drop (bootstrap)

\ Live start
key drop zstart key

