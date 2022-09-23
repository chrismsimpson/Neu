
#pragma once

template<typename>
class Optional;

template<typename T>
requires(!IsLValueReference<T>) class [[nodiscard]] Optional<T> {
    
};