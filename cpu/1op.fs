\ 1OP instructions

CREATE 1ops 16 allot

\ jz a ?(label) - Jump if zero.
:noname 0= zbranch ; 1ops !

\ get_sibling object -> (result) ?(label)
\   - Fetch this object's sibling, branching if it exists (is nonzero).
:noname zobject sibling rel@   dup zstore zbranch ; 1ops 1 + !

\ get_child object -> (result) ?(label)
\   - Fetch this object's child branching if it exists (is nonzero).
:noname zobject child   rel@   dup zstore zbranch ; 1ops 2 + !

\ get_parent object -> (result)
\   - Fetch this object's parent. Note that it does not branch like the other
\     two.
:noname zobject parent  rel@   zstore ; 1ops 3 + !


\ get_prop_len prop-address -> (result)
\   - Stores the length of the property at this address.
\     The address points at the data value, so we need to work backward to find
\     the size.
:noname data>prop prop-len zstore ; 1ops 4 + !

\ inc (var) - Increments the variable whose number is given.
:noname dup var@ 1+ swap var! ; 1ops 5 + !
\ dec (var) - Decrements the variable whose number is given.
:noname dup var@ 1- swap var! ; 1ops 6 + !

\ print_addr ba - Prints the string at the given byte address.
:noname print-ba ; 1ops 7 + !

\ call_1s routine -> (result) - Call a routine without args, storing the result.
:noname 0 0 true zcall ; 1ops 8 + !

\ remove_obj object - Removes this object from the tree: no parent, no siblings.
:noname remove-obj ; 1ops 9 + !

\ print_obj obj - Prints the short name of the object.
:noname zobject print-obj ; 1ops 10 + !

\ ret val - Returns the value from this routine.
:noname zreturn ; 1ops 11 + !

\ jump label - Jumps unconditionally.
\ This is not a branch opcode; the value is a signed word to apply to the PC.
:noname 2 - pc+ ; 1ops 12 + !

\ print_paddr pa - Print the string at the given packed address.
:noname print-pa ; 1ops 13 + !

\ load (var) -> (result) - Stores the given variable.
:noname var@ zstore ; 1ops 14 + !

\ v1-4: not value -> (result) - Store the bitwise negation of the value.
\ v5:   call_1n routine       - Call a routine with no arguments, no return.
:noname v5 IF 0 0 false zcall ELSE invert zstore THEN ; 1ops 15 + !

