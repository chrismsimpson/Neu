/*
 * Copyright (c) 2020, the SerenityOS developers.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Forward.h>
#include <Core/std.h>

namespace Concepts {

    template<typename T>
    concept Integral = IsIntegral<T>;
    
    ///

    template<typename T>
    concept FloatingPoint = IsFloatingPoint<T>;

    ///

    template<typename T>
    concept Fundamental = IsFundamental<T>;

    ///

    template<typename T>
    concept Arithmetic = IsArithmetic<T>;

    ///

    template<typename T>
    concept Signed = IsSigned<T>;

    ///

    template<typename T>
    concept Unsigned = IsUnsigned<T>;

    ///

    template<typename T>
    concept Enum = IsEnum<T>;

    ///

    template<typename T, typename U>
    concept SameAs = IsSame<T, U>;

    ///

    template<typename U, typename... Ts>
    concept OneOf = IsOneOf<U, Ts...>;

    ///

    template<typename U, typename... Ts>
    concept OneOfIgnoringConstVolatile = IsOneOfIgnoringConstVolatile<U, Ts...>;

    ///

    template<typename T, template<typename...> typename S>
    concept SpecializationOf = IsSpecializationOf<T, S>;

    ///

    template<typename T>
    concept AnyString = Detail::IsConstructible<StringView, T>;

    ///

    template<typename T, typename U>
    concept HashCompatible = IsHashCompatible<Detail::Decay<T>, Detail::Decay<U>>;

    ///

    // Any indexable, sized, contiguous data structure.
    template<typename ArrayT, typename ContainedT, typename SizeT = size_t>
    concept ArrayLike = requires(ArrayT array, SizeT index) {

        {
            array[index]
        }
        -> SameAs<RemoveReference<ContainedT>&>;

        {
            array.size()
        }
        -> SameAs<SizeT>;

        {
            array.span()
        }
        -> SameAs<Span<RemoveReference<ContainedT>>>;

        {
            array.data()
        }
        -> SameAs<RemoveReference<ContainedT>*>;
    };

    ///

    template<typename Func, typename... Args>
    concept VoidFunction = requires(Func func, Args... args) {

        {
            func(args...)
        }
        -> SameAs<void>;
    };

    ///

    template<typename T, typename EndT>
    concept IteratorPairWith = requires(T it, EndT end) {
        
        *it;
        { it != end } -> SameAs<bool>;
        ++it;
    };

    ///

    template<typename T>
    concept IterableContainer = requires {

        { declval<T>().begin() } -> IteratorPairWith<decltype(declval<T>().end())>;
    };
}

using Concepts::Arithmetic;
using Concepts::ArrayLike;
using Concepts::Enum;
using Concepts::FloatingPoint;
using Concepts::Fundamental;
using Concepts::Integral;
using Concepts::IterableContainer;
using Concepts::IteratorPairWith;
using Concepts::OneOf;
using Concepts::OneOfIgnoringConstVolatile;
using Concepts::SameAs;
using Concepts::Signed;
using Concepts::SpecializationOf;
using Concepts::Unsigned;
using Concepts::VoidFunction;