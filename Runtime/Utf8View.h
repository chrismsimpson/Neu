/*
 * Copyright (c) 2019-2020, Sergey Bugaev <bugaevc@serenityos.org>
 * Copyright (c) 2021, Max Wipfli <mail@maxwipfli.ch>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include "String.h"
#include "StringView.h"
#include "Types.h"

class Utf8View;

class Utf8CodePointIterator {
    
    friend class Utf8View;

public:

    Utf8CodePointIterator() = default;

    ~Utf8CodePointIterator() = default;

    bool operator==(Utf8CodePointIterator const&) const = default;
    
    bool operator!=(Utf8CodePointIterator const&) const = default;
    
    Utf8CodePointIterator& operator++();
    
    UInt32 operator*() const;

    // NOTE: This returns { } if the peek is at or past EOF.
    
    Optional<UInt32> peek(size_t offset = 0) const;

    ssize_t operator-(Utf8CodePointIterator const& other) const {

        return m_ptr - other.m_ptr;
    }

    // Note : These methods return the information about the underlying UTF-8 bytes.
    // If the UTF-8 string encoding is not valid at the iterator's position, then the underlying bytes might be different from the
    // decoded character's re-encoded bytes (which will be an `0xFFFD REPLACEMENT CHARACTER` with an UTF-8 length of three bytes).
    // If your code relies on the decoded character being equivalent to the re-encoded character, use the `UTF8View::validate()`
    // method on the view prior to using its iterator.
    
    size_t underlyingCodePointLengthInBytes() const;
    
    ReadOnlyBytes underlyingCodePointBytes() const;
    
    bool done() const { return m_length == 0; }


private:

    Utf8CodePointIterator(UInt8 const* ptr, size_t length)
        : m_ptr(ptr), 
          m_length(length) { }

    UInt8 const* m_ptr { nullptr };

    size_t m_length { 0 };
};

///

class Utf8View {

public:

    using Iterator = Utf8CodePointIterator;

    Utf8View() = default;

    explicit Utf8View(String& string)
        : m_string(string.view()) { }

    explicit constexpr Utf8View(StringView string)
        : m_string(string) { }

    ~Utf8View() = default;

    explicit Utf8View(String&&) = delete;

    StringView asString() const { return m_string; }

    Utf8CodePointIterator begin() const { return { beginPointer(), m_string.length() }; }
    
    Utf8CodePointIterator end() const { return { endPointer(), 0 }; }
    
    Utf8CodePointIterator iteratorAtByteOffset(size_t) const;

    unsigned char const* bytes() const { return beginPointer(); }
    
    size_t byteLength() const { return m_string.length(); }
    
    size_t byteOffsetOf(Utf8CodePointIterator const&) const;
    
    size_t byteOffsetOf(size_t codePointOffset) const;

    Utf8View substringView(size_t byteOffset, size_t byteLength) const { return Utf8View { m_string.substringView(byteOffset, byteLength) }; }
    
    Utf8View substringView(size_t byteOffset) const { return substringView(byteOffset, byteLength() - byteOffset); }
    
    Utf8View unicodeSubstringView(size_t codePointOffset, size_t codePointLength) const;
    
    Utf8View unicodeSubstringView(size_t codePointOffset) const { return unicodeSubstringView(codePointOffset, length() - codePointOffset); }

    bool isEmpty() const { return m_string.isEmpty(); }
    
    bool isNull() const { return m_string.isNull(); }
    
    bool startsWith(Utf8View const&) const;
    
    bool contains(UInt32) const;

    Utf8View trim(Utf8View const& characters, TrimMode mode = TrimMode::Both) const;

    size_t iteratorOffset(Utf8CodePointIterator const& it) const {

        return byteOffsetOf(it);
    }

    bool validate(size_t& validBytes) const;

    bool validate() const {

        size_t validBytes;
        
        return validate(validBytes);
    }

    ///

    size_t length() const {

        if (!m_haveLength) {
            
            m_length = calculateLength();
            
            m_haveLength = true;
        }

        return m_length;
    }

private:
    
    UInt8 const* beginPointer() const { return (UInt8 const*) m_string.charactersWithoutNullTermination(); }

    UInt8 const* endPointer() const { return beginPointer() + m_string.length(); }
    
    size_t calculateLength() const;

    StringView m_string;
    
    mutable size_t m_length { 0 };
    
    mutable bool m_haveLength { false };
};