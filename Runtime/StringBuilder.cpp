/*
 * Copyright (c) 2018-2021, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#include "ByteBuffer.h"
#include "Checked.h"
#include "std.h"
#include "StringBuilder.h"
#include "StringView.h"
#include "UnicodeUtils.h"

inline ErrorOr<void> StringBuilder::willAppend(size_t size) {

    Checked<size_t> neededCapacity = m_buffer.size();

    neededCapacity += size;
    
    VERIFY(!neededCapacity.hasOverflow());
    
    // Prefer to completely use the existing capacity first
    
    if (neededCapacity <= m_buffer.capacity()) {

        return { };
    }

    Checked<size_t> expandedCapacity = neededCapacity;
    
    expandedCapacity *= 2;
    
    VERIFY(!expandedCapacity.hasOverflow());
    
    TRY(m_buffer.tryEnsureCapacity(expandedCapacity.value()));
    
    return { };
}

StringBuilder::StringBuilder(size_t initialCapacity) {

    m_buffer.ensureCapacity(initialCapacity);
}

ErrorOr<void> StringBuilder::tryAppend(StringView string) {
    
    if (string.isEmpty()) {

        return { };
    }

    TRY(willAppend(string.length()));
    
    TRY(m_buffer.tryAppend(string.charactersWithoutNullTermination(), string.length()));
    
    return { };
}

ErrorOr<void> StringBuilder::tryAppend(char ch) {

    TRY(willAppend(1));
    
    TRY(m_buffer.tryAppend(ch));
    
    return { };
}

void StringBuilder::append(StringView string) {

    MUST(tryAppend(string));
}

ErrorOr<void> StringBuilder::tryAppend(char const* characters, size_t length) {

    return tryAppend(StringView { characters, length });
}

void StringBuilder::append(char const* characters, size_t length) {

    MUST(tryAppend(characters, length));
}

void StringBuilder::append(char ch) {

    MUST(tryAppend(ch));
}

ByteBuffer StringBuilder::toByteBuffer() const {

    // FIXME: Handle OOM failure.
    
    return ByteBuffer::copy(data(), length()).releaseValueButFixmeShouldPropagateErrors();
}

String StringBuilder::toString() const {

    if (isEmpty()) {

        return String::empty();
    }

    return String((char const*) data(), length());
}

String StringBuilder::build() const {

    return toString();
}

StringView StringBuilder::stringView() const {

    return StringView { data(), m_buffer.size() };
}

void StringBuilder::clear() {

    m_buffer.clear();
}

ErrorOr<void> StringBuilder::tryAppendCodePoint(UInt32 codePoint) {

    auto nwritten = UnicodeUtils::codePointToUtf8(codePoint, [this](char c) { append(c); });
    
    if (nwritten < 0) {
        
        TRY(tryAppend(0xef));
        
        TRY(tryAppend(0xbf));
        
        TRY(tryAppend(0xbd));
    }

    return { };
}

void StringBuilder::appendCodePoint(UInt32 codePoint) {

    MUST(tryAppendCodePoint(codePoint));
}

void StringBuilder::appendAsLowercase(char ch) {

    if (ch >= 'A' && ch <= 'Z') {

        append(ch + 0x20);
    }
    else {

        append(ch);
    }
}

void StringBuilder::appendEscapedForJson(StringView string) {

    MUST(tryAppendEscapedForJson(string));
}


ErrorOr<void> StringBuilder::tryAppendEscapedForJson(StringView string) {

    for (auto ch : string) {

        switch (ch) {

        case '\b':
            
            TRY(tryAppend("\\b"));
            
            break;

        case '\n':
            
            TRY(tryAppend("\\n"));
            
            break;

        case '\t':
            
            TRY(tryAppend("\\t"));
            
            break;
        
        case '\"':
            
            TRY(tryAppend("\\\""));
            
            break;
        
        case '\\':
            
            TRY(tryAppend("\\\\"));
            
            break;
        
        default:
        
            if (ch >= 0 && ch <= 0x1f) {

                TRY(tryAppendff("\\u{:04x}", ch));
            }
            else {

                TRY(tryAppend(ch));
            }
        }
    }

    return { };
}