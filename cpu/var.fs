\ VAR ops. Access their arguments with the varargs array, or 'th'.

\ Helper for function calls. All the var-format calls are the same, we just
\ need to supply the return-expected flag.
: var-call ( return-expected? -- ) >R   0 th   varargs 1+ #args 1- R> zcall ;

CREATE varops 32 allot

\ call_vs routine args... -> (result) - Call with variable args and store.
:noname true var-call ; varops !

\ storew array index value - Store the word into the word array.
:noname 1 th 2 *   0 th +   2 th swap wwba ; varops $01 + !

\ storeb array index value - Store the byte into the byte array.
:noname 2 th   0 th 1 th + wbba ; varops $02 + !

\ put_prop obj prop value - Set the value of the property on the object.
\   - If the object doesn't have the property, error out.
\   - If the data field is longer than 2 bytes, we only write the first two.
:noname
  2 th
  0 th zobject 1 th prop ( value ba-prop )
  dup 0= IF (break) THEN \ Busted if it's not defined.
  dup >data swap prop-len 1 = ( value ba long? ) IF wbba ELSE wwba THEN
; varops $03 + !

\ read text parse time routine -> (result)
\ NB: Only stores in v5.
\ TODO Handle input!
:noname ; varops $04 + !

\ print_char char-code - Prints a single character.
:noname 0 th 13 = IF cr ELSE 0 th emit THEN ; varops $05 + !

\ print_num value - Prints a signed number.
:noname 0 th . ; varops $06 + !


\ random range -> (result)
\ Positive values return a result in the range [1, n].
\ Negative values (eg. -7) use their positive selves as the seed (eg. 7).
\ 0 re-seeds randomly.
\ We use the PRNG device for this, making HWIs.
:noname 0 th dup 0> IF random ELSE negate seed 0 THEN zstore ; varops $07 + !


\ push value - Saves the given value onto the stack.
:noname 0 th push ; varops $08 + !

\ pull (var) - Pulls a value from the stack onto the given variable.
\ Special case: pulling into var 0 (stack) results in overwriting the top of the
\ stack, not pushing a new value.
:noname pop 0 th dup IF var! ELSE pop 2drop push THEN ; varops $09 + !


\ split_window lines - Splits the window.
\ Not supported in this serial or printing workflow, so this silently does
\ nothing.
\ TODO Terminal handling
:noname ; varops $0a + !

\ set_window window - Moves the cursor to the specified window.
\ TODO Terminal handling
:noname ; varops $0b + !

\ call_vs2 routine args... -> (result)
:noname true var-call ; varops $0c + !

\ erase_window window
\ TODO Terminal handling
:noname ; varops $0d + !

\ erase_line value
\ TODO Terminal handling
:noname ; varops $0e + !

\ set_cursor line column
\ TODO Terminal handling
:noname ; varops $0f + !

\ get_cursor array
\ TODO Terminal handling
:noname 0 0 th wwba   0 0 th 2 + wwba ; varops $10 + !

\ set_text_style style
\ TODO Terminal handling
:noname ; varops $11 + !

\ buffer_mode flag
\ TODO Terminal handling
:noname ; varops $12 + !

\ output_stream number table
\ TODO Output streams - handling the buffering one.
:noname ; varops $13 + !

\ input_stream number
\ TODO Input streams - only relevant if we support output stream 4 and sending
\ command transcripts to and from files.
:noname ; varops $14 + !

\ sound_effect
:noname ; varops $15 + !

\ read_char 1 time routine -> (result)
\ Reads a single character, storing its value.
\ TODO Handle the time and routine arguments; same as in @read.
\ TODO ZSCII is not the same format as the DCPU keyboard sends; adjust it.
:noname key zstore ; varops $16 + !

VARIABLE st-width  \ Width of each record in the table.
VARIABLE st-reader \ xt for the reader function (rbba or rwba).

\ scan_table x table len form -> (result)
\ Searches through a table for a specific value.
:noname
  \ Read the format, if any, and populate the variables.
  #args 4 = IF
    3 th
    dup $80 and IF ['] rwba ELSE ['] rbba THEN st-reader !
    $7f and st-width !
  ELSE
    2        st-width  !
    ['] rwba st-reader !
  THEN

  \ Now scan the table.
  1 th
  2 th 0 DO
    dup st-reader @ execute   0 th = IF zstore UNLOOP EXIT THEN
    st-width @ +
  LOOP
  drop 0 zstore
; varops $17 + !


\ not value -> (result)
:noname 0 th invert zstore ; varops $18 + !

\ call_vn routine args...
:noname false var-call ; varops $19 + !
\ call_vn2 routine args...
:noname false var-call ; varops $1A + !


\ tokenise test parse dictionary flag
\ TODO Reading input
\ This performs roughly the same operation as read, so can be combined with it.
:noname ; varops $1B + !


\ encode_text zscii-text length from coded-text
\ TODO Reading input
\ This is similarly one fraction of the text-encoding process, and should be
\ combined with the read code.
:noname ; varops $1C + !


\ copy_table first second size
\ If second is 0, size bytes of first are zeroed.
\ Otherwise, first is copied to second, its length being |size|.
\ If size is positive, copy to avoid damage. If negative, copy forward always.
:noname
  1 th 0= IF 0 th   2 th abs   0 zfill EXIT THEN \ Handle the second=0 case.

  0 th 1 th ( src dst )
  2 th dup 0 < IF abs zmove+ ELSE zmove THEN
; varops $1D + !


\ print_table zscii-text width height skip
\ This is doable even without cursor addressing, but it's inessential; punted.
\ TODO Implement print_table before someone needs it.
:noname ; varops $1E + !


\ check_arg_count arg-number ?(label)
\ Branches if the argument (1-based) has been provided by the caller.
:noname 0 th   routine-args @  <= zbranch ; varops $1F + !

