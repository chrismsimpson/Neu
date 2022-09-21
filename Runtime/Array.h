
#pragma once

#include "Iterator.h"
#include "Span.h"

template<typename T, size_t Size>
struct Array {

    using ValueType = T;

    ///

    [[nodiscard]] constexpr T const* data() const { return __data; }

    [[nodiscard]] constexpr T* data() { return __data; }

    ///

    [[nodiscard]] constexpr size_t size() const { return Size; }

    ///

    [[nodiscard]] constexpr Span<T const> span() const { return { __data, Size }; }

    [[nodiscard]] constexpr Span<T> span() { return { __data, Size }; }

    ///

    [[nodiscard]] constexpr T const& at(size_t index) const {

        VERIFY(index < size());

        return __data[index];
    }

    [[nodiscard]] constexpr T& at(size_t index) {

        VERIFY(index < size());

        return __data[index];
    }

    ///

    [[nodiscard]] constexpr T const& first() const { return at(0); }

    [[nodiscard]] constexpr T& first() { return at(0); }

    ///

    [[nodiscard]] constexpr T const& last() const requires(Size > 0) { return at(Size - 1); }

    [[nodiscard]] constexpr T& last() requires(Size > 0) { return at(Size - 1); }

    ///

    [[nodiscard]] constexpr bool isEmpty() const { return size() == 0; }

    ///

    [[nodiscard]] constexpr T const& operator[](size_t index) const { return at(index); }

    [[nodiscard]] constexpr T& operator[](size_t index) { return at(index); }

    ///

    template<typename T2, size_t Size2>
    [[nodiscard]] constexpr bool operator==(Array<T2, Size2> const& other) const { return span() == other.span(); }

    ///




    T __data[Size];
};