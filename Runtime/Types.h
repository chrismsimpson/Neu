
#pragma once

#include "IterationDecision.h"
#include "Platform.h"
#include "std.h"

using UInt64 = __UINT64_TYPE__;
using UInt32 = __UINT32_TYPE__;
using UInt16 = __UINT16_TYPE__;
using UInt8 = __UINT8_TYPE__;
using Int64 = __INT64_TYPE__;
using Int32 = __INT32_TYPE__;
using Int16 = __INT16_TYPE__;
using Int8 = __INT8_TYPE__;

#include <stddef.h>
#include <stdint.h>
#include <sys/types.h>

using FlatPointer = Conditional<sizeof(void*) == 8, UInt64, UInt32>;

enum MemoryOrder {
    
    memory_order_relaxed = __ATOMIC_RELAXED,
    memory_order_consume = __ATOMIC_CONSUME,
    memory_order_acquire = __ATOMIC_ACQUIRE,
    memory_order_release = __ATOMIC_RELEASE,
    memory_order_acq_rel = __ATOMIC_ACQ_REL,
    memory_order_seq_cst = __ATOMIC_SEQ_CST
};