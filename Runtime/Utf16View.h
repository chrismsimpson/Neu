

#pragma once

#include "Format.h"
#include "Forward.h"
#include "Optional.h"
#include "Span.h"
#include "String.h"
#include "Types.h"
#include "Vector.h"

Vector<UInt16, 1> utf8ToUtf16(StringView);

Vector<UInt16, 1> utf8ToUtf16(Utf8View const&);

Vector<UInt16, 1> utf32ToUtf16(Utf32View const&);

void codePointToUtf16(Vector<UInt16, 1>&, UInt32);


class Utf16CodePointIterator {
    
    friend class Utf16View;

public:

    Utf16CodePointIterator() = default;

    ~Utf16CodePointIterator() = default;

    bool operator==(Utf16CodePointIterator const& other) const {

        return (m_ptr == other.m_ptr) && (m_remainingCodeUnits == other.m_remainingCodeUnits);
    }

    bool operator!=(Utf16CodePointIterator const& other) const {

        return !(*this == other);
    }

    Utf16CodePointIterator& operator++();

    UInt32 operator*() const;

    size_t lengthInCodeUnits() const;

private:

    Utf16CodePointIterator(UInt16 const* ptr, size_t length)
        : m_ptr(ptr), 
          m_remainingCodeUnits(length) { }

    UInt16 const* m_ptr { nullptr };

    size_t m_remainingCodeUnits { 0 };
};

///

class Utf16View {

public:

    static bool isHighSurrogate(UInt16);
    
    static bool isLowSurrogate(UInt16);
    
    static UInt32 decodeSurrogatePair(UInt16 highSurrogate, UInt16 lowSurrogate);

    Utf16View() = default;
    
    ~Utf16View() = default;

    explicit Utf16View(Span<UInt16 const> codeUnits)
        : m_codeUnits(codeUnits) { }

    bool operator==(Utf16View const& other) const { return m_codeUnits == other.m_codeUnits; }

    enum class AllowInvalidCodeUnits {

        Yes,
        No
    };

    String toUtf8(AllowInvalidCodeUnits = AllowInvalidCodeUnits::No) const;

    bool isNull() const { return m_codeUnits.isNull(); }
    
    bool isEmpty() const { return m_codeUnits.isEmpty(); }
    
    size_t lengthInCodeUnits() const { return m_codeUnits.size(); }
    
    size_t lengthInCodePoints() const;

    Utf16CodePointIterator begin() const { return { beginPointer(), m_codeUnits.size() }; }

    Utf16CodePointIterator end() const { return { endPointer(), 0 }; }

    UInt16 const* data() const { return m_codeUnits.data(); }
    
    UInt16 codeUnitAt(size_t index) const;
    
    UInt32 codePointAt(size_t index) const;

    size_t codePointOffsetOf(size_t codeUnitOffset) const;
    size_t codeUnitOffsetOf(size_t codePointOffset) const;
    size_t codeUnitOffsetOf(Utf16CodePointIterator const&) const;

    Utf16View substringView(size_t codeUnitOffset, size_t codeUnitLength) const;
    
    Utf16View substringView(size_t codeUnitOffset) const { return substringView(codeUnitOffset, lengthInCodeUnits() - codeUnitOffset); }

    Utf16View unicodeSubstringView(size_t codePointOffset, size_t codePointLength) const;
    
    Utf16View unicodeSubstringView(size_t codePointOffset) const { return unicodeSubstringView(codePointOffset, lengthInCodePoints() - codePointOffset); }

    bool validate(size_t& validCodeUnits) const;

    bool validate() const {

        size_t validCodeUnits;
        
        return validate(validCodeUnits);
    }

    bool equalsIgnoringCase(Utf16View const&) const;

private:
    
    UInt16 const* beginPointer() const { return m_codeUnits.data(); }
    
    UInt16 const* endPointer() const { return beginPointer() + m_codeUnits.size(); }

    size_t calculateLengthInCodePoints() const;

    Span<UInt16 const> m_codeUnits;

    mutable Optional<size_t> m_lengthInCodePoints;
};

// template<>
// struct Formatter<Utf16View> : Formatter<FormatString> {

//     ErrorOr<void> format(FormatBuilder& builder, Utf16View const& value) {

//         return builder.builder().tryAppend(value);
//     }
// };