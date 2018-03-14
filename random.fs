\ Random number handling - HWIs to the PRNG device.

VARIABLE rng

: init-rng ( -- ) $dc12 $13e2 find-dev rng ! ;

\ Returns a random value, capped to the range.
\ A = 1, C gets the output. a--- ---- --C- ----
: random ( n -- n ) 1 $8020 rng @ hwi ( max u ) swap umod 1+ ;

\ Seeds the generator with this value. Both the Z-machine and the hardware use
\ the same convention that a seed of 0 means to re-seed with real entropy.
\ A = 0, B = seed.  ab-- ---- ---- ----
: seed ( u -- ) 0 $c000 rng @ hwi ( ) ;

