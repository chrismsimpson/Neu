/*
 * Copyright (c) 2018-2021, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Assertions.h>
#include <Core/Checked.h>
#include <Core/Forward.h>
#include <Core/Optional.h>
#include <Core/Span.h>
#include <Core/std.h>
#include <Core/StringHash.h>
#include <Core/StringUtils.h>

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

    ALWAYS_INLINE constexpr StringView(char const* cstring)
        : m_characters(cstring),
          m_length(cstring 
            ? __builtin_strlen(cstring) 
            : 0) { }

    ALWAYS_INLINE StringView(ReadOnlyBytes bytes)
        : m_characters(reinterpret_cast<char const*>(bytes.data())), 
          m_length(bytes.size()) { }

#ifndef KERNEL

    StringView(String const&);

#endif

#ifndef KERNEL

    explicit StringView(String&&) = delete;

#endif

    [[nodiscard]] constexpr bool isNull() const {

        return m_characters == nullptr;
    }

    [[nodiscard]] constexpr bool isEmpty() const { return m_length == 0; }

    [[nodiscard]] constexpr char const* charactersWithoutNullTermination() const { return m_characters; }

    [[nodiscard]] constexpr size_t length() const { return m_length; }

    [[nodiscard]] ReadOnlyBytes bytes() const { return { m_characters, m_length }; }

    constexpr char const& operator[](size_t index) const { return m_characters[index]; }

    using ConstIterator = SimpleIterator<const StringView, char const>;

    [[nodiscard]] constexpr ConstIterator begin() const { return ConstIterator::begin(*this); }
    
    [[nodiscard]] constexpr ConstIterator end() const { return ConstIterator::end(*this); }


    [[nodiscard]] constexpr unsigned hash() const {

        if (isEmpty()) {

            return 0;
        }

        return stringHash(charactersWithoutNullTermination(), length());
    }

    [[nodiscard]] bool startsWith(StringView, CaseSensitivity = CaseSensitivity::CaseSensitive) const;
    
    [[nodiscard]] bool endsWith(StringView, CaseSensitivity = CaseSensitivity::CaseSensitive) const;

    [[nodiscard]] bool startsWith(char) const;
    
    [[nodiscard]] bool endsWith(char) const;

    [[nodiscard]] bool matches(StringView mask, CaseSensitivity = CaseSensitivity::CaseInsensitive) const;

    [[nodiscard]] bool matches(StringView mask, Vector<MaskSpan>&, CaseSensitivity = CaseSensitivity::CaseInsensitive) const;

    [[nodiscard]] bool contains(char) const;

    [[nodiscard]] bool contains(StringView, CaseSensitivity = CaseSensitivity::CaseSensitive) const;

    [[nodiscard]] bool equalsIgnoringCase(StringView other) const;

    [[nodiscard]] StringView trim(StringView characters, TrimMode mode = TrimMode::Both) const { return StringUtils::trim(*this, characters, mode); }
    
    [[nodiscard]] StringView trimWhitespace(TrimMode mode = TrimMode::Both) const { return StringUtils::trimWhitespace(*this, mode); }

#ifndef KERNEL
    
    [[nodiscard]] String toLowercaseString() const;

    [[nodiscard]] String toUppercaseString() const;
    
    [[nodiscard]] String toTitlecaseString() const;

#endif

    [[nodiscard]] Optional<size_t> find(char needle, size_t start = 0) const {

        return StringUtils::find(*this, needle, start);
    }

    [[nodiscard]] Optional<size_t> find(StringView needle, size_t start = 0) const { return StringUtils::find(*this, needle, start); }
    
    [[nodiscard]] Optional<size_t> findLast(char needle) const { return StringUtils::findLast(*this, needle); }

    // FIXME: Implement find_last(StringView) for API symmetry.

    [[nodiscard]] Vector<size_t> findAll(StringView needle) const;

    using SearchDirection = StringUtils::SearchDirection;

    [[nodiscard]] Optional<size_t> findAnyOf(StringView needles, SearchDirection direction = SearchDirection::Forward) const { return StringUtils::findAnyOf(*this, needles, direction); }

    [[nodiscard]] constexpr StringView substringView(size_t start, size_t length) const {

        if (!isConstantEvaluated()) {

            VERIFY(start + length <= m_length);
        }

        return { m_characters + start, length };
    }

    [[nodiscard]] constexpr StringView substringView(size_t start) const {

        if (!isConstantEvaluated()) {

            VERIFY(start <= length());
        }

        return substringView(start, length() - start);
    }

    [[nodiscard]] Vector<StringView> splitView(char, bool keepEmpty = false) const;

    [[nodiscard]] Vector<StringView> splitView(StringView, bool keepEmpty = false) const;

    [[nodiscard]] Vector<StringView> splitViewIf(Function<bool(char)> const& predicate, bool keepEmpty = false) const;

    template<VoidFunction<StringView> Callback>
    void forEachSplitView(char separator, bool keepEmpty, Callback callback) const {

        StringView seperatorView { &separator, 1 };

        forEachSplitView(seperatorView, keepEmpty, callback);
    }


    template<VoidFunction<StringView> Callback>
    void forEachSplitView(StringView separator, bool keepEmpty, Callback callback) const {

        VERIFY(!separator.isEmpty());

        if (isEmpty()) {

            return;
        }

        StringView view { *this };

        auto maybeSeparatorIndex = find(separator);
        
        while (maybeSeparatorIndex.hasValue()) {
            
            auto separatorIndex = maybeSeparatorIndex.value();
            
            auto partWithSeparator = view.substringView(0, separatorIndex + separator.length());
            
            if (keepEmpty || separatorIndex > 0) {

                callback(partWithSeparator.substringView(0, separatorIndex));
            }
            
            view = view.substringViewStartingAfterSubstring(partWithSeparator);
            
            maybeSeparatorIndex = view.find(separator);
        }

        if (keepEmpty || !view.isEmpty()) {

            callback(view);
        }
    }

    // Create a Vector of StringViews split by line endings. As of CommonMark
    // 0.29, the spec defines a line ending as "a newline (U+000A), a carriage
    // return (U+000D) not followed by a newline, or a carriage return and a
    // following newline.".

    [[nodiscard]] Vector<StringView> lines(bool considerCarriageReturn = true) const;

    template<typename T = int>
    Optional<T> toInt() const;

    template<typename T = unsigned>
    Optional<T> toUInt() const;

    // Create a new substring view of this string view, starting either at the beginning of
    // the given substring view, or after its end, and continuing until the end of this string
    // view (that is, for the remaining part of its length). For example,
    //
    //    StringView str { "foobar" };
    //    StringView substr = str.substringView(1, 2);  // "oo"
    //    StringView substrFrom = str.substringViewStartingFromSubstring(subst);  // "oobar"
    //    StringView substrAfter = str.substringViewStartingAfterSubstring(subst);  // "bar"
    //
    // Note that this only works if the string view passed as an argument is indeed a substring
    // view of this string view, such as one created by substringView() and splitView(). It
    // does not work for arbitrary strings; for example declaring substr in the example above as
    //
    //     StringView substr { "oo" };
    //
    // would not work.

    [[nodiscard]] StringView substringViewStartingFromSubstring(StringView substring) const;

    [[nodiscard]] StringView substringViewStartingAfterSubstring(StringView substring) const;

    [[nodiscard]] bool copyCharactersToBuffer(char* buffer, size_t bufferSize) const;

    constexpr bool operator==(char const* cstring) const {

        if (isNull()) {

            return cstring == nullptr;
        }

        if (!cstring) {

            return false;
        }
        
        // NOTE: `m_characters` is not guaranteed to be null-terminated, but `cstring` is.
        
        char const* cp = cstring;
        
        for (size_t i = 0; i < m_length; ++i) {

            if (*cp == '\0') {

                return false;
            }

            if (m_characters[i] != *(cp++)) {

                return false;
            }
        }

        return *cp == '\0';
    }

    constexpr bool operator!=(char const* cstring) const {

        return !(*this == cstring);
    }

#ifndef KERNEL

    bool operator==(String const&) const;

#endif

    [[nodiscard]] constexpr int compare(StringView other) const {

        if (m_characters == nullptr) {

            return other.m_characters ? -1 : 0;
        }

        if (other.m_characters == nullptr) {

            return 1;
        }

        size_t rlen = min(m_length, other.m_length);

        int c = __builtin_memcmp(m_characters, other.m_characters, rlen);

        if (c == 0) {

            if (length() < other.length()) {

                return -1;
            }
            
            if (length() == other.length()) {

                return 0;
            }
            
            return 1; 
        }

        return c;
    }

    constexpr bool operator==(StringView other) const {

        return length() == other.length() && compare(other) == 0;
    }

    constexpr bool operator!=(StringView other) const {

        return length() != other.length() || compare(other) != 0;
    }

    constexpr bool operator<(StringView other) const { return compare(other) < 0; }

    constexpr bool operator<=(StringView other) const { return compare(other) <= 0; }

    constexpr bool operator>(StringView other) const { return compare(other) > 0; }

    constexpr bool operator>=(StringView other) const { return compare(other) >= 0; }

#ifndef KERNEL

    [[nodiscard]] String toString() const;

#endif

    [[nodiscard]] bool isWhitespace() const {

        return StringUtils::isWhitespace(*this);
    }

#ifndef KERNEL

    [[nodiscard]] String replace(StringView needle, StringView replacement, bool allOccurrences = false) const;

#endif

    [[nodiscard]] size_t count(StringView needle) const {

        return StringUtils::count(*this, needle);
    }

    template<typename... Ts>
    [[nodiscard]] ALWAYS_INLINE constexpr bool isOneOf(Ts&&... strings) const {

        return (... || this->operator==(forward<Ts>(strings)));
    }

    template<typename... Ts>
    [[nodiscard]] ALWAYS_INLINE constexpr bool isOneOfIgnoringCase(Ts&&... strings) const {

        return (... ||
                [this, &strings]() -> bool {
            if constexpr (requires(Ts a) { a.view()->StringView; }) {

                return this->equalsIgnoringCase(forward<Ts>(strings.view()));
            }
            else {

                return this->equalsIgnoringCase(forward<Ts>(strings));
            }
        }());
    }

private:

    friend class String;

    char const* m_characters { nullptr };

    size_t m_length { 0 };
};

template<>
struct Traits<StringView> : public GenericTraits<StringView> {

    static unsigned hash(StringView s) { return s.hash(); }
};

struct CaseInsensitiveStringViewTraits : public Traits<StringView> {

    static unsigned hash(StringView s) {

        if (s.isEmpty()) {

            return 0;
        }

        return caseInsensitiveStringHash(s.charactersWithoutNullTermination(), s.length());
    }
};

// FIXME: Remove this when clang fully supports consteval (specifically in the context of default parameter initialization).
// See: https://stackoverflow.com/questions/68789984/immediate-function-as-default-function-argument-initializer-in-clang

#if defined(__clang__)
#    define STRING_VIEW_LITERAL_CONSTEVAL constexpr
#else
#    define STRING_VIEW_LITERAL_CONSTEVAL consteval
#endif

[[nodiscard]] ALWAYS_INLINE STRING_VIEW_LITERAL_CONSTEVAL StringView operator"" sv(char const* cstring, size_t length) {

    return StringView(cstring, length);
}