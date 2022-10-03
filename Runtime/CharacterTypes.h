/*
 * Copyright (c) 2021, Max Wipfli <mail@maxwipfli.ch>
 * Copyright (c) 2022, the SerenityOS developers.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include "Array.h"
#include "Types.h"

// NOTE: For a quick reference for most of this, see https://www.cplusplus.com/reference/cctype/ and https://infra.spec.whatwg.org/#code-points.
// NOTE: To avoid ambiguity when including this header, all methods contains names should contain "ascii" or "unicode".

constexpr bool isAscii(UInt32 codePoint) {

    return codePoint < 0x80;
}

constexpr bool isAsciiDigit(UInt32 codePoint) {

    return codePoint >= '0' && codePoint <= '9';
}

constexpr bool isAsciiUpperAlpha(UInt32 codePoint) {

    return (codePoint >= 'A' && codePoint <= 'Z');
}

constexpr bool isAsciiLowerAlpha(UInt32 codePoint) {

    return (codePoint >= 'a' && codePoint <= 'z');
}

constexpr bool isAsciiAlpha(UInt32 codePoint) {

    return isAsciiLowerAlpha(codePoint) || isAsciiUpperAlpha(codePoint);
}

constexpr bool isAsciiAlphanumeric(UInt32 codePoint) {

    return isAsciiAlpha(codePoint) || isAsciiDigit(codePoint);
}

constexpr bool isAsciiBinaryDigit(UInt32 codePoint) {

    return codePoint == '0' || codePoint == '1';
}

constexpr bool isAsciiOctalDigit(UInt32 codePoint) {

    return codePoint >= '0' && codePoint <= '7';
}

constexpr bool isAsciiHexDigit(UInt32 codePoint) {

    return isAsciiDigit(codePoint) || (codePoint >= 'A' && codePoint <= 'F') || (codePoint >= 'a' && codePoint <= 'f');
}

constexpr bool isAsciiBlank(UInt32 codePoint) {

    return codePoint == '\t' || codePoint == ' ';
}

constexpr bool isAsciiSpace(UInt32 codePoint) {

    return codePoint == ' ' || codePoint == '\t' || codePoint == '\n' || codePoint == '\v' || codePoint == '\f' || codePoint == '\r';
}

constexpr bool isAsciiPunctuation(UInt32 codePoint) {

    return (codePoint >= 0x21 && codePoint <= 0x2F) || (codePoint >= 0x3A && codePoint <= 0x40) || (codePoint >= 0x5B && codePoint <= 0x60) || (codePoint >= 0x7B && codePoint <= 0x7E);
}

constexpr bool isAsciiGraphical(UInt32 codePoint) {

    return codePoint >= 0x21 && codePoint <= 0x7E;
}

constexpr bool isAsciiPrintable(UInt32 codePoint) {

    return codePoint >= 0x20 && codePoint <= 0x7E;
}

constexpr bool isAsciiC0Control(UInt32 codePoint) {

    return codePoint < 0x20;
}

constexpr bool isAsciiControl(UInt32 codePoint) {

    return isAsciiC0Control(codePoint) || codePoint == 0x7F;
}

constexpr bool isUnicode(UInt32 codePoint) {

    return codePoint <= 0x10FFFF;
}

constexpr bool isUnicodeControl(UInt32 codePoint) {

    return isAsciiC0Control(codePoint) || (codePoint >= 0x7E && codePoint <= 0x9F);
}

constexpr bool isUnicodeSurrogate(UInt32 codePoint) {

    return codePoint >= 0xD800 && codePoint <= 0xDFFF;
}

constexpr bool isUnicodeScalarValue(UInt32 codePoint) {

    return isUnicode(codePoint) && !isUnicodeSurrogate(codePoint);
}

constexpr bool isUnicodeNonCharacter(UInt32 codePoint) {

    return isUnicode(codePoint) && ((codePoint >= 0xFDD0 && codePoint <= 0xFDEF) || ((codePoint & 0xFFFE) == 0xFFFE) || ((codePoint & 0xFFFF) == 0xFFFF));
}

constexpr UInt32 toAsciiLowercase(UInt32 codePoint) {

    if (isAsciiUpperAlpha(codePoint)) {

        return codePoint + 0x20;
    }
    return codePoint;
}

constexpr UInt32 toAsciiUppercase(UInt32 codePoint) {

    if (isAsciiLowerAlpha(codePoint)) {

        return codePoint - 0x20;
    }

    return codePoint;
}

constexpr UInt32 parseAsciiDigit(UInt32 codePoint) {

    if (isAsciiDigit(codePoint)) {

        return codePoint - '0';
    }

    VERIFY_NOT_REACHED();
}

constexpr UInt32 parseAsciiHexDigit(UInt32 codePoint) {

    if (isAsciiDigit(codePoint)) {
        
        return parseAsciiDigit(codePoint);
    }

    if (codePoint >= 'A' && codePoint <= 'F') {

        return codePoint - 'A' + 10;
    }

    if (codePoint >= 'a' && codePoint <= 'f') {

        return codePoint - 'a' + 10;
    }

    VERIFY_NOT_REACHED();
}

constexpr UInt32 parseAsciiBase36Digit(UInt32 codePoint) {

    if (isAsciiDigit(codePoint)) {

        return parseAsciiDigit(codePoint);
    }
    
    if (codePoint >= 'A' && codePoint <= 'Z') {

        return codePoint - 'A' + 10;
    }

    if (codePoint >= 'a' && codePoint <= 'z') {

        return codePoint - 'a' + 10;
    }

    VERIFY_NOT_REACHED();
}

constexpr UInt32 toAsciiBase36Digit(UInt32 digit) {

    constexpr Array<char, 36> base36Map = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
    
    VERIFY(digit < base36Map.size());
    
    return base36Map[digit];
}