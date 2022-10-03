/*
 * Copyright (c) 2020, Benoit Lormeau <blormeau@outlook.com>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#include "Assertions.h"
#include "CharacterTypes.h"
#include "GenericLexer.h"
#include "StringBuilder.h"

#ifndef OS
#    include "String.h"
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
