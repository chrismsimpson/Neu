/*
 * Copyright (c) 2020-2022, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include "Assertions.h"
#include "Checked.h"
#include "Optional.h"
#include "Platform.h"
#include "std.h"

template<typename OutputType, typename InputType>
ALWAYS_INLINE bool is(InputType& input) {

    if constexpr (requires { input.template fastIs<OutputType>(); }) {

        return input.template fastIs<OutputType>();
    }

    return dynamic_cast<CopyConst<InputType, OutputType>*>(&input);
}

template<typename OutputType, typename InputType>
ALWAYS_INLINE bool is(InputType* input) {

    return input && is<OutputType>(*input);
}

template<typename OutputType, typename InputType>
ALWAYS_INLINE CopyConst<InputType, OutputType>* verifyCast(InputType* input) {

    static_assert(IsBaseOf<InputType, OutputType>);
    
    VERIFY(!input || is<OutputType>(*input));
    
    return static_cast<CopyConst<InputType, OutputType>*>(input);
}

template<typename OutputType, typename InputType>
ALWAYS_INLINE CopyConst<InputType, OutputType>& verifyCast(InputType& input) {

    static_assert(IsBaseOf<InputType, OutputType>);

    VERIFY(is<OutputType>(input));

    return static_cast<CopyConst<InputType, OutputType>&>(input);
}

template<typename OutputType, typename InputType>
ALWAYS_INLINE Optional<OutputType> fallibleIntegerCast(InputType input) {

    static_assert(IsIntegral<InputType>);
    
    if (!isWithinRange<OutputType>(input)) {

        return { };
    }

    return static_cast<OutputType>(input);
}

template<typename OutputType, typename InputType>
ALWAYS_INLINE OutputType infallibleIntegerCast(InputType input) {

    static_assert(IsIntegral<InputType>);
    
    VERIFY(isWithinRange<OutputType>(input));
    
    return static_cast<OutputType>(input);
}

template<typename OutputType, typename InputType>
ALWAYS_INLINE OutputType saturatingIntegerCast(InputType input) {

    static_assert(IsIntegral<InputType>);
    
    if (!isWithinRange<OutputType>(input)) {
    
        if constexpr (IsSigned<InputType>) {
    
            if (input < 0) {

                return NumericLimits<OutputType>::min();
            }
        }

        return NumericLimits<OutputType>::max();
    }

    return static_cast<OutputType>(input);
}

template<typename OutputType, typename InputType>
ALWAYS_INLINE OutputType truncatingIntegerCast(InputType input) {
    
    static_assert(IsIntegral<InputType>);
    
    return static_cast<OutputType>(input);
}
