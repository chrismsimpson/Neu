#pragma once

#include <stdint.h>

#include "Assertions.h"
#include "Checked.h"
#include "CheckedFormatString.h"
#include "Detail.h"
#include "Format.h"
#include "Platform.h"
#include "ScopeGuard.h"
#include "std.h"
#include "StringView.h"
#include "Types.h"

#include "Format.cpp"
#include "StringView.cpp"

using Int8 = int8_t;
using Int16 = int16_t;
using Int32 = int32_t;
using Int64 = int64_t;
using UInt8 = uint8_t;
using UInt16 = uint16_t;
using UInt32 = uint32_t;
using UInt64 = uint64_t;

using Float = float; // 32-bit
using Double = double; // 64-bit

static_assert(sizeof(Float) == 4);
static_assert(sizeof(Double) == 8);