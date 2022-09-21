
#pragma once

#include "Array.h"
#include "std.h"

namespace Format::Detail {

    template<typename... Args>
    struct CheckedFormatString {

        template<size_t N>
        consteval CheckedFormatString(
            char const (&fmt)[N]) {

        }
    };
    
} // namespace Format::Detail

template<typename... Args>
using CheckedFormatString = Format::Detail::CheckedFormatString<IdentityType<Args>...>;