
#include "CharacterTypes.h"
#include "StringBuilder.h"
#include "StringView.h"
#include "Utf16View.h"
#include "Utf32View.h"
#include "Utf8View.h"

static constexpr UInt16 highSurrogateMin = 0xd800;

static constexpr UInt16 highSurrogateMax = 0xdbff;

static constexpr UInt16 lowSurrogateMin = 0xdc00;

static constexpr UInt16 lowSurrogateMax = 0xdfff;

static constexpr UInt32 replacementCodePoint = 0xfffd;

static constexpr UInt32 firstSupplementaryPlaneCodePoint = 0x10000;

template<typename UtfViewType>
static Vector<UInt16, 1> toUtf16Impl(UtfViewType const& view) requires(IsSame<UtfViewType, Utf8View> || IsSame<UtfViewType, Utf32View>) {

    Vector<UInt16, 1> utf16Data;
    
    utf16Data.ensureCapacity(view.length());

    for (auto codePoint : view) {

        codePointToUtf16(utf16Data, codePoint);
    }

    return utf16Data;
}

Vector<UInt16, 1> utf8ToUtf16(StringView utf8View) {

    return toUtf16Impl(Utf8View { utf8View });
}

Vector<UInt16, 1> utf8ToUtf16(Utf8View const& utf8View) {

    return toUtf16Impl(utf8View);
}

Vector<UInt16, 1> utf32ToUtf16(Utf32View const& utf32View) {

    return toUtf16Impl(utf32View);
}

void codePointToUtf16(Vector<UInt16, 1>& string, UInt32 codePoint) {

    VERIFY(isUnicode(codePoint));

    if (codePoint < firstSupplementaryPlaneCodePoint) {
        
        string.append(static_cast<UInt16>(codePoint));
    } 
    else {
        
        codePoint -= firstSupplementaryPlaneCodePoint;
        
        string.append(static_cast<UInt16>(highSurrogateMin | (codePoint >> 10)));
        
        string.append(static_cast<UInt16>(lowSurrogateMin | (codePoint & 0x3ff)));
    }
}

bool Utf16View::isHighSurrogate(UInt16 codeUnit) {

    return (codeUnit >= highSurrogateMin) && (codeUnit <= highSurrogateMax);
}

bool Utf16View::isLowSurrogate(UInt16 codeUnit) {

    return (codeUnit >= lowSurrogateMin) && (codeUnit <= lowSurrogateMax);
}

UInt32 Utf16View::decodeSurrogatePair(UInt16 highSurrogate, UInt16 lowSurrogate) {

    VERIFY(isHighSurrogate(highSurrogate));
    
    VERIFY(isLowSurrogate(lowSurrogate));

    return ((highSurrogate - highSurrogateMin) << 10) + (lowSurrogate - lowSurrogateMin) + firstSupplementaryPlaneCodePoint;
}

String Utf16View::toUtf8(AllowInvalidCodeUnits allowInvalidCodeUnits) const {

    StringBuilder builder;

    if (allowInvalidCodeUnits == AllowInvalidCodeUnits::Yes) {

        for (auto const* ptr = beginPointer(); ptr < endPointer(); ++ptr) {

            if (isHighSurrogate(*ptr)) {

                auto const* next = ptr + 1;

                if ((next < endPointer()) && isLowSurrogate(*next)) {

                    auto codePoint = decodeSurrogatePair(*ptr, *next);
                    
                    builder.appendCodePoint(codePoint);
                    
                    ++ptr;
                    
                    continue;
                }
            }

            builder.appendCodePoint(static_cast<UInt32>(*ptr));
        }
    } 
    else {

        for (auto codePoint : *this) {

            builder.appendCodePoint(codePoint);
        }
    }

    return builder.build();
}


// size_t Utf16View::lengthInCodePoints() const {

//     if (!m_lengthInCodePoints.hasValue()) {

//         m_lengthInCodePoints = calculateLengthInCodePoints();
//     }

//     return *m_lengthInCodePoints;
// }
