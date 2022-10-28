/*
 * Copyright (c) 2020-2022, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Assertions.h>
#include <Core/Checked.h>
#include <Core/Optional.h>
#include <Core/Platform.h>
#include <Core/std.h>

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
ALWAYS_INLINE bool is(NonNullRefPointer<InputType> const& input) {

    return is<OutputType>(*input);
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
