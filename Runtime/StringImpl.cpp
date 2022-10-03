/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#include "CharacterTypes.h"
#include "HashTable.h"
#include "Memory.h"
#include "std.h"
#include "StringHash.h"
#include "StringImpl.h"
#include "kmalloc.h"

static StringImpl* s_theEmptyStringImpl = nullptr;

StringImpl& StringImpl::theEmptyStringImpl() {

    if (!s_theEmptyStringImpl) {

        void* slot = kmalloc(sizeof(StringImpl) + sizeof(char));
        
        s_theEmptyStringImpl = new (slot) StringImpl(ConstructTheEmptyStringImpl);
    }

    return *s_theEmptyStringImpl;
}

StringImpl::StringImpl(ConstructWithInlineBufferTag, size_t length)
    : m_length(length) { }


StringImpl::~StringImpl() { }

NonNullRefPointer<StringImpl> StringImpl::createUninitialized(size_t length, char*& buffer) {

    VERIFY(length);
    
    void* slot = kmalloc(allocationSizeForStringImpl(length));
    
    VERIFY(slot);
    
    auto newStringImpl = adoptRef(*new (slot) StringImpl(ConstructWithInlineBuffer, length));
    
    buffer = const_cast<char*>(newStringImpl->characters());
    
    buffer[length] = '\0';
    
    return newStringImpl;
}

RefPointer<StringImpl> StringImpl::create(char const* cstring, size_t length, ShouldChomp shouldChomp) {

    if (!cstring) {

        return nullptr;
    }

    if (shouldChomp) {

        
        while (length) {
        
            char lastCh = cstring[length - 1];
        
            if (!lastCh || lastCh == '\n' || lastCh == '\r') {

                --length;
            }
            else {

                break;
            }
        }
    }

    if (!length) {

        return theEmptyStringImpl();
    }

    char* buffer;
    
    auto newStringImpl = createUninitialized(length, buffer);
    
    memcpy(buffer, cstring, length * sizeof(char));

    return newStringImpl;
}

RefPointer<StringImpl> StringImpl::create(char const* cstring, ShouldChomp shouldChomp) {

    if (!cstring) {

        return nullptr;
    }

    if (!*cstring) {
        
        return theEmptyStringImpl();
    }

    return create(cstring, strlen(cstring), shouldChomp);
}

RefPointer<StringImpl> StringImpl::create(ReadOnlyBytes bytes, ShouldChomp shouldChomp) {

    return StringImpl::create(reinterpret_cast<char const*>(bytes.data()), bytes.size(), shouldChomp);
}

RefPointer<StringImpl> StringImpl::createLowercased(char const* cstring, size_t length) {

    if (!cstring) {

        return nullptr;
    }

    if (!length) {

        return theEmptyStringImpl();
    }

    char* buffer;
    
    auto impl = createUninitialized(length, buffer);
    
    for (size_t i = 0; i < length; ++i) {

        buffer[i] = (char) toAsciiLowercase(cstring[i]);
    }
    
    return impl;
}

RefPointer<StringImpl> StringImpl::createUppercased(char const* cstring, size_t length) {

    if (!cstring) {

        return nullptr;
    }

    if (!length) {

        return theEmptyStringImpl();
    }

    char* buffer;
    
    auto impl = createUninitialized(length, buffer);
    
    for (size_t i = 0; i < length; ++i) {

        buffer[i] = (char) toAsciiUppercase(cstring[i]);
    }

    return impl;
}

NonNullRefPointer<StringImpl> StringImpl::toLowercase() const {

    for (size_t i = 0; i < m_length; ++i) { 

        if (isAsciiUpperAlpha(characters()[i])) {

            return createLowercased(characters(), m_length).releaseNonNull();
        }
    }

    return const_cast<StringImpl&>(*this);
}

NonNullRefPointer<StringImpl> StringImpl::toUppercase() const {

    for (size_t i = 0; i < m_length; ++i) {

        if (isAsciiLowerAlpha(characters()[i])) {

            return createUppercased(characters(), m_length).releaseNonNull();
        }
    }

    return const_cast<StringImpl&>(*this);
}

unsigned StringImpl::caseInsensitiveHash() const {

    return caseInsensitiveStringHash(characters(), length());
}

void StringImpl::computeHash() const {
    
    if (!length()) {

        m_hash = 0;
    }
    else {

        m_hash = stringHash(characters(), m_length);
    }

    m_hasHash = true;
}
