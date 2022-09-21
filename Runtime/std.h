
#pragma once

#include "Detail.h"

namespace std {


    template<typename T>
    constexpr T&& move(T& arg) {

        return static_cast<T&&>(arg);
    }
    
} // namespace std

using std::move;
