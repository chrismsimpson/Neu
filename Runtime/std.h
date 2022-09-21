
#pragma once

#include "Detail.h"









namespace std {

    template<typename T>
    constexpr T&& move(T& arg) {

        return static_cast<T&&>(arg);
    }
    
} // namespace std

using std::move;




constexpr bool isConstantEvaluated() {

#if __has_builtin(__builtin_is_constant_evaluated)
    return __builtin_is_constant_evaluated();
#else
    return false;
#endif
}