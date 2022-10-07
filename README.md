# Neu

An experimental programming language

Very much inspired by https://github.com/serenityOS/jakt, with modifications:

There is no `mut`, there is only `var`.

You `var` (mutable) or `let` (immutable) things.

`::` is nowhere in the language, there is only `.`. This is enforced with `Uppercase` to specific namespaces and/or type names, `lowercase` is for instance names.

Instances are denoted with `(this, ...`, mutable instances with `(var this, ...`.

