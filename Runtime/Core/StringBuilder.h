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
#include <Builtins/Dictionary.h>
#include <Builtins/Set.h>
#include <stdarg.h>

class StringBuilder {

public:

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

    [[nodiscard]] ErrorOr<String> toString() const;

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
void appendValue(StringBuilder& stringBuilder, T const& value) {

    if constexpr (IsSame<String, T>) {

        stringBuilder.append("\"");
    }

    stringBuilder.appendff("{}", value);

    if constexpr (IsSame<String, T>) {

        stringBuilder.append("\"");
    }
}

template<typename T>
struct Formatter<Array<T>> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, Array<T> const& value) {

        StringBuilder stringBuilder;

        stringBuilder.append("[");

        for (size_t i = 0; i < value.size(); ++i) {

            appendValue(stringBuilder, value[i]);

            if (i != value.size() - 1) {

                stringBuilder.append(", ");
            }
        }
        
        stringBuilder.append("]");

        return Formatter<StringView>::format(builder, TRY(stringBuilder.toString()));
    }
};

template<typename T>
struct Formatter<Set<T>> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, Set<T> const& set) {

        StringBuilder stringBuilder;
        
        stringBuilder.append("{");
        
        auto iter = set.iterator();

        for (size_t i = 0; i < set.size(); ++i) {
            
            appendValue(stringBuilder, iter.next().value());

            if (i != set.size() - 1) {

                stringBuilder.append(", ");
            }
        }
        
        stringBuilder.append("}");

        return Formatter<StringView>::format(builder, TRY(stringBuilder.toString()));
    }
};

template<typename K, typename V>
struct Formatter<Dictionary<K, V>> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, Dictionary<K, V> const& dict) {

        StringBuilder stringBuilder;

        stringBuilder.append("[");

        auto iter = dict.iterator();

        for (size_t i = 0; i < dict.size(); ++i) {

            auto item = iter.next().value();

            appendValue(stringBuilder, item.template get<0>());

            stringBuilder.append(": ");

            appendValue(stringBuilder, item.template get<1>());

            if (i != dict.size() - 1) {

                stringBuilder.append(", ");
            }
        }

        stringBuilder.append("]");
        
        return Formatter<StringView>::format(builder, TRY(stringBuilder.toString()));
    }
};

template<typename... Ts>
struct Formatter<Tuple<Ts...>> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, Tuple<Ts...> const& tuple) {

        StringBuilder stringBuilder;
        
        stringBuilder.append("(");
        
        if constexpr (sizeof...(Ts) > 0) {
            
            tuple.applyAsArgs([&] (auto first, auto... args) {
                
                appendValue(stringBuilder, first);
                
                ((stringBuilder.append(", "), append_value(stringBuilder, args)), ...);
            });
        }

        stringBuilder.append(")");
        
        return Formatter<StringView>::format(builder, TRY(stringBuilder.toString()));
    }
};

