\ EXT opcodes

CREATE extops 32 ALLOT

\ save table bytes name prompt -> (result)
\ TODO Save/restore
:noname 0 zstore ; extops !

\ restore table bytes name prompt -> (result)
\ TODO Save/restore
:noname 0 zstore ; extops $01 + !

\ log_shift number places -> (result)
:noname 0 th 1 th dup 0 < IF abs rshift ELSE lshift THEN zstore ; extops $02 + !

\ art_shift number places -> (result)
:noname 0 th 1 th dup 0 < IF abs arshift ELSE lshift THEN zstore ; extops $03 + !

\ set_font font -> (result)
\ Sets the font indicated, and store the previous font's ID (or 0 if unavailble).
:noname 0 zstore ; extops $04 + !

\ 5 - 8 are v6 only.

\ save_undo -> (result)
:noname -1 zstore ; extops $09 + ! \ -1 means "unable to provide".

\ restore_undo -> (result)
:noname 0 zstore ; extops $0A + !  \ Illegal without a successful save_undo.

\ B and C are unicode-related.
\ D is true colour.
\ E and F are undefined.
\ 10 - 1D are v6 only.

