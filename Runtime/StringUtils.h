
#pragma once

#include "Concepts.h"
#include "Forward.h"

namespace Detail {

    template<Concepts::AnyString T, Concepts::AnyString U>
    inline constexpr bool IsHashCompatible<T, U> = true;
}

enum class CaseSensitivity {

    CaseInsensitive,
    CaseSensitive,
};

enum class TrimMode {

    Left,
    Right,
    Both
};

enum class TrimWhitespace {

    Yes,
    No
};

struct MaskSpan {

    size_t start;
    
    size_t length;

    bool operator==(MaskSpan const& other) const {

        return start == other.start && length == other.length;
    }

    bool operator!=(MaskSpan const& other) const {

        return !(*this == other);
    }
};

///

namespace StringUtils {

    // bool matches(StringView str, StringView mask, CaseSensitivity = CaseSensitivity::CaseInsensitive, Vector<MaskSpan>* match_spans = nullptr);

    // template<typename T = int>
    // Optional<T> convertToInt(StringView, TrimWhitespace = TrimWhitespace::Yes);
}