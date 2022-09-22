
#pragma once

#include "Detail.h"

#include "Assertions.h"



///

namespace std {

    template<typename T>
    constexpr T&& forward(Detail::RemoveReference<T>& param) {

        return static_cast<T&&>(param);
    }

    template<typename T>
    constexpr T&& forward(Detail::RemoveReference<T>&& param) noexcept {

        static_assert(!IsLValueReference<T>, "Can't forward an rvalue as an lvalue.");
        
        return static_cast<T&&>(param);
    }

    template<typename T>
    constexpr T&& move(T& arg) {

        return static_cast<T&&>(arg);
    }
    
} // namespace std

using std::forward;
using std::move;

///

namespace Detail {

    template<typename T>
    struct _RawPointer {

        using Type = T*;
    };
}

///

template<typename T, typename SizeType = decltype(sizeof(T)), SizeType N>
constexpr SizeType arraySize(T (&)[N]) {

    return N;
}

///

template<typename T>
constexpr T min(const T& a, IdentityType<T> const& b) {

    return b < a ? b : a;
}

///

template<typename T>
constexpr T max(const T& a, IdentityType<T> const& b) {

    return a < b ? b : a;
}

///




///

template<typename T, typename U = T>
constexpr T exchange(T& slot, U&& value) {

    T oldValue = move(slot);

    slot = forward<U>(value);

    return oldValue;
}

///

template<typename T>
using RawPointer = typename Detail::_RawPointer<T>::Type;

///









constexpr bool isConstantEvaluated() {

#if __has_builtin(__builtin_is_constant_evaluated)
    return __builtin_is_constant_evaluated();
#else
    return false;
#endif
}