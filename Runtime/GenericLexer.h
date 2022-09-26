
#pragma once

#include "Result.h"
#include "StringView.h"

class GenericLexer {

public:

    constexpr explicit GenericLexer(StringView input)
        : m_input(input) { }

    constexpr size_t tell() const { return m_index; }
    
    constexpr size_t tellRemaining() const { return m_input.length() - m_index; }

    StringView remaining() const { return m_input.substringView(m_index); }

    constexpr bool isEof() const { return m_index >= m_input.length(); }

    constexpr char peek(size_t offset = 0) const {

        return (m_index + offset < m_input.length()) ? m_input[m_index + offset] : '\0';
    }

    ///

    constexpr bool nextIs(char expected) const {

        return peek() == expected;
    }

    constexpr bool nextIs(StringView expected) const {

        for (size_t i = 0; i < expected.length(); ++i) {

            if (peek(i) != expected[i]) {

                return false;
            }
        }

        return true;
    }

    constexpr bool nextIs(char const* expected) const {

        for (size_t i = 0; expected[i] != '\0'; ++i) {

            if (peek(i) != expected[i]) {

                return false;
            }
        }

        return true;
    }

    ///

    constexpr void retreat() {

        VERIFY(m_index > 0);
        
        --m_index;
    }

    constexpr void retreat(size_t count) {

        VERIFY(m_index >= count);
        
        m_index -= count;
    }

    ///

    constexpr char consume() {

        VERIFY(!isEof());

        return m_input[m_index++];
    }

    template<typename T>
    constexpr bool consumeSpecific(const T& next) {

        if (!nextIs(next)) {

            return false;
        }

        if constexpr (requires { next.length(); }) {

            ignore(next.length());
        } 
        else {

            ignore(sizeof(next));
        }

        return true;
    }

#ifndef OS

    bool consumeSpecific(String const& next) {

        return consumeSpecific(StringView { next });
    }

#endif

    constexpr bool consumeSpecific(char const* next) {

        return consumeSpecific(StringView { next });
    }

    constexpr char consumeEscapedCharacter(char escapeChar = '\\', StringView escapeMap = "n\nr\rt\tb\bf\f") {

        if (!consumeSpecific(escapeChar)) {

            return consume();
        }

        auto c = consume();

        for (size_t i = 0; i < escapeMap.length(); i += 2) {

            if (c == escapeMap[i]) {

                return escapeMap[i + 1];
            }
        }

        return c;
    }

    StringView consume(size_t count);
    
    StringView consumeAll();
    
    StringView consumeLine();
    
    StringView consumeUntil(char);
    
    StringView consumeUntil(char const*);
    
    StringView consumeUntil(StringView);
    
    StringView consumeQuotedString(char escapeChar = 0);

#ifndef OS

    String consumeAndUnescapeString(char escapeChar = '\\');

#endif

    enum class UnicodeEscapeError {
        
        MalformedUnicodeEscape,
        UnicodeEscapeOverflow
    };

#ifndef OS
    
    Result<UInt32, UnicodeEscapeError> consumeEscapedCodePoint(bool combineSurrogatePairs = true);

#endif

    constexpr void ignore(size_t count = 1) {

        count = min(count, m_input.length() - m_index);
        
        m_index += count;
    }

    constexpr void ignoreUntil(char stop)
    {
        while (!isEof() && peek() != stop) {

            ++m_index;
        }

        ignore();
    }

    constexpr void ignoreUntil(char const* stop)
    {
        while (!isEof() && !nextIs(stop)) {

            ++m_index;
        }

        ignore(__builtin_strlen(stop));
    }

    /*
     * Conditions are used to match arbitrary characters. You can use lambdas,
     * ctype functions, or is_any_of() and its derivatives (see below).
     * A few examples:
     *   - `if (lexer.next_is(isdigit))`
     *   - `auto name = lexer.consume_while([](char c) { return isalnum(c) || c == '_'; });`
     *   - `lexer.ignore_until(is_any_of("<^>"));`
     */

    // Test the next character against a Condition
    
    template<typename TPredicate>
    constexpr bool nextIs(TPredicate pred) const {

        return pred(peek());
    }

    // Consume and return characters while `pred` returns true

    template<typename TPredicate>
    StringView consumeWhile(TPredicate pred) {

        size_t start = m_index;
        
        while (!isEof() && pred(peek())) {

            ++m_index;
        }

        size_t length = m_index - start;

        if (length == 0) {

            return { };
        }

        return m_input.substringView(start, length);
    }

    // Consume and return characters until `pred` return true
    template<typename TPredicate>
    StringView consumeUntil(TPredicate pred) {

        size_t start = m_index;
        
        while (!isEof() && !pred(peek())) {

            ++m_index;
        }
        
        size_t length = m_index - start;

        if (length == 0) {

            return { };
        }

        return m_input.substringView(start, length);
    }

    ///

    // Ignore characters while `pred` returns true

    template<typename TPredicate>
    constexpr void ignoreWhile(TPredicate pred) {

        while (!isEof() && pred(peek())) {

            ++m_index;
        }
    }

    // Ignore characters until `pred` return true
    // We don't skip the stop character as it may not be a unique value

    template<typename TPredicate>
    constexpr void ignoreUntil(TPredicate pred) {

        while (!isEof() && !pred(peek())) {

            ++m_index;
        }
    }

protected:
    
    StringView m_input;
    
    size_t m_index { 0 };

private:

#ifndef OS
    
    Result<UInt32, UnicodeEscapeError> decodeCodePoint();
    
    Result<UInt32, UnicodeEscapeError> decodeSingleOrPairedSurrogate(bool combineSurrogatePairs);

#endif

};

///

constexpr auto isAnyOf(StringView values) {

    return [values](auto c) { return values.contains(c); };
}

constexpr auto isNotAnyOf(StringView values) {

    return [values](auto c) { return !values.contains(c); };
}

constexpr auto isPathSeparator = isAnyOf("/\\");

constexpr auto isQuote = isAnyOf("'\"");