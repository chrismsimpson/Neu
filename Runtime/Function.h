
#pragma once


#include "Assertions.h"
#include "Atomic.h"
#include "BitCast.h"
#include "NonCopyable.h"
#include "ScopeGuard.h"
#include "std.h"
#include "Types.h"

template<typename>
class Function;

template<typename F>
inline constexpr bool IsFunctionPointer = (IsPointer<F> && IsFunction<RemovePointer<F>>);

// Not a function pointer, and not an lvalue reference.
template<typename F>
inline constexpr bool IsFunctionObject = (!IsFunctionPointer<F> && IsRValueReference<F&&>);


// template<typename Out, typename... In>
// class Function<Out(In...)> {
//     MAKE_NONCOPYABLE(Function);