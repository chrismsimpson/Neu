/*
 * Copyright (c) 2020-2022, the SerenityOS developers.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include "Array.h"
#include "Assertions.h"
#include "Span.h"
#include "Types.h"
#include "Vector.h"

namespace {

    namespace Detail {

        constexpr void const* bitapBitwise(void const* haystack, size_t haystackLength, void const* needle, size_t needleLength) {

            VERIFY(needleLength < 32);

            UInt64 lookup = 0xfffffffe;

            constexpr size_t mask_length = (size_t)((UInt8)-1) + 1;
            
            UInt64 needleMask[mask_length];

            for (size_t i = 0; i < mask_length; ++i) {

                needleMask[i] = 0xffffffff;
            }

            for (size_t i = 0; i < needleLength; ++i) {

                needleMask[((UInt8 const*) needle)[i]] &= ~(0x00000001 << i);
            }

            for (size_t i = 0; i < haystackLength; ++i) {

                lookup |= needleMask[((UInt8 const*) haystack)[i]];
                
                lookup <<= 1;

                if (0 == (lookup & (0x00000001 << needleLength))) {

                    return ((UInt8 const*) haystack) + i - needleLength + 1;
                }
            }

            return nullptr;
        }
    }

    ///

    template<typename HaystackIterT>
    inline Optional<size_t> memmem(HaystackIterT const& haystackBegin, HaystackIterT const& haystackEnd, Span<const UInt8> needle) requires(requires { (*haystackBegin).data(); (*haystackBegin).size(); }) {
        
        auto prepareKmpPartialTable = [&] {
            
            Vector<int, 64> table;
            
            table.resize(needle.size());

            size_t position = 1;
            
            int candidate = 0;

            table[0] = -1;

            while (position < needle.size()) {

                if (needle[position] == needle[candidate]) {
                    
                    table[position] = table[candidate];
                } 
                else {
                    
                    table[position] = candidate;
                    
                    do {
                        
                        candidate = table[candidate];
                    } 
                    while (candidate >= 0 && needle[candidate] != needle[position]);
                }

                ++position;
                
                ++candidate;
            }

            return table;
        };

        auto table = prepareKmpPartialTable();
        
        size_t totalHaystackIndex = 0;
        
        size_t currentHaystackIndex = 0;
        
        int needleIndex = 0;
        
        auto haystackIt = haystackBegin;

        while (haystackIt != haystackEnd) {

            auto&& chunk = *haystackIt;
            
            if (currentHaystackIndex >= chunk.size()) {
                
                currentHaystackIndex = 0;
                
                ++haystackIt;
                
                continue;
            }
            
            if (needle[needleIndex] == chunk[currentHaystackIndex]) {
                
                ++needleIndex;
                
                ++currentHaystackIndex;
                
                ++totalHaystackIndex;
                
                if ((size_t)needleIndex == needle.size()) {

                    return totalHaystackIndex - needleIndex;
                }
                
                continue;
            }
            
            needleIndex = table[needleIndex];
            
            if (needleIndex < 0) {
                
                ++needleIndex;
                
                ++currentHaystackIndex;
                
                ++totalHaystackIndex;
            }
        }

        return { };
    }

    inline Optional<size_t> memmemOptional(void const* haystack, size_t haystackLength, void const* needle, size_t needleLength) {

        if (needleLength == 0) {

            return 0;
        }

        if (haystackLength < needleLength) {

            return { };
        }

        if (haystackLength == needleLength) {

            if (__builtin_memcmp(haystack, needle, haystackLength) == 0) {

                return 0;
            }

            return { };
        }

        if (needleLength < 32) {

            auto const* ptr = Detail::bitapBitwise(haystack, haystackLength, needle, needleLength);
            
            if (ptr) {

                return static_cast<size_t>((FlatPointer) ptr - (FlatPointer) haystack);
            }
            
            return { };
        }

        // Fallback to KMP.
        Array<Span<const UInt8>, 1> spans { Span<const UInt8> { (UInt8 const*) haystack, haystackLength } };
        
        return memmem(spans.begin(), spans.end(), { (UInt8 const*) needle, needleLength });
    }

    inline void const* memmem(void const* haystack, size_t haystackLength, void const* needle, size_t needleLength) {

        auto offset = memmemOptional(haystack, haystackLength, needle, needleLength);

        if (offset.hasValue()) {

            return ((UInt8 const*) haystack) + offset.value();
        }

        return nullptr;
    }
}