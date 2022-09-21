
#pragma once

#include "Array.h"
#include "Assertions.h"
#include "Iterator.h"
#include "Types.h"

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

    protected:

        T* m_values { nullptr };

        size_t m_size { 0 };
    };

} // namespace Detail
