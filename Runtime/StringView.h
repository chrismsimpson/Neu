
#pragma once

#include "Assertions.h"
#include "Checked.h"
#include "Forward.h"
#include "Span.h"
#include "std.h"

class StringView {

public:

    ALWAYS_INLINE constexpr StringView() = default;

    ALWAYS_INLINE constexpr StringView(
        char const* characters, 
        size_t length)
        : m_characters(characters),
          m_length(length) {

        if (!isConstantEvaluated()) {

            VERIFY(!Checked<uintptr_t>::additionWouldOverflow((uintptr_t)characters, length));
        }
    }

    ALWAYS_INLINE StringView(unsigned char const* characters, size_t length)
        : m_characters((char const*) characters),
          m_length(length) {

        VERIFY(!Checked<uintptr_t>::additionWouldOverflow((uintptr_t)characters, length));
    }

    // ALWAYS_INLINE StringView(ReadonlyBytes bytes)
    //     : m_characters(reinterpret_cast<char const*>(bytes.data())), 
    //       m_length(bytes.size()) { }

private:

    char const* m_characters { nullptr };

    size_t m_length { 0 };
};
