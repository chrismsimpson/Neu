# Neu

An experimental programming language

Very much inspired by https://github.com/serenityOS/jakt, with modifications:

Types and namespaces are `PascalCase`, instances are `camelCase`. This intentionally mimics Swift. The runtime has no namespace (as opposed to say `AK::`), therefore a lot of stuff is hidden in `Detail::`.

There is no `mut`, there is only `var`.

You `var` (mutable) or `let` (immutable) things.

`::` is nowhere in the language, there is only `.`. This is enforced with `Uppercase` to specify namespaces and/or type names, `lowercase` is for instance names.

Instances are denoted with `(this, ...`, mutable instances with `(var this, ...`.

