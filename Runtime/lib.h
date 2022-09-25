
#pragma once

#include <stdint.h>

#include "AllOf.h"
#include "AnyOf.h"
#include "Array.h"
#include "Assertions.h"
#include "Atomic.h"
#include "Badge.h"
#include "BitCast.h"
#include "BuiltinWrappers.h"
#include "ByteBuffer.h"
#include "Checked.h"
#include "CheckedFormatString.h"
#include "Detail.h"
#include "Error.h"
#include "Find.h"
#include "FlyString.h"
#include "Format.h"
#include "Function.h"
#include "HashFunctions.h"
#include "IterationDecision.h"
#include "Iterator.h"
#include "NonCopyable.h"
#include "NonNullOwnPointer.h"
#include "NonNullRefPointer.h"
#include "NumericLimits.h"
#include "Optional.h"
#include "OwnPointer.h"
#include "Platform.h"
#include "RefCounted.h"
#include "RefPointer.h"
#include "ScopeGuard.h"
#include "Span.h"
#include "std.h"
#include "String.h"
#include "StringBuilder.h"
#include "StringHash.h"
#include "StringImpl.h"
#include "StringUtils.h"
#include "StringView.h"
#include "Traits.h"
#include "Try.h"
#include "TypedTransfer.h"
#include "Types.h"
#include "Variant.h"
#include "Vector.h"
#include "WeakPointer.h"

#include "kmalloc.h"

#include "FlyString.h"
#include "Format.cpp"
#include "String.cpp"
#include "StringBuilder.cpp"
#include "StringImpl.cpp"
#include "StringUtils.cpp"
#include "StringView.cpp"

using Float = float; // 32-bit
using Double = double; // 64-bit

static_assert(sizeof(Float) == 4);
static_assert(sizeof(Double) == 8);