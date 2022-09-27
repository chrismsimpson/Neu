
#include "AnyOf.h"
#include "ByteBuffer.h"
#include "Find.h"
#include "Function.h"
#include "StringView.h"
#include "Vector.h"

#ifndef OS
#    include "FlyString.h"
#    include "String.h"
#endif

#ifndef OS

StringView::StringView(String const& string)
    : m_characters(string.characters()), 
      m_length(string.length()) { }

StringView::StringView(FlyString const& string)
    : m_characters(string.characters()), 
      m_length(string.length()) { }

#endif

StringView::StringView(ByteBuffer const& buffer)
    : m_characters((char const*) buffer.data()), 
      m_length(buffer.size()) { }


Vector<StringView> StringView::splitView(char const separator, bool keepEmpty) const {

    StringView seperatorView { &separator, 1 };

    return splitView(seperatorView, keepEmpty);
}

Vector<StringView> StringView::splitView(StringView separator, bool keepEmpty) const {

    Vector<StringView> parts;

    forEachSplitView(separator, keepEmpty, [&](StringView view) {

        parts.append(view);
    });

    return parts;
}

Vector<StringView> StringView::lines(bool considerCarriageReturn) const {

    if (isEmpty()) {

        return { };
    }

    if (!considerCarriageReturn) {

        return splitView('\n', true);
    }

    Vector<StringView> v;
    
    size_t substart = 0;
    
    bool lastChWasCarriageReturn = false;
    
    bool splitView = false;
    
    for (size_t i = 0; i < length(); ++i) {
        
        char ch = charactersWithoutNullTermination()[i];

        if (ch == '\n') {

            splitView = true;

            if (lastChWasCarriageReturn) {

                substart = i + 1;
                
                splitView = false;
            }
        }
        if (ch == '\r') {

            splitView = true;
            
            lastChWasCarriageReturn = true;
        } 
        else {
            
            lastChWasCarriageReturn = false;
        }

        if (splitView) {
            
            size_t sublen = i - substart;
            
            v.append(substringView(substart, sublen));
            
            substart = i + 1;
        }

        splitView = false;
    }
    
    size_t taillen = length() - substart;
    
    if (taillen != 0) {

        v.append(substringView(substart, taillen));
    }
    
    return v;
}

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

bool StringView::matches(StringView mask, Vector<MaskSpan>& maskSpans, CaseSensitivity caseSensitivity) const {

    return StringUtils::matches(*this, mask, caseSensitivity, &maskSpans);
}

bool StringView::matches(StringView mask, CaseSensitivity caseSensitivity) const {

    return StringUtils::matches(*this, mask, caseSensitivity);
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

#ifndef OS

String StringView::toLowercaseString() const {

    return StringImpl::createLowercased(charactersWithoutNullTermination(), length());
}

String StringView::toUppercaseString() const {

    return StringImpl::createUppercased(charactersWithoutNullTermination(), length());
}

String StringView::toTitlecaseString() const {

    return StringUtils::toTitlecase(*this);
}

#endif

StringView StringView::substringViewStartingFromSubstring(StringView substring) const {

    char const* remainingCharacters = substring.charactersWithoutNullTermination();
    
    VERIFY(remainingCharacters >= m_characters);
    
    VERIFY(remainingCharacters <= m_characters + m_length);
    
    size_t remainingLength = m_length - (remainingCharacters - m_characters);
    
    return { remainingCharacters, remainingLength };
}

StringView StringView::substringViewStartingAfterSubstring(StringView substring) const {

    char const* remainingCharacters = substring.charactersWithoutNullTermination() + substring.length();
    
    VERIFY(remainingCharacters >= m_characters);
    
    VERIFY(remainingCharacters <= m_characters + m_length);
    
    size_t remainingLength = m_length - (remainingCharacters - m_characters);
    
    return { remainingCharacters, remainingLength };
}

bool StringView::copyCharactersToBuffer(char* buffer, size_t bufferSize) const {
    
    // We must fit at least the NUL-terminator.
    
    VERIFY(bufferSize > 0);

    size_t charactersToCopy = min(m_length, bufferSize - 1);
    
    __builtin_memcpy(buffer, m_characters, charactersToCopy);
    
    buffer[charactersToCopy] = 0;

    return charactersToCopy == m_length;
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

#ifndef OS

bool StringView::operator==(String const& string) const {

    return *this == string.view();
}

String StringView::toString() const { return String { *this }; }

String StringView::replace(StringView needle, StringView replacement, bool allOccurrences) const {

    return StringUtils::replace(*this, needle, replacement, allOccurrences);
}

#endif

Vector<size_t> StringView::findAll(StringView needle) const {

    return StringUtils::findAll(*this, needle);
}

Vector<StringView> StringView::splitViewIf(Function<bool(char)> const& predicate, bool keepEmpty) const {

    if (isEmpty()) {

        return { };
    }

    Vector<StringView> v;
    
    size_t substart = 0;
    
    for (size_t i = 0; i < length(); ++i) {
        
        char ch = charactersWithoutNullTermination()[i];
        
        if (predicate(ch)) {

            size_t sublen = i - substart;
            
            if (sublen != 0 || keepEmpty) {

                v.append(substringView(substart, sublen));
            }
            
            substart = i + 1;
        }
    }
    
    size_t taillen = length() - substart;
    
    if (taillen != 0 || keepEmpty) {

        v.append(substringView(substart, taillen));
    }
    
    return v;
}
