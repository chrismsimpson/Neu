
#pragma once

#include "Traits.h"

constexpr UInt32 stringHash(char const* characters, size_t length, UInt32 seed = 0) {

    UInt32 hash = seed;

    for (size_t i = 0; i < length; ++i) {

        hash += (UInt32) characters[i];
        
        hash += (hash << 10);
        
        hash ^= (hash >> 6);
    }

    hash += hash << 3;
    
    hash ^= hash >> 11;
    
    hash += hash << 15;

    return hash;
}

constexpr UInt32 caseInsensitiveStringHash(char const* characters, size_t length, UInt32 seed = 0) {

    // CharacterTypes.h cannot be included from here.

    auto toLowercase = [](char ch) -> UInt32 {

        if (ch >= 'A' && ch <= 'Z') {

            return static_cast<UInt32>(ch) + 0x20;
        }

        return static_cast<UInt32>(ch);
    };

    UInt32 hash = seed;

    for (size_t i = 0; i < length; ++i) {

        hash += toLowercase(characters[i]);
        
        hash += (hash << 10);
        
        hash ^= (hash >> 6);
    }

    hash += hash << 3;
    
    hash ^= hash >> 11;
    
    hash += hash << 15;
    
    return hash;
}