/*
 * Copyright (c) 2018-2022, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <string.h>

#include "BitCast.h"
#include "Concepts.h"
#include "Forward.h"
#include "HashFunctions.h"
#include "StringHash.h"

template<typename T>
struct GenericTraits {

    using PeekType = T&;
    
    using ConstPeekType = T const&;

    ///

    static constexpr bool isTrivial() { return false; }

    ///

    static constexpr bool equals(const T& a, const T& b) { return a == b; }

    ///

    template<Concepts::HashCompatible<T> U>
    static bool equals(U const& a, T const& b) { return a == b; }
};

///

template<typename T>
struct Traits : public GenericTraits<T> { };

///

template<typename T>
requires(IsIntegral<T>) struct Traits<T> : public GenericTraits<T> {

    static constexpr bool isTrivial() { return true; }

    static constexpr unsigned hash(T value) {

        if constexpr (sizeof(T) < 8) {

            return hashUInt32(value);
        }
        else {

            return hashUInt64(value);
        }
    }
};

#ifndef KERNEL

template<typename T>
requires(IsFloatingPoint<T>) struct Traits<T> : public GenericTraits<T> {

    static constexpr bool isTrivial() { return true; }

    static constexpr unsigned hash(T value) {

        if constexpr (sizeof(T) < 8) {

            return hashUInt32(bitCast<UInt32>(value));
        }
        else {

            return hashUInt64(bitCast<UInt64>(value));
        }
    }
};

#endif

template<typename T>
requires(IsPointer<T> && !Detail::IsPointerOfType<char, T>) struct Traits<T> : public GenericTraits<T> {

    static unsigned hash(T p) { return hashPointer((FlatPointer) p); }

    static constexpr bool isTrivial() { return true; }
};

///

template<Enum T>
struct Traits<T> : public GenericTraits<T> {

    static unsigned hash(T value) { return Traits<UnderlyingType<T>>::hash(toUnderlying(value)); }

    static constexpr bool isTrivial() { return Traits<UnderlyingType<T>>::isTrivial(); }
};

///

template<typename T>
requires(Detail::IsPointerOfType<char, T>) struct Traits<T> : public GenericTraits<T> {

    static unsigned hash(T const value) { return stringHash(value, strlen(value)); }
    
    static constexpr bool equals(T const a, T const b) { return strcmp(a, b); }
    
    static constexpr bool isTrivial() { return true; }
};