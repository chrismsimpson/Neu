/*
 * Copyright (c) 2022, Leon Albrecht <leon2002.la@gmail.com>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/BuiltinWrappers.h>
#include <Core/Concepts.h>
#include <Core/Types.h>

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

