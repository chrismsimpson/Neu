/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 * Copyright (c) 2020, Fei Wu <f.eiwu@yahoo.com>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

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

    bool matches(StringView str, StringView mask, CaseSensitivity = CaseSensitivity::CaseInsensitive, Vector<MaskSpan>* match_spans = nullptr);

    template<typename T = int>
    Optional<T> convertToInt(StringView, TrimWhitespace = TrimWhitespace::Yes);

    template<typename T = unsigned>
    Optional<T> convertToUInt(StringView, TrimWhitespace = TrimWhitespace::Yes);
    
    template<typename T = unsigned>
    Optional<T> convertToUintFromHex(StringView, TrimWhitespace = TrimWhitespace::Yes);
    
    template<typename T = unsigned>
    Optional<T> convertToUintFromOctal(StringView, TrimWhitespace = TrimWhitespace::Yes);
        
    bool equalsIgnoringCase(StringView, StringView);
    
    bool endsWith(StringView a, StringView b, CaseSensitivity);
    
    bool startsWith(StringView, StringView, CaseSensitivity);
    
    bool contains(StringView, StringView, CaseSensitivity);
    
    bool isWhitespace(StringView);
    
    StringView trim(StringView string, StringView characters, TrimMode mode);
    
    StringView trimWhitespace(StringView string, TrimMode mode);

    Optional<size_t> find(StringView haystack, char needle, size_t start = 0);
    
    Optional<size_t> find(StringView haystack, StringView needle, size_t start = 0);
    
    Optional<size_t> findLast(StringView haystack, char needle);
    
    Vector<size_t> findAll(StringView haystack, StringView needle);
    
    enum class SearchDirection {
        Forward,
        Backward
    };
    
    Optional<size_t> findAnyOf(StringView haystack, StringView needles, SearchDirection);

    String toSnakecase(StringView);
    
    String toTitlecase(StringView);

    String replace(StringView, StringView needle, StringView replacement, bool allOccurrences = false);
    
    size_t count(StringView, StringView needle);
}