
#pragma once

#include <string.h>

#include "BitCast.h"
#include "Concepts.h"
#include "Forward.h"
#include "HashFunctions.h"
// #include "StringHash.h"

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