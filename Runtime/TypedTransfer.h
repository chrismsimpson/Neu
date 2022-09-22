
#pragma once

#include "Traits.h"

template<typename T>
class TypedTransfer {

public: 

    // static void move(T* destination, T* source, size_t count) {

    //     if (count == 0) {

    //         return;
    //     }

    //     if constexpr (Traits<T>::isTrivial()) {
    //         __builtin_memmove(destination, source, count * sizeof(T));
    //         return;
    //     }

    //     for (size_t i = 0; i < count; ++i) {
    //         if (destination <= source) {

    //             new (&destination[i]) T(std::move(source[i]));
    //         }
    //         else {

    //             new (&destination[count - i - 1]) T(std::move(source[count - i - 1]));
    //         }
    //     }
    // }

};