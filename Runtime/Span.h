
#pragma once

#include "Array.h"
#include "Assertions.h"
#include "Iterator.h"
#include "TypedTransfer.h"
#include "Types.h"

///

namespace Detail {

    template<typename T>
    class Span {

    public:

        ALWAYS_INLINE constexpr Span() = default;

        ALWAYS_INLINE constexpr Span(T* values, size_t size)
            : m_values(values), 
              m_size(size) { }

        template<size_t size>
        ALWAYS_INLINE constexpr Span(T (&values)[size])
            : m_values(values), 
              m_size(size) { }

        // template<size_t size>
        // ALWAYS_INLINE constexpr Span(Array<T, size>& array)
        //     : m_values(array.data()), 
        //       m_size(size) { }

    protected:

        T* m_values { nullptr };

        size_t m_size { 0 };
    };

} // namespace Detail

///

template<typename T>
class Span: public Detail::Span<T> {

public: 
    
    using Detail::Span<T>::Span;

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr T const* data() const { return this->m_values; }

    [[nodiscard]] ALWAYS_INLINE constexpr T* data() { return this->m_values; }

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr T const* offsetPointer(size_t offset) const { return this->m_values + offset; }

    [[nodiscard]] ALWAYS_INLINE constexpr T* offsetPointer(size_t offset) { return this->m_values + offset; }

    ///

    using ConstIterator = SimpleIterator<Span const, T const>;

    using Iterator = SimpleIterator<Span, T>;

    ///

    constexpr ConstIterator begin() const { return ConstIterator::begin(*this); }

    constexpr Iterator begin() { return Iterator::begin(*this); }

    ///

    constexpr ConstIterator end() const { return ConstIterator::end(*this); }

    constexpr Iterator end() { return Iterator::end(*this); }

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr size_t size() const { return this->m_size; }

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr bool isNull() const { return this->m_values == nullptr; }

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr bool isEmpty() const { return this->m_size == 0; }

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr Span slice(size_t start, size_t length) const {

        VERIFY(start + length <= size());
        
        return { this->m_values + start, length };
    }

    [[nodiscard]] ALWAYS_INLINE constexpr Span slice(size_t start) const {

        VERIFY(start <= size());
        
        return { this->m_values + start, size() - start };
    }

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr Span sliceFromEnd(size_t count) const {

        VERIFY(count <= size());
        
        return { this->m_values + size() - count, count };
    }

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr Span trim(size_t length) const {

        return { this->m_values, min(size(), length) };
    }

    ///

    [[nodiscard]] ALWAYS_INLINE constexpr T* offset(size_t start) const {

        VERIFY(start < this->m_size);
        
        return this->m_values + start;
    }

    ///

    ALWAYS_INLINE constexpr void overwrite(size_t offset, void const* data, size_t dataSize) {

        // make sure we're not told to write past the end
        
        VERIFY(offset + dataSize <= size());
        
        __builtin_memmove(this->data() + offset, data, dataSize);
    }

    ///

    // ALWAYS_INLINE constexpr size_t copyTo(Span<RemoveConst<T>> other) const {

    //     VERIFY(other.size() >= size());
        
    //     return TypedTransfer<RemoveConst<T>>::copy(other.data(), data(), size());
    // }





};