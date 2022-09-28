
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

































size_t Utf16View::lengthInCodePoints() const {

    if (!m_lengthInCodePoints.hasValue()) {

        m_lengthInCodePoints = calculateLengthInCodePoints();
    }

    return *m_lengthInCodePoints;
}

UInt16 Utf16View::codeUnitAt(size_t index) const {

    VERIFY(index < lengthInCodeUnits());
    
    return m_codeUnits[index];
}

UInt32 Utf16View::codePointAt(size_t index) const {

    VERIFY(index < lengthInCodeUnits());

    UInt32 codePoint = codeUnitAt(index);

    if (!isHighSurrogate(codePoint) && !isLowSurrogate(codePoint)) {

        return codePoint;
    }

    if (isLowSurrogate(codePoint) || (index + 1 == lengthInCodeUnits())) {

        return codePoint;
    }

    auto second = codeUnitAt(index + 1);

    if (!isLowSurrogate(second)) {

        return codePoint;
    }

    return decodeSurrogatePair(codePoint, second);
}

size_t Utf16View::codePointOffsetOf(size_t codeUnitOffset) const {

    size_t codePointOffset = 0;

    for (auto it = begin(); it != end(); ++it) {

        if (codeUnitOffset == 0) {

            return codePointOffset;
        }

        codeUnitOffset -= it.lengthInCodeUnits();

        ++codePointOffset;
    }

    return codePointOffset;
}

size_t Utf16View::codeUnitOffsetOf(size_t codePointOffset) const {

    size_t codeUnitOffset = 0;

    for (auto it = begin(); it != end(); ++it) {

        if (codePointOffset == 0) {

            return codeUnitOffset;
        }

        codeUnitOffset += it.lengthInCodeUnits();

        --codePointOffset;
    }

    return codeUnitOffset;
}

size_t Utf16View::codeUnitOffsetOf(Utf16CodePointIterator const& it) const {

    VERIFY(it.m_ptr >= beginPointer());
    
    VERIFY(it.m_ptr <= endPointer());

    return it.m_ptr - beginPointer();
}

Utf16View Utf16View::substringView(size_t codeUnitOffset, size_t codeUnitLength) const {

    VERIFY(!Checked<size_t>::additionWouldOverflow(codeUnitOffset, codeUnitLength));
    
    VERIFY(codeUnitOffset + codeUnitLength <= lengthInCodeUnits());

    return Utf16View { m_codeUnits.slice(codeUnitOffset, codeUnitLength) };
}

Utf16View Utf16View::unicodeSubstringView(size_t codePointOffset, size_t codePointLength) const {

    if (codePointLength == 0) {

        return { };
    }

    auto codeUnitOffsetOf = [&](Utf16CodePointIterator const& it) { return it.m_ptr - beginPointer(); };

    size_t codePointIndex = 0;

    size_t codeUnitOffset = 0;

    for (auto it = begin(); it != end(); ++it) {

        if (codePointIndex == codePointOffset) {

            codeUnitOffset = codeUnitOffsetOf(it);
        }

        if (codePointIndex == (codePointOffset + codePointLength - 1)) {
            
            size_t codeUnitLength = codeUnitOffsetOf(++it) - codeUnitOffset;
            
            return substringView(codeUnitOffset, codeUnitLength);
        }

        ++codePointIndex;
    }

    VERIFY_NOT_REACHED();
}

bool Utf16View::validate(size_t& validCodeUnits) const {

    validCodeUnits = 0;

    for (auto const* ptr = beginPointer(); ptr < endPointer(); ++ptr) {

        if (isHighSurrogate(*ptr)) {

            if ((++ptr >= endPointer()) || !isLowSurrogate(*ptr)) {

                return false;
            }

            ++validCodeUnits;
        } 
        else if (isLowSurrogate(*ptr)) {

            return false;
        }

        ++validCodeUnits;
    }

    return true;
}

size_t Utf16View::calculateLengthInCodePoints() const {

    size_t codePoints = 0;
    
    for ([[maybe_unused]] auto codePoint : *this) {

        ++codePoints;
    }

    return codePoints;
}

bool Utf16View::equalsIgnoringCase(Utf16View const& other) const {

    if (lengthInCodeUnits() == 0) {

        return other.lengthInCodeUnits() == 0;
    }

    if (lengthInCodeUnits() != other.lengthInCodeUnits()) {

        return false;
    }

    for (size_t i = 0; i < lengthInCodeUnits(); ++i) {
        
        // FIXME: Handle non-ASCII case insensitive comparisons.
        
        if (toAsciiLowercase(m_codeUnits[i]) != toAsciiLowercase(other.m_codeUnits[i])) {

            return false;
        }
    }

    return true;
}

Utf16CodePointIterator& Utf16CodePointIterator::operator++() {

    size_t codeUnits = lengthInCodeUnits();

    if (codeUnits > m_remainingCodeUnits) {
        
        // If there aren't enough code units remaining, skip to the end.
        
        m_ptr += m_remainingCodeUnits;
        
        m_remainingCodeUnits = 0;
    } 
    else {
        
        m_ptr += codeUnits;
        
        m_remainingCodeUnits -= codeUnits;
    }

    return *this;
}

UInt32 Utf16CodePointIterator::operator*() const {

    VERIFY(m_remainingCodeUnits > 0);

    if (Utf16View::isHighSurrogate(*m_ptr)) {

        if ((m_remainingCodeUnits > 1) && Utf16View::isLowSurrogate(*(m_ptr + 1))) {

            return Utf16View::decodeSurrogatePair(*m_ptr, *(m_ptr + 1));
        }

        return replacementCodePoint;
    } 
    else if (Utf16View::isLowSurrogate(*m_ptr)) {
        
        return replacementCodePoint;
    }

    return static_cast<UInt32>(*m_ptr);
}

size_t Utf16CodePointIterator::lengthInCodeUnits() const {

    VERIFY(m_remainingCodeUnits > 0);

    if (Utf16View::isHighSurrogate(*m_ptr)) {

        if ((m_remainingCodeUnits > 1) && Utf16View::isLowSurrogate(*(m_ptr + 1))) {

            return 2;
        }
    }

    // If this return is reached, either the encoded code point is a valid single code unit, or that
    // code point is invalid (e.g. began with a low surrogate, or a low surrogate did not follow a
    // high surrogate). In the latter case, a single replacement code unit will be used.
    
    return 1;
}