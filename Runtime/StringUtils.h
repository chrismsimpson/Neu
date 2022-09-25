
#pragma once

#include "Concepts.h"
#include "Forward.h"

namespace Detail {

    template<Concepts::AnyString T, Concepts::AnyString U>
    inline constexpr bool IsHashCompatible<T, U> = true;
}