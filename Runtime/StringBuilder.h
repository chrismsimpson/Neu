
#pragma once

#include "ByteBuffer.h"
#include "Format.h"
#include "Forward.h"
#include "StringView.h"

#include <stdarg.h>

class StringBuilder {

public:

    using OutputType = String;

    explicit StringBuilder(size_t initialCapacity = inlineCapacity);

    ~StringBuilder() = default;

    ErrorOr<void> tryAppend(StringView);

    ErrorOr<void> tryAppendCodePoint(UInt32);

    ErrorOr<void> tryAppend(char);

    template<typename... Parameters>
    ErrorOr<void> tryAppendff(CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) {

        VariadicFormatParams variadicFormatParams { parameters... };

        return vformat(*this, fmtstr.view(), variadicFormatParams);
    }

    ErrorOr<void> tryAppend(char const*, size_t);

    ErrorOr<void> tryAppendEscapedForJson(StringView);

    void append(StringView);
    
    void append(char);
    
    void appendCodePoint(UInt32);
    
    void append(char const*, size_t);

    void appendAsLowercase(char);

    void appendEscapedForJson(StringView);

    template<typename... Parameters>
    void appendff(CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) {

        VariadicFormatParams variadicFormatParams { parameters... };
        
        MUST(vformat(*this, fmtstr.view(), variadicFormatParams));
    }

#ifndef OS

    [[nodiscard]] String build() const;

    [[nodiscard]] String toString() const;

#endif

    [[nodiscard]] ByteBuffer toByteBuffer() const;

    [[nodiscard]] StringView stringView() const;

    void clear();

    [[nodiscard]] size_t length() const { return m_buffer.size(); }

    [[nodiscard]] bool isEmpty() const { return m_buffer.isEmpty(); }

    void trim(size_t count) { m_buffer.resize(m_buffer.size() - count); }

    template<class SeparatorType, class CollectionType>
    void join(SeparatorType const& separator, CollectionType const& collection, StringView fmtstr = "{}"sv) {
        
        bool first = true;
        
        for (auto& item : collection) {

            if (first) {

                first = false;
            }
            else {

                append(separator);
            }

            appendff(fmtstr, item);
        }
    }

private:

    ErrorOr<void> willAppend(size_t);

    UInt8* data() { return m_buffer.data(); }
    
    UInt8 const* data() const { return m_buffer.data(); }

    static constexpr size_t inlineCapacity = 256;

    Detail::ByteBuffer<inlineCapacity> m_buffer;
};