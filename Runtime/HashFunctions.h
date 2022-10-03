/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include "Types.h"

constexpr unsigned hashUInt32(UInt32 key) {
    
    key += ~(key << 15);
    
    key ^= (key >> 10);
    
    key += (key << 3);
    
    key ^= (key >> 6);
    
    key += ~(key << 11);
    
    key ^= (key >> 16);
    
    return key;
}

constexpr unsigned doubleHashUInt32(UInt32 key) {

    unsigned const magic = 0xBA5EDB01;

    if (key == magic) {

        return 0u;
    }

    if (key == 0u) {

        key = magic;
    }

    key ^= key << 13;
    
    key ^= key >> 17;
    
    key ^= key << 5;
    
    return key;
}

///

constexpr unsigned hashPairUInt32(UInt32 key1, UInt32 key2) {

    return hashUInt32((hashUInt32(key1) * 209) ^ (hashUInt32(key2 * 413)));
}

///

constexpr unsigned hashUInt64(UInt64 key) {

    UInt32 first = key & 0xFFFFFFFF;
    
    UInt32 last = key >> 32;
    
    return hashPairUInt32(first, last);
}

///

constexpr unsigned hashPointer(FlatPointer ptr) {

    if constexpr (sizeof(ptr) == 8) {

        return hashUInt64(ptr);
    }
    else {

        return hashUInt32(ptr);
    }
}

///

inline unsigned hashPointer(void const* ptr) {

    return hashPointer(FlatPointer(ptr));
}
