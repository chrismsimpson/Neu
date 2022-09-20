#pragma once

#include <stdint.h>

#include "Format.h"

#include "Format.cpp"

template<typename Block>
class ScopeGuard {

public:

    ScopeGuard(
        Block block)
        : m_block(block) { }

    ~ScopeGuard() { m_block(); }

private:

    Block m_block;
};

void foo();

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