# Neu

An experimental system designed for brevity, ergonomics and productivity.

Very much inspired by https://github.com/serenityOS/jakt, with modifications:

`::` is nowhere in the language, there is only `.` (nowhere in Neu itself, it obviously maps to this in C++). This is acheived with types and namespaces observing `PascalCase` and instance names observing `camelCase`. This intentionally mimics Swift. The runtime has no namespace (as opposed to say `AK::`), therefore a lot of stuff is hidden in `Detail::`.

Core type and instance names are a bit more format/explicit. E.g. `RawPointer`, `NonNullRefPointer` and `pointer()`. Primitives mimic Swift and C# (e.g. `UInt32` as opposed to `i32`).

There is no `mut`, there is only `var`.

You `var` (mutable) or `let` (immutable) things.

Instances are denoted with `(this, ...`, mutable instances with `(var this, ...`. May be subject to change.

`func` is the keyword specifier for functions.

C's `char` and `int` are mapped to `CChar` and `CInt`, respectively.

Instead of literal suffixes (e.g. `0u8`), there are literal casts (e.g. `UInt8(0)`), more like Swift.

There is no `usize`, instead there is `UInt` & `Int` that map to C's `size_t` & `ssize_t`, respectively, which effectively serves the same purpose. That is, platform/width specific or 'native' primitives (e.g. 32-bit or 64-bit). Primitives created at lex time using C#'s BigInteger (which itself could be a native primitive I guess).

For pattern matching, I have opted to use the word `when` (borrowed from C#) rather than `match`. Felt cute, might delete later.