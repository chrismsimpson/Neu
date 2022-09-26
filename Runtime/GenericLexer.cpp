
#include "Assertions.h"
#include "CharacterTypes.h"
#include "GenericLexer.h"
#include "StringBuilder.h"


#ifndef OS
#    include "String.h"
// #    include "Utf16View.h"
#endif

///

// Consume a number of characters

StringView GenericLexer::consume(size_t count) {

    if (count == 0) {

        return { };
    }

    size_t start = m_index;
    
    size_t length = min(count, m_input.length() - m_index);
    
    m_index += length;

    return m_input.substringView(start, length);
}

///

// Consume the rest of the input

StringView GenericLexer::consumeAll() {

    if (isEof()) {

        return { };
    }

    auto rest = m_input.substringView(m_index, m_input.length() - m_index);

    m_index = m_input.length();

    return rest;
}

///

// Consume until a new line is found

StringView GenericLexer::consumeLine() {

    size_t start = m_index;
    
    while (!isEof() && peek() != '\r' && peek() != '\n') {

        m_index++;
    }

    size_t length = m_index - start;

    consumeSpecific('\r');
    
    consumeSpecific('\n');

    if (length == 0) {

        return { };
    }

    return m_input.substringView(start, length);
}

///

// Consume and return characters until `stop` is peek'd

StringView GenericLexer::consumeUntil(char stop) {

    size_t start = m_index;

    while (!isEof() && peek() != stop) {

        m_index++;
    }

    size_t length = m_index - start;

    if (length == 0) {

        return { };
    }

    return m_input.substringView(start, length);
}

///

// Consume and return characters until the string `stop` is found

StringView GenericLexer::consumeUntil(char const* stop) {

    size_t start = m_index;
    
    while (!isEof() && !nextIs(stop)) {

        m_index++;
    }
    
    size_t length = m_index - start;

    if (length == 0) {

        return { };
    }
    
    return m_input.substringView(start, length);
}

///

// Consume and return characters until the string `stop` is found

StringView GenericLexer::consumeUntil(StringView stop) {

    size_t start = m_index;
    
    while (!isEof() && !nextIs(stop)) {

        m_index++;
    }
    
    size_t length = m_index - start;

    if (length == 0) {

        return { };
    }
    
    return m_input.substringView(start, length);
}

///


/*
 * Consume a string surrounded by single or double quotes. The returned
 * StringView does not include the quotes. An escape character can be provided
 * to capture the enclosing quotes. Please note that the escape character will
 * still be in the resulting StringView
 */

StringView GenericLexer::consumeQuotedString(char escapeChar) {

    if (!nextIs(isQuote)) {

        return { };
    }

    char quoteChar = consume();
    
    size_t start = m_index;
    
    while (!isEof()) {
        
        if (nextIs(escapeChar)) {

            m_index++;
        }
        else if (nextIs(quoteChar)) {

            break;
        }

        m_index++;
    }
    
    size_t length = m_index - start;

    if (peek() != quoteChar) {
        
        // Restore the index in case the string is unterminated
        
        m_index = start - 1;
        
        return { };
    }

    // Ignore closing quote

    ignore();

    return m_input.substringView(start, length);
}


#ifndef OS

// String GenericLexer::consumeAndUnescapeString(char escapeChar) {

//     auto view = consumeQuotedString(escapeChar);

//     if (view.isNull()) {

//         return {};
//     }

//     StringBuilder builder;

//     for (size_t i = 0; i < view.length(); ++i) {

//         builder.append(consumeEscapedCharacter(escapeChar));
//     }

//     return builder.toString();
// }

// auto GenericLexer::consume_escaped_code_point(bool combine_surrogate_pairs) -> Result<u32, UnicodeEscapeError>
// {
//     if (!consume_specific("\\u"sv))
//         return UnicodeEscapeError::MalformedUnicodeEscape;

//     if (next_is('{'))
//         return decode_code_point();
//     return decode_single_or_paired_surrogate(combine_surrogate_pairs);
// }

// auto GenericLexer::decode_code_point() -> Result<u32, UnicodeEscapeError>
// {
//     bool starts_with_open_bracket = consume_specific('{');
//     VERIFY(starts_with_open_bracket);

//     u32 code_point = 0;

//     while (true) {
//         if (!next_is(is_ascii_hex_digit))
//             return UnicodeEscapeError::MalformedUnicodeEscape;

//         auto new_code_point = (code_point << 4u) | parse_ascii_hex_digit(consume());
//         if (new_code_point < code_point)
//             return UnicodeEscapeError::UnicodeEscapeOverflow;

//         code_point = new_code_point;
//         if (consume_specific('}'))
//             break;
//     }

//     if (is_unicode(code_point))
//         return code_point;
//     return UnicodeEscapeError::UnicodeEscapeOverflow;
// }

// auto GenericLexer::decode_single_or_paired_surrogate(bool combine_surrogate_pairs) -> Result<u32, UnicodeEscapeError>
// {
//     constexpr size_t surrogate_length = 4;

//     auto decode_one_surrogate = [&]() -> Optional<u16> {
//         u16 surrogate = 0;

//         for (size_t i = 0; i < surrogate_length; ++i) {
//             if (!next_is(is_ascii_hex_digit))
//                 return {};

//             surrogate = (surrogate << 4u) | parse_ascii_hex_digit(consume());
//         }

//         return surrogate;
//     };

//     auto high_surrogate = decode_one_surrogate();
//     if (!high_surrogate.has_value())
//         return UnicodeEscapeError::MalformedUnicodeEscape;
//     if (!Utf16View::is_high_surrogate(*high_surrogate))
//         return *high_surrogate;
//     if (!combine_surrogate_pairs || !consume_specific("\\u"sv))
//         return *high_surrogate;

//     auto low_surrogate = decode_one_surrogate();
//     if (!low_surrogate.has_value())
//         return UnicodeEscapeError::MalformedUnicodeEscape;
//     if (Utf16View::is_low_surrogate(*low_surrogate))
//         return Utf16View::decode_surrogate_pair(*high_surrogate, *low_surrogate);

//     retreat(6);
//     return *high_surrogate;
// }

#endif