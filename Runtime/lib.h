#pragma once

#include <stdint.h>

#include "Assertions.h"
#include "Checked.h"
#include "CheckedFormatString.h"
#include "Detail.h"
#include "Format.h"
#include "IterationDecision.h"
#include "Platform.h"
#include "ScopeGuard.h"
#include "Span.h"
#include "std.h"
#include "StringView.h"
#include "Types.h"

#include "Format.cpp"
#include "StringView.cpp"

using Float = float; // 32-bit
using Double = double; // 64-bit

static_assert(sizeof(Float) == 4);
static_assert(sizeof(Double) == 8);