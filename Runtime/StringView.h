
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

            // TODO 
        }
    }

private:

    char const* m_characters { nullptr };

    size_t m_length { 0 };
};
