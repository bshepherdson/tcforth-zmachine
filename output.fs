\ Set up to use the serial line as our output.
VARIABLE serial

\ Makes a HWI to the SSI serial device. B holds the input, C an error code.
\ We ignore the error code, and expect the device to be configured for 1 octet,
\ no interrupts.
: emit ( c -- ) 3 $c000 serial @ hwi ; \ A, B input, no output.

: cr ( -- ) 10 emit ; \ Send an ASCII newline.
\ : accept accept ; \ TODO

0 ' emit ' accept ' cr (setup-hooks)

: serial-init $9027 $e57d find-dev serial ! ;
serial-init

