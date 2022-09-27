
#include "ByteBuffer.h"
#include "Checked.h"
#include "PrintfImpl.h"
#include "std.h"
#include "StringBuilder.h"
#include "StringView.h"
#include "UnicodeUtils.h"
#include "Utf32View.h"

#ifndef OS
#    include "String.h"
#    include "Utf16View.h"
#endif

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

void StringBuilder::appendvf(char const* fmt, va_list ap) {

    printfInternal([this](char*&, char ch) {
        
            append(ch);
        },
        nullptr, fmt, ap);
}

ByteBuffer StringBuilder::toByteBuffer() const {

    // FIXME: Handle OOM failure.
    
    return ByteBuffer::copy(data(), length()).releaseValueButFixmeShouldPropagateErrors();
}

#ifndef KERNEL

String StringBuilder::toString() const {

    if (isEmpty()) {

        return String::empty();
    }

    return String((char const*) data(), length());
}

String StringBuilder::build() const {

    return toString();
}

#endif
