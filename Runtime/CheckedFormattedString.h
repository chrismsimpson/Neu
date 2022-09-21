
#pragma once

#include "std.h"

namespace Format::Detail {

    template<typename... Args>
    struct CheckedFormatString {

    };
    
} // namespace Format::Detail

template<typename... Args>
using CheckedFormatString = Format::Detail::CheckedFormatString<IdentityType<Args>...>;