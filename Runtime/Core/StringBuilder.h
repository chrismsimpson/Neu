/*
 * Copyright (c) 2018-2021, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Format.h>
#include <Core/Forward.h>
#include <Core/String.h>
#include <Core/StringView.h>
#include <Builtins/Array.h>
#include <stdarg.h>

class StringBuilder {

public:

    using OutputType = String;

    explicit StringBuilder();

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

#ifndef KERNEL

    [[nodiscard]] String build() const;

    [[nodiscard]] String toString() const;

#endif

    [[nodiscard]] StringView stringView() const;

    void clear();

    [[nodiscard]] size_t length() const { return m_buffer.size(); }

    [[nodiscard]] bool isEmpty() const { return m_buffer.isEmpty(); }

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

    UInt8* data() { return m_buffer.unsafeData(); }
    
    UInt8 const* data() const { return const_cast<StringBuilder*>(this)->m_buffer.unsafeData(); }
    
    static constexpr size_t inlineCapacity = 256;

    Array<UInt8> m_buffer;
};

template<typename T>
struct Formatter<Array<T>> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, Array<T> const& value) {

        StringBuilder stringBuilder;

        stringBuilder.append("[");

        for (size_t i = 0; i < value.size(); ++i) {

            if constexpr (IsSame<String, T>) {

                stringBuilder.append("\"");
            }

            stringBuilder.appendff("{}", value[i]);

            if constexpr (IsSame<String, T>) {

                stringBuilder.append("\"");
            }

            if (i != value.size() - 1) {

                stringBuilder.append(",");
            }
        }
        
        stringBuilder.append("]");

        return Formatter<StringView>::format(builder, stringBuilder.toString());
    }
};
