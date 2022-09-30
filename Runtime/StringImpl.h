
#pragma once

#include "Badge.h"
#include "RefCounted.h"
#include "RefPointer.h"
#include "Span.h"
#include "Types.h"
#include "kmalloc.h"

enum ShouldChomp {

    NoChomp,
    Chomp
};

size_t allocationSizeForStringImpl(size_t length);

class StringImpl : public RefCounted<StringImpl> {

public:

    static NonNullRefPointer<StringImpl> createUninitialized(size_t length, char*& buffer);

    static RefPointer<StringImpl> create(char const* cstring, ShouldChomp = NoChomp);
    
    static RefPointer<StringImpl> create(char const* cstring, size_t length, ShouldChomp = NoChomp);
    
    static RefPointer<StringImpl> create(ReadOnlyBytes, ShouldChomp = NoChomp);
    
    static RefPointer<StringImpl> createLowercased(char const* cstring, size_t length);
    
    static RefPointer<StringImpl> createUppercased(char const* cstring, size_t length);

    NonNullRefPointer<StringImpl> toLowercase() const;

    NonNullRefPointer<StringImpl> toUppercase() const;

    ///

    void operator delete(void* ptr) {

        kfreeSized(ptr, allocationSizeForStringImpl(static_cast<StringImpl*>(ptr)->m_length));
    }


    static StringImpl& theEmptyStringImpl();

    ~StringImpl();

    size_t length() const { return m_length; }

    // Includes NUL-terminator.

    char const* characters() const { return &m_inlineBuffer[0]; }

    ALWAYS_INLINE ReadOnlyBytes bytes() const { return { characters(), length() }; }

    ALWAYS_INLINE StringView view() const { return { characters(), length() }; }

    char const& operator[](size_t i) const {

        VERIFY(i < m_length);

        return characters()[i];
    }

    bool operator==(StringImpl const& other) const {

        if (length() != other.length()) {

            return false;
        }

        return __builtin_memcmp(characters(), other.characters(), length()) == 0;
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

    unsigned caseInsensitiveHash() const;

private:

    enum ConstructTheEmptyStringImplTag {

        ConstructTheEmptyStringImpl
    };

    explicit StringImpl(ConstructTheEmptyStringImplTag) {

        m_inlineBuffer[0] = '\0';
    }

    ///

    enum ConstructWithInlineBufferTag {
        
        ConstructWithInlineBuffer
    };

    StringImpl(ConstructWithInlineBufferTag, size_t length);

    void computeHash() const;

    ///

    size_t m_length { 0 };
    
    mutable unsigned m_hash { 0 };
    
    mutable bool m_hasHash { false };
    
    char m_inlineBuffer[0];
};

inline size_t allocationSizeForStringImpl(size_t length) {

    return sizeof(StringImpl) + (sizeof(char) * length) + sizeof(char);
}

template<>
struct Formatter<StringImpl> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, StringImpl const& value) {

        return Formatter<StringView>::format(builder, { value.characters(), value.length() });
    }
};