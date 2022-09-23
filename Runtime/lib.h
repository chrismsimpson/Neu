
#pragma once

#include <stdint.h>

#include "Array.h"
#include "Assertions.h"
#include "BitCast.h"
#include "ByteBuffer.h"
#include "Checked.h"
#include "CheckedFormatString.h"
#include "Detail.h"
#include "Error.h"
#include "Format.h"
#include "Function.h"
#include "HashFunctions.h"
#include "IterationDecision.h"
#include "Iterator.h"
#include "NonCopyable.h"
#include "NumericLimits.h"
#include "Optional.h"
#include "OwnPointer.h"
#include "Platform.h"
#include "RefCounted.h"
#include "ScopeGuard.h"
#include "Span.h"
#include "std.h"
#include "String.h"
#include "StringHash.h"
#include "StringView.h"
#include "Traits.h"
#include "Try.h"
#include "TypedTransfer.h"
#include "Types.h"
#include "Variant.h"


#include "kmalloc.h"

#include "Format.cpp"
#include "StringView.cpp"
#include "String.cpp"

using Float = float; // 32-bit
using Double = double; // 64-bit

static_assert(sizeof(Float) == 4);
static_assert(sizeof(Double) == 8);