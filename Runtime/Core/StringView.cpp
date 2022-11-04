/*
 * Copyright (c) 2018-2021, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#include <Core/AnyOf.h>
#include <Core/Find.h>
#include <Core/Function.h>
#include <Core/StringView.h>
#include <Core/Vector.h>

#ifndef KERNEL
#    include <Core/String.h>
#endif

StringView::StringView(String const& string)
    : m_characters(string.cString()), 
      m_length(string.length()) { }

bool StringView::startsWith(char ch) const {

    if (isEmpty()) {

        return false;
    }

    return ch == charactersWithoutNullTermination()[0];
}

bool StringView::startsWith(StringView str, CaseSensitivity caseSensitivity) const {

    return StringUtils::startsWith(*this, str, caseSensitivity);
}

bool StringView::endsWith(char ch) const {

    if (isEmpty()) {

        return false;
    }

    return ch == charactersWithoutNullTermination()[length() - 1];
}

bool StringView::endsWith(StringView str, CaseSensitivity caseSensitivity) const {

    return StringUtils::endsWith(*this, str, caseSensitivity);
}

bool StringView::contains(char needle) const {

    for (char current : *this) {

        if (current == needle) {

            return true;
        }
    }

    return false;
}

bool StringView::contains(StringView needle, CaseSensitivity caseSensitivity) const {

    return StringUtils::contains(*this, needle, caseSensitivity);
}

bool StringView::equalsIgnoringCase(StringView other) const {

    return StringUtils::equalsIgnoringCase(*this, other);
}

template<typename T>
Optional<T> StringView::toInt() const {

    return StringUtils::convertToInt<T>(*this);
}

template Optional<Int8> StringView::toInt() const;

template Optional<Int16> StringView::toInt() const;

template Optional<Int32> StringView::toInt() const;

template Optional<long> StringView::toInt() const;

template Optional<long long> StringView::toInt() const;

template<typename T>
Optional<T> StringView::toUInt() const {

    return StringUtils::convertToUInt<T>(*this);
}

template Optional<UInt8> StringView::toUInt() const;

template Optional<UInt16> StringView::toUInt() const;

template Optional<UInt32> StringView::toUInt() const;

template Optional<unsigned long> StringView::toUInt() const;

template Optional<unsigned long long> StringView::toUInt() const;

template Optional<long> StringView::toUInt() const;

template Optional<long long> StringView::toUInt() const;

bool StringView::operator==(String const& string) const {

    return *this == string.view();
}

ErrorOr<String> StringView::toString() const { return String::copy(*this); }

ErrorOr<Array<size_t>> StringView::findAll(StringView needle) const {

    return StringUtils::findAll(*this, needle);
}
