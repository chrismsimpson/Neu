
#pragma once

#include "BuiltinWrappers.h"
#include "Concepts.h"
#include "Types.h"

template<Integral T>
constexpr T exp2(T exponent) {

    return 1u << exponent;
}

template<Integral T>
constexpr T log2(T x) {

    return x ? (8 * sizeof(T) - 1) - countLeadingZeroes(static_cast<MakeUnsigned<T>>(x)) : 0;
}

template<Integral I>
constexpr I pow(I base, I exponent) {

    // https://en.wikipedia.org/wiki/Exponentiation_by_squaring

    if (exponent < 0) {

        return 0;
    }

    if (exponent == 0) {

        return 1;
    }

    I res = 1;

    while (exponent > 0) {

        if (exponent & 1) {

            res *= base;
        }

        base *= base;
        
        exponent /= 2u;
    }

    return res;
}

