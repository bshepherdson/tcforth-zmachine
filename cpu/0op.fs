\ 0OP instructions

CREATE 0ops 16 allot

\ rtrue - Return true (ie. 1) from the current routine.
:noname ( -- ) 1 zreturn ; 0ops 0 + !

\ rfalse - Return false (ie. 0) from the current routine.
:noname ( -- ) 0 zreturn ; 0ops 1 + !

\ print (literal-string) - Prints the Z-string found at PC, leaving PC pointed
\ after the string.
:noname ( -- ) print-pc ; 0ops 2 + !

\ print_ret - Prints as above, and returns true.
:noname ( -- ) print-pc   1 zreturn ; 0ops 3 + !

\ nop - Does nothing
:noname ; 0ops 4 + !

\ save - (Old save) Saves the game, branching and storing according to version.
\ Illegal in v5+.
:noname
  version 4 < IF false zbranch EXIT THEN
  version 4 = IF 0 zstore EXIT THEN
  2 illegal
; 0ops 5 + !

\ restore - (Old restore) Restores the game, branching and storing according to
\ version.
:noname
  version 4 < IF false zbranch EXIT THEN
  version 4 = IF 0 zstore EXIT THEN
  2 illegal
; 0ops 6 + !

\ restart - Restarts the Z-machine from the beginning.
:noname zrestart ; 0ops 7 + !

\ ret_popped - Pop a value and return it.
:noname pop zreturn ; 0ops 8 + !

\ v1-4: pop   - Pops a value and discards it.
\ v5:   catch - Returns the current "stack frame" for use by a later throw.
\ Stack frames take the form of the current setting of zfp.
:noname v5 IF zfp @ zstore ELSE pop drop THEN ; 0ops 9 + !

\ quit - Quits the interpreter.
:noname cr S" Goodbye!" type cr   false running ! ; 0ops 10 + !

\ new_line - Prints a newline.
:noname cr ; 0ops 11 + !

\ show_status - Reprints the status line immediately, rather than waiting for
\ input.
\ v3 only, but should be a no-op in other versions.
:noname version 3 = IF print-status THEN ; 0ops 12 + !

\ verify - Runs the checksum on the file.
\ TODO - Actually implement the checksum.
:noname true zbranch ; 0ops 13 + !

\ 14 is the first byte of extended opcodes, and not defined for earlier versions.

\ piracy - Branch if the story file is legit.
\ We always branch, gullibly.
:noname true zbranch ; 0ops 15 + !

