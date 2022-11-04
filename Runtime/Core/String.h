/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/CheckedFormatString.h>
#include <Core/Format.h>
#include <Core/Forward.h>
#include <Core/NonNullRefPointer.h>
#include <Core/RefCounted.h>
#include <Core/Span.h>
#include <Core/StringView.h>
#include <Core/Traits.h>
#include <Core/Types.h>
#include <Core/kmalloc.h>
#include <Builtins/Array.h>

class StringStorage : public RefCounted<StringStorage> {

public:

    static ErrorOr<NonNullRefPointer<StringStorage>> createUninitialized(size_t length, char*& buffer);

    static ErrorOr<NonNullRefPointer<StringStorage>> create(char const* c_string, size_t length);

    void operator delete(void* ptr);

    static StringStorage& theEmptyString();

    ~StringStorage();

    size_t length() const { return m_length; }

    // NOTE: Always includes null terminator.

    char const* cString() const { return &m_inlineBuffer[0]; }

    StringView view() const { return { cString(), length() }; }

    char const& operator[](size_t i) const {

        VERIFY(i < m_length);

        return cString()[i];
    }

    bool operator==(StringStorage const& other) const {

        if (length() != other.length()) {

            return false;
        }

        return __builtin_memcmp(cString(), other.cString(), length()) == 0;
    }

    unsigned hash() const {

        if (!m_hasHash) {

            computeHash();
        }

        return m_hash;
    }

    unsigned existingHash() const {

        return m_hash;
    }

private:

    enum ConstructTheEmptyStringStorageTag {

        ConstructTheEmptyStringStorage
    };

    explicit StringStorage(ConstructTheEmptyStringStorageTag) {

        m_inlineBuffer[0] = '\0';
    }

    enum ConstructWithInlineBufferTag {

        ConstructWithInlineBuffer
    };

    StringStorage(ConstructWithInlineBufferTag, size_t length);

    void computeHash() const;

    size_t m_length { 0 };
    
    mutable unsigned m_hash { 0 };
    
    mutable bool m_hasHash { false };
    
    char m_inlineBuffer[0];
};

inline size_t allocationSizeForStringStorage(size_t length) {

    return sizeof(StringStorage) + (sizeof(char) * length) + sizeof(char);
}

class String {

public:

    String(String const&) = default;
    
    String(String&&) = default;
    
    String& operator=(String&&) = default;
    
    String& operator=(String const&) = default;

    // FIXME: Remove this constructor!
    
    explicit String(char const* c_string)
        : m_storage(MUST(StringStorage::create(c_string, strlen(c_string)))) { }

    ~String() = default;

    [[nodiscard]] static String empty() { return String { StringStorage::theEmptyString() }; }

    static ErrorOr<String> fromUtf8(StringView);

    static ErrorOr<String> copy(StringView);

    [[nodiscard]] static ErrorOr<String> vformatted(StringView fmtstr, TypeErasedFormatParams&);

    template<typename... Parameters>
    [[nodiscard]] static ErrorOr<String> formatted(CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) {

        VariadicFormatParams variadicFormatParameters { parameters... };

        return vformatted(fmtstr.view(), variadicFormatParameters);
    }

    StringView view() const { return { cString(), length() }; }

    static ErrorOr<String> repeated(char, size_t count);

    template<typename T = int>
    Optional<T> toInt(TrimWhitespace = TrimWhitespace::Yes) const;
    
    template<typename T = unsigned>
    Optional<T> toUInt(TrimWhitespace = TrimWhitespace::Yes) const;

    [[nodiscard]] bool isWhitespace() const;

    [[nodiscard]] bool equalsIgnoringCase(StringView) const;

    [[nodiscard]] bool contains(StringView, CaseSensitivity = CaseSensitivity::CaseSensitive) const;
    
    [[nodiscard]] bool contains(char, CaseSensitivity = CaseSensitivity::CaseSensitive) const;

    ErrorOr<Array<String>> split(char separator, bool keepEmpty = false) const;
    
    ErrorOr<Array<String>> splitLimit(char separator, size_t limit, bool keepEmpty) const;

    [[nodiscard]] ErrorOr<String> substring(size_t start, size_t length) const;

    [[nodiscard]] bool isEmpty() const { return length() == 0; }
    
    [[nodiscard]] size_t length() const { return m_storage->length(); }

    // Guaranteed to include null terminator.

    [[nodiscard]] char const* cString() const { return m_storage->cString(); }

    [[nodiscard]] ALWAYS_INLINE char const& operator[](size_t i) const {

        return (*m_storage)[i];
    }

    [[nodiscard]] bool startsWith(StringView, CaseSensitivity = CaseSensitivity::CaseSensitive) const;
    
    [[nodiscard]] bool endsWith(StringView, CaseSensitivity = CaseSensitivity::CaseSensitive) const;
    
    [[nodiscard]] bool startsWith(char) const;
    
    [[nodiscard]] bool endsWith(char) const;

    bool operator==(String const&) const;
    bool operator!=(String const& other) const { return !(*this == other); }

    bool operator==(StringView) const;
    bool operator!=(StringView other) const { return !(*this == other); }

    bool operator<(String const&) const;
    bool operator<(char const*) const;
    bool operator>=(String const& other) const { return !(*this < other); }
    bool operator>=(char const* other) const { return !(*this < other); }

    bool operator>(String const&) const;
    bool operator>(char const*) const;
    bool operator<=(String const& other) const { return !(*this > other); }
    bool operator<=(char const* other) const { return !(*this > other); }

    bool operator==(char const* cstring) const;
    bool operator!=(char const* cstring) const { return !(*this == cstring); }

    [[nodiscard]] StringStorage& storage() { return *m_storage; }
    [[nodiscard]] StringStorage const& storage() const { return *m_storage; }

    [[nodiscard]] UInt32 hash() const {

        return m_storage->hash();
    }

    template<typename T>
    [[nodiscard]] static ErrorOr<String> number(T value) requires IsArithmetic<T> {

        return formatted("{}", value);
    }

private:

    String(NonNullRefPointer<StringStorage> storage)
        : m_storage(move(storage)) { }

    NonNullRefPointer<StringStorage> m_storage;
};

template<>
struct Formatter<StringStorage> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, StringStorage const& value) {

        return Formatter<StringView>::format(builder, { value.cString(), value.length() });
    }
};

template<>
struct Traits<String> : public GenericTraits<String> {

    static unsigned hash(String const& s) { return s.storage().hash(); }
};

template<typename T>
struct Formatter<NonNullRefPointer<T>> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, NonNullRefPointer<T> const& value) {

        auto str = TRY(String::formatted("{}", *value));
        
        return Formatter<StringView>::format(builder, str);
    }
};

template<typename T>
struct Formatter<Optional<T>> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, Optional<T> const& value) {

        if (!value.hasValue()) {

            return Formatter<StringView>::format(builder, "none");
        }

        auto str = TRY(String::formatted("{}", *value));
        
        return Formatter<StringView>::format(builder, str);
    }
};
