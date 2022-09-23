
#pragma once

template<typename>
class Optional;

template<typename T>
requires(!IsLValueReference<T>) class [[nodiscard]] Optional<T> {

    template<typename U>
    friend class Optional;

    static_assert(!IsLValueReference<T> && !IsRValueReference<T>);

    
};