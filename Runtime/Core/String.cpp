/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#include <Core/Checked.h>
#include <Core/Format.h>
#include <Core/Memory.h>
#include <Core/std.h>
#include <Core/String.h>
#include <Core/StringUtils.h>

ErrorOr<String> String::copy(StringView view) {

    auto storage = TRY(StringStorage::create(view.charactersWithoutNullTermination(), view.length()));

    return String { storage };
}

ErrorOr<String> String::vformatted(StringView fmtstr, TypeErasedFormatParams& params) {

    StringBuilder builder;

    TRY(vformat(builder, fmtstr, params));

    return builder.toString();
}

bool String::operator==(String const& other) const {

    return m_storage == other.storage() || view() == other.view();
}

bool String::operator==(StringView other) const {

    return view() == other;
}

bool String::operator<(String const& other) const {

    return view() < other.view();
}

bool String::operator>(String const& other) const {

    return view() > other.view();
}

ErrorOr<String> String::substring(size_t start, size_t length) const {

    if (!length) {

        return String::empty();
    }

    VERIFY(!Checked<size_t>::additionWouldOverflow(start, length));
    
    VERIFY(start + length <= m_storage->length());
    
    return String::copy(StringView { cString() + start, length });
}

ErrorOr<Array<String>> String::split(char separator, bool keepEmpty) const {

    return splitLimit(separator, 0, keepEmpty);
}

ErrorOr<Array<String>> String::splitLimit(char separator, size_t limit, bool keepEmpty) const {

    auto v = Array<String>();

    if (isEmpty()) {

        return v;
    }
    
    size_t substart = 0;
    
    for (size_t i = 0; i < length() && (v.size() + 1) != limit; ++i) {
        
        char ch = cString()[i];
        
        if (ch == separator) {
            
            size_t sublen = i - substart;
            
            if (sublen != 0 || keepEmpty) {

                TRY(v.push(TRY(substring(substart, sublen))));
            }

            substart = i + 1;
        }
    }
    
    size_t taillen = length() - substart;

    if (taillen != 0 || keepEmpty) {

        TRY(v.push(TRY(substring(substart, taillen))));
    }

    return v;
}

template<typename T>
Optional<T> String::toInt(TrimWhitespace trimWhitespace) const {

    return StringUtils::convertToInt<T>(view(), trimWhitespace);
}

template Optional<Int8> String::toInt(TrimWhitespace) const;

template Optional<Int16> String::toInt(TrimWhitespace) const;

template Optional<Int32> String::toInt(TrimWhitespace) const;

template Optional<Int64> String::toInt(TrimWhitespace) const;

template<typename T>
Optional<T> String::toUInt(TrimWhitespace trimWhitespace) const {

    return StringUtils::convertToUInt<T>(view(), trimWhitespace);
}

template Optional<UInt8> String::toUInt(TrimWhitespace) const;

template Optional<UInt16> String::toUInt(TrimWhitespace) const;

template Optional<UInt32> String::toUInt(TrimWhitespace) const;

template Optional<unsigned long> String::toUInt(TrimWhitespace) const;

template Optional<unsigned long long> String::toUInt(TrimWhitespace) const;

bool String::startsWith(StringView str, CaseSensitivity caseSensitivity) const {

    return StringUtils::startsWith(view(), str, caseSensitivity);
}

bool String::startsWith(char ch) const {

    if (isEmpty()) {

        return false;
    }

    return cString()[0] == ch;
}

bool String::endsWith(StringView str, CaseSensitivity caseSensitivity) const {

    return StringUtils::endsWith(view(), str, caseSensitivity);
}

bool String::endsWith(char ch) const {
    
    if (isEmpty()) {

        return false;
    }

    return cString()[length() - 1] == ch;
}

ErrorOr<String> String::repeated(char ch, size_t count) {

    if (!count) {

        return empty();
    }
    
    char* buffer;
    
    auto storage = TRY(StringStorage::createUninitialized(count, buffer));
    
    memset(buffer, ch, count);
    
    return String { *storage };
}

bool String::contains(StringView needle, CaseSensitivity caseSensitivity) const {

    return StringUtils::contains(view(), needle, caseSensitivity);
}

bool String::contains(char needle, CaseSensitivity caseSensitivity) const {

    return StringUtils::contains(view(), StringView(&needle, 1), caseSensitivity);
}

bool String::equalsIgnoringCase(StringView other) const {

    return StringUtils::equalsIgnoringCase(view(), other);
}

bool operator<(char const* characters, String const& string) {

    return string.view() > characters;
}

bool operator>=(char const* characters, String const& string) {

    return string.view() <= characters;
}

bool operator>(char const* characters, String const& string) {

    return string.view() < characters;
}

bool operator<=(char const* characters, String const& string) {

    return string.view() >= characters;
}

bool String::operator==(char const* c_string) const {

    return view() == c_string;
}

void StringStorage::operator delete(void* ptr) {

    free(ptr);
}

static StringStorage* s_theEmptyStringStorage = nullptr;

StringStorage& StringStorage::theEmptyString() {

    if (!s_theEmptyStringStorage) { 

        void* slot = malloc(sizeof(StringStorage) + sizeof(char));

        s_theEmptyStringStorage = new (slot) StringStorage(ConstructTheEmptyStringStorage);
    }

    return *s_theEmptyStringStorage;
}

StringStorage::StringStorage(ConstructWithInlineBufferTag, size_t length)
    : m_length(length) { }

StringStorage::~StringStorage() { }

constexpr size_t constAllocationSizeForStringStorage(size_t length) {

    return sizeof(StringStorage) + (sizeof(char) * length) + sizeof(char);
}

ErrorOr<NonNullRefPointer<StringStorage>> StringStorage::createUninitialized(size_t length, char*& buffer) {

    VERIFY(length);

    void* slot = malloc(constAllocationSizeForStringStorage(length));

    if (!slot) {

        return Error::fromError(ENOMEM);
    }

    auto newStringStorage = adoptRef(*new (slot) StringStorage(ConstructWithInlineBuffer, length));

    buffer = const_cast<char*>(newStringStorage->cString());

    buffer[length] = '\0';

    return newStringStorage;
}

ErrorOr<NonNullRefPointer<StringStorage>> StringStorage::create(char const* c_string, size_t length) {

    if (!length) {

        return theEmptyString();
    }

    VERIFY(c_string);

    char* buffer;
    
    auto newStringStorage = TRY(createUninitialized(length, buffer));
    
    memcpy(buffer, c_string, length * sizeof(char));

    return newStringStorage;
}

void StringStorage::computeHash() const {

    if (!length()) {

        m_hash = 0;
    }
    else {

        m_hash = stringHash(cString(), m_length);
    }

    m_hasHash = true;
}