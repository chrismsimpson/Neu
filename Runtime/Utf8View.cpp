

#include "Assertions.h"
#include "CharacterTypes.h"
#include "Debug.h"
#include "Format.h"
#include "Utf8View.h"

Utf8CodePointIterator Utf8View::iteratorAtByteOffset(size_t byteOffset) const {

    size_t currentOffset = 0;

    for (auto iterator = begin(); !iterator.done(); ++iterator) {

        if (currentOffset >= byteOffset) {

            return iterator;
        }
        
        currentOffset += iterator.underlyingCodePointLengthInBytes();
    }

    return end();
}

size_t Utf8View::byteOffsetOf(Utf8CodePointIterator const& it) const {

    VERIFY(it.m_ptr >= beginPointer());

    VERIFY(it.m_ptr <= endPointer());

    return it.m_ptr - beginPointer();
}

size_t Utf8View::byteOffsetOf(size_t codePointOffset) const {

    size_t byteOffset = 0;

    for (auto it = begin(); !it.done(); ++it) {

        if (codePointOffset == 0) {

            return byteOffset;
        }

        byteOffset += it.underlyingCodePointLengthInBytes();

        --codePointOffset;
    }

    return byteOffset;
}

Utf8View Utf8View::unicodeSubstringView(size_t codePointOffset, size_t codePointLength) const {

    if (codePointLength == 0) {

        return { };
    }

    size_t codePointIndex = 0, offsetInBytes = 0;

    for (auto iterator = begin(); !iterator.done(); ++iterator) {

        if (codePointIndex == codePointOffset) {

            offsetInBytes = byteOffsetOf(iterator);
        }

        if (codePointIndex == codePointOffset + codePointLength - 1) {

            size_t length_in_bytes = byteOffsetOf(++iterator) - offsetInBytes;
            
            return substringView(offsetInBytes, length_in_bytes);
        }

        ++codePointIndex;
    }

    VERIFY_NOT_REACHED();
}


static inline bool decodeFirstByte(
    unsigned char byte,
    size_t& outCodePointLengthInBytes,
    UInt32& outValue) {

    if ((byte & 128) == 0) {
        
        outValue = byte;
        
        outCodePointLengthInBytes = 1;
        
        return true;
    }
    
    if ((byte & 64) == 0) {
    
        return false;
    }
    
    if ((byte & 32) == 0) {
    
        outValue = byte & 31;
    
        outCodePointLengthInBytes = 2;
    
        return true;
    }
    
    if ((byte & 16) == 0) {
    
        outValue = byte & 15;
    
        outCodePointLengthInBytes = 3;
    
        return true;
    }
    
    if ((byte & 8) == 0) {
    
        outValue = byte & 7;
    
        outCodePointLengthInBytes = 4;
    
        return true;
    }

    return false;
}

bool Utf8View::validate(size_t& validBytes) const
{
    validBytes = 0;

    for (auto ptr = beginPointer(); ptr < endPointer(); ptr++) {

        size_t codePointLengthInBytes = 0;

        UInt32 codePoint = 0;
        
        bool firstByteMakesSense = decodeFirstByte(*ptr, codePointLengthInBytes, codePoint);
        
        if (!firstByteMakesSense) {

            return false;
        }

        for (size_t i = 1; i < codePointLengthInBytes; i++) {

            ptr++;

            if (ptr >= endPointer()) {

                return false;
            }

            if (*ptr >> 6 != 2) {

                return false;
            }

            codePoint <<= 6;
            
            codePoint |= *ptr & 63;
        }

        if (!isUnicode(codePoint)) {

            return false;
        }

        validBytes += codePointLengthInBytes;
    }

    return true;
}

size_t Utf8View::calculateLength() const {

    size_t length = 0;

    for ([[maybe_unused]] auto codePoint : *this) {

        ++length;
    }

    return length;
}

bool Utf8View::startsWith(Utf8View const& start) const {

    if (start.isEmpty()) {

        return true;
    }

    if (isEmpty()) {

        return false;
    }

    if (start.length() > length()) {

        return false;
    }

    if (beginPointer() == start.beginPointer()) {

        return true;
    }

    for (auto k = begin(), l = start.begin(); l != start.end(); ++k, ++l) {

        if (*k != *l) {

            return false;
        }
    }

    return true;
}

bool Utf8View::contains(UInt32 needle) const {

    for (UInt32 codePoint : *this) {

        if (codePoint == needle) {

            return true;
        }
    }
        
    return false;
}

Utf8View Utf8View::trim(Utf8View const& characters, TrimMode mode) const {

    size_t substringStart = 0;
    
    size_t substringLength = byteLength();

    if (mode == TrimMode::Left || mode == TrimMode::Both) {

        for (auto codePoint = begin(); codePoint != end(); ++codePoint) {

            if (substringLength == 0) {

                return {};
            }

            if (!characters.contains(*codePoint)) {

                break;
            }

            substringStart += codePoint.underlyingCodePointLengthInBytes();
            
            substringLength -= codePoint.underlyingCodePointLengthInBytes();
        }
    }

    if (mode == TrimMode::Right || mode == TrimMode::Both) {

        size_t seenWhitespaceLength = 0;

        for (auto codePoint = begin(); codePoint != end(); ++codePoint) {

            if (characters.contains(*codePoint)) {

                seenWhitespaceLength += codePoint.underlyingCodePointLengthInBytes();
            }
            else {

                seenWhitespaceLength = 0;
            }

        }

        if (seenWhitespaceLength >= substringLength) {

            return { };
        }

        substringLength -= seenWhitespaceLength;
    }

    return substringView(substringStart, substringLength);
}

Utf8CodePointIterator& Utf8CodePointIterator::operator++() {

    VERIFY(m_length > 0);

    size_t codePointLengthInBytes = underlyingCodePointLengthInBytes();

    if (codePointLengthInBytes > m_length) {

        // We don't have enough data for the next code point. Skip one character and try again.
        // The rest of the code will output replacement characters as needed for any eventual extension bytes we might encounter afterwards.
        
        debugLineIf(UTF8_DEBUG, "Expected code point size {} is too big for the remaining length {}. Moving forward one byte.", codePointLengthInBytes, m_length);
        
        m_ptr += 1;
        
        m_length -= 1;
        
        return *this;
    }

    m_ptr += codePointLengthInBytes;
    
    m_length -= codePointLengthInBytes;
    
    return *this;
}

size_t Utf8CodePointIterator::underlyingCodePointLengthInBytes() const {
    
    VERIFY(m_length > 0);
    
    size_t codePointLengthInBytes = 0;
    
    UInt32 value;

    bool firstByteMakesSense = decodeFirstByte(*m_ptr, codePointLengthInBytes, value);

    // If any of these tests fail, we will output a replacement character for this byte and treat it as a code point of size 1.
    
    if (!firstByteMakesSense) {

        return 1;
    }

    if (codePointLengthInBytes > m_length) {

        return 1;
    }

    for (size_t offset = 1; offset < codePointLengthInBytes; offset++) {

        if (m_ptr[offset] >> 6 != 2) {

            return 1;
        }
    }

    return codePointLengthInBytes;
}

ReadOnlyBytes Utf8CodePointIterator::underlyingCodePointBytes() const {

    return { m_ptr, underlyingCodePointLengthInBytes() };
}

UInt32 Utf8CodePointIterator::operator*() const {

    VERIFY(m_length > 0);

    UInt32 codePointValueSoFar = 0;

    size_t codePointLengthInBytes = 0;

    bool firstByteMakesSense = decodeFirstByte(m_ptr[0], codePointLengthInBytes, codePointValueSoFar);

    if (!firstByteMakesSense) {
        
        // The first byte of the code point doesn't make sense: output a replacement character
        
        debugLineIf(UTF8_DEBUG, "First byte doesn't make sense: {:#02x}.", m_ptr[0]);
        
        return 0xFFFD;
    }

    if (codePointLengthInBytes > m_length) {
        
        // There is not enough data left for the full code point: output a replacement character
        
        debugLineIf(UTF8_DEBUG, "Not enough bytes (need {}, have {}), first byte is: {:#02x}.", codePointLengthInBytes, m_length, m_ptr[0]);
        
        return 0xFFFD;
    }

    for (size_t offset = 1; offset < codePointLengthInBytes; offset++) {

        if (m_ptr[offset] >> 6 != 2) {
            
            // One of the extension bytes of the code point doesn't make sense: output a replacement character
            
            debugLineIf(UTF8_DEBUG, "Extension byte {:#02x} in {} position after first byte {:#02x} doesn't make sense.", m_ptr[offset], offset, m_ptr[0]);
            
            return 0xFFFD;
        }

        codePointValueSoFar <<= 6;
        
        codePointValueSoFar |= m_ptr[offset] & 63;
    }

    return codePointValueSoFar;
}

///

Optional<UInt32> Utf8CodePointIterator::peek(size_t offset) const {

    if (offset == 0) {

        if (this->done()) {

            return { };
        }

        return this->operator*();
    }

    auto newIterator = *this;

    for (size_t index = 0; index < offset; ++index) {

        ++newIterator;

        if (newIterator.done()) {

            return { };
        }
    }

    return *newIterator;
}