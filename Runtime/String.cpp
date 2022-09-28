
#include "ByteBuffer.h"
#include "FlyString.h"
#include "Format.h"
#include "Function.h"
#include "Memory.h"
#include "std.h"
#include "String.h"
#include "StringView.h"
#include "Vector.h"

bool String::operator==(FlyString const& flyString) const {

    return m_impl == flyString.impl() || view() == flyString.view();
}

bool String::operator==(String const& other) const {

    return m_impl == other.impl() || view() == other.view();
}

bool String::operator==(StringView other) const {

    return view() == other;
}

bool String::operator<(String const& other) const {

    return view() < other.view();
}

bool String::operator>(String const& other) const {

    return view() > other.view();
}

bool String::copyCharactersToBuffer(char* buffer, size_t bufferSize) const {

    // We must fit at least the NUL-terminator.
    
    VERIFY(bufferSize > 0);

    size_t charactersToCopy = min(length(), bufferSize - 1);

    __builtin_memcpy(buffer, characters(), charactersToCopy);

    buffer[charactersToCopy] = 0;

    return charactersToCopy == length();
}

String String::isolatedCopy() const {

    if (!m_impl) {

        return {};
    }

    if (!m_impl->length()) {

        return empty();
    }

    char* buffer;
    
    auto impl = StringImpl::createUninitialized(length(), buffer);
    
    memcpy(buffer, m_impl->characters(), m_impl->length());
    
    return String(move(*impl));
}

String String::substring(size_t start, size_t length) const {

    if (!length) {

        return String::empty();
    }

    VERIFY(m_impl);
    
    VERIFY(!Checked<size_t>::additionWouldOverflow(start, length));
    
    VERIFY(start + length <= m_impl->length());
    
    return { characters() + start, length };
}

String String::substring(size_t start) const {

    VERIFY(m_impl);
    
    VERIFY(start <= length());
    
    return { characters() + start, length() - start };
}

StringView String::substringView(size_t start, size_t length) const {

    VERIFY(m_impl);
    
    VERIFY(!Checked<size_t>::additionWouldOverflow(start, length));
    
    VERIFY(start + length <= m_impl->length());
    
    return { characters() + start, length };
}

StringView String::substringView(size_t start) const {

    VERIFY(m_impl);
    
    VERIFY(start <= length());
    
    return { characters() + start, length() - start };
}

Vector<String> String::split(char separator, bool keepEmpty) const {

    return splitLimit(separator, 0, keepEmpty);
}

Vector<String> String::splitLimit(char separator, size_t limit, bool keepEmpty) const {

    if (isEmpty()) {

        return { };
    }

    Vector<String> v;
    
    size_t substart = 0;
    
    for (size_t i = 0; i < length() && (v.size() + 1) != limit; ++i) {
        
        char ch = characters()[i];
        
        if (ch == separator) {
            
            size_t sublen = i - substart;
            
            if (sublen != 0 || keepEmpty) {

                v.append(substring(substart, sublen));
            }

            substart = i + 1;
        }
    }
    
    size_t taillen = length() - substart;

    if (taillen != 0 || keepEmpty) {

        v.append(substring(substart, taillen));
    }

    return v;
}

Vector<StringView> String::splitView(Function<bool(char)> separator, bool keepEmpty) const {

    if (isEmpty()) {

        return { };
    }

    Vector<StringView> v;
    
    size_t substart = 0;
    
    for (size_t i = 0; i < length(); ++i) {
        
        char ch = characters()[i];
        
        if (separator(ch)) {
            
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

Vector<StringView> String::splitView(char const separator, bool keepEmpty) const {

    return splitView([separator](char ch) { return ch == separator; }, keepEmpty);
}

ByteBuffer String::toByteBuffer() const {

    if (!m_impl) {

        return { };
    }

    // FIXME: Handle OOM failure.
    
    return ByteBuffer::copy(bytes()).releaseValueButFixmeShouldPropagateErrors();
}

template<typename T>
Optional<T> String::toInt(TrimWhitespace trimWhitespace) const {

    return StringUtils::convertToInt<T>(view(), trimWhitespace);
}

template Optional<Int8> String::toInt(TrimWhitespace) const;

template Optional<Int16> String::toInt(TrimWhitespace) const;

template Optional<Int32> String::toInt(TrimWhitespace) const;

template Optional<Int64> String::toInt(TrimWhitespace) const;

template<typename T>
Optional<T> String::toUInt(TrimWhitespace trimWhitespace) const {

    return StringUtils::convertToUInt<T>(view(), trimWhitespace);
}

template Optional<UInt8> String::toUInt(TrimWhitespace) const;

template Optional<UInt16> String::toUInt(TrimWhitespace) const;

template Optional<UInt32> String::toUInt(TrimWhitespace) const;

template Optional<unsigned long> String::toUInt(TrimWhitespace) const;

template Optional<unsigned long long> String::toUInt(TrimWhitespace) const;

bool String::startsWith(StringView str, CaseSensitivity caseSensitivity) const {

    return StringUtils::startsWith(*this, str, caseSensitivity);
}

bool String::startsWith(char ch) const {

    if (isEmpty()) {

        return false;
    }

    return characters()[0] == ch;
}

bool String::endsWith(StringView str, CaseSensitivity caseSensitivity) const {

    return StringUtils::endsWith(*this, str, caseSensitivity);
}

bool String::endsWith(char ch) const {
    
    if (isEmpty()) {

        return false;
    }

    return characters()[length() - 1] == ch;
}

String String::repeated(char ch, size_t count) {

    if (!count) {

        return empty();
    }

    char* buffer;
    
    auto impl = StringImpl::createUninitialized(count, buffer);
    
    memset(buffer, ch, count);
    
    return *impl;
}

String String::repeated(StringView string, size_t count) {

    if (!count || string.isEmpty()) {

        return empty();
    }

    char* buffer;
    
    auto impl = StringImpl::createUninitialized(count * string.length(), buffer);
    
    for (size_t i = 0; i < count; i++) {

        __builtin_memcpy(buffer + i * string.length(), string.charactersWithoutNullTermination(), string.length());
    }

    return *impl;
}

String String::bijectiveBaseFrom(size_t value, unsigned base, StringView map) {

    if (map.isNull()) {

        map = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"sv;
    }

    VERIFY(base >= 2 && base <= map.length());

    // The '8 bits per byte' assumption may need to go?
    
    Array<char, roundUpToPowerOfTwo(sizeof(size_t) * 8 + 1, 2)> buffer;
    
    size_t i = 0;
    
    do {
        
        buffer[i++] = map[value % base];
        
        value /= base;
    } 
    while (value > 0);

    // NOTE: Weird as this may seem, the thing that comes after 'Z' is 'AA', which as a number would be '00'
    //       to make this work, only the most significant digit has to be in a range of (1..25) as opposed to (0..25),
    //       but only if it's not the only digit in the string.
    
    if (i > 1) {

        --buffer[i - 1];
    }

    for (size_t j = 0; j < i / 2; ++j) {

        swap(buffer[j], buffer[i - j - 1]);
    }

    return String { ReadOnlyBytes(buffer.data(), i) };
}

String String::romanNumberFrom(size_t value) {

    if (value > 3999) {

        return String::number(value);
    }

    StringBuilder builder;

    while (value > 0) {
        
        if (value >= 1000) {
            
            builder.append('M');
            
            value -= 1000;
        } 
        else if (value >= 900) {
            
            builder.append("CM"sv);
            
            value -= 900;
        } 
        else if (value >= 500) {
            
            builder.append('D');
            
            value -= 500;
        } 
        else if (value >= 400) {
            
            builder.append("CD"sv);
            
            value -= 400;
        } 
        else if (value >= 100) {
            
            builder.append('C');
            
            value -= 100;
        } 
        else if (value >= 90) {
            
            builder.append("XC"sv);
        
            value -= 90;
        } 
        else if (value >= 50) {
            
            builder.append('L');
            
            value -= 50;
        } 
        else if (value >= 40) {
            
            builder.append("XL"sv);
        
            value -= 40;
        } 
        else if (value >= 10) {
            
            builder.append('X');
            
            value -= 10;
        } 
        else if (value == 9) {
            
            builder.append("IX"sv);
            
            value -= 9;
        }
        else if (value >= 5 && value <= 8) {
            
            builder.append('V');
            
            value -= 5;
        } 
        else if (value == 4) {
            
            builder.append("IV"sv);
            
            value -= 4;
        } 
        else if (value <= 3) {
            
            builder.append('I');
            
            value -= 1;
        }
    }

    return builder.toString();
}

bool String::matches(StringView mask, Vector<MaskSpan>& maskSpans, CaseSensitivity caseSensitivity) const {

    return StringUtils::matches(*this, mask, caseSensitivity, &maskSpans);
}

bool String::matches(StringView mask, CaseSensitivity caseSensitivity) const {

    return StringUtils::matches(*this, mask, caseSensitivity);
}

bool String::contains(StringView needle, CaseSensitivity caseSensitivity) const {

    return StringUtils::contains(*this, needle, caseSensitivity);
}

bool String::contains(char needle, CaseSensitivity caseSensitivity) const {

    return StringUtils::contains(*this, StringView(&needle, 1), caseSensitivity);
}

bool String::equalsIgnoringCase(StringView other) const {

    return StringUtils::equalsIgnoringCase(view(), other);
}

String String::reverse() const {

    StringBuilder reversedString(length());

    for (size_t i = length(); i-- > 0;) {
        
        reversedString.append(characters()[i]);
    }

    return reversedString.toString();
}

String escapeHtmlEntities(StringView html) {

    StringBuilder builder;

    for (size_t i = 0; i < html.length(); ++i) {

        if (html[i] == '<') {

            builder.append("&lt;");
        }
        else if (html[i] == '>') {

            builder.append("&gt;");
        }
        else if (html[i] == '&') {

            builder.append("&amp;");
        }
        else if (html[i] == '"') {

            builder.append("&quot;");
        }
        else {

            builder.append(html[i]);
        }
    }

    return builder.toString();
}

String::String(FlyString const& string)
    : m_impl(string.impl()) { }

String String::toLowercase() const {

    if (!m_impl) {

        return { };
    }

    return m_impl->toLowercase();
}

String String::toUppercase() const {

    if (!m_impl) {

        return { };
    }

    return m_impl->toUppercase();
}

String String::toSnakecase() const {

    return StringUtils::toSnakecase(*this);
}

String String::toTitlecase() const {

    return StringUtils::toTitlecase(*this);
}

bool operator<(char const* characters, String const& string) {

    return string.view() > characters;
}

bool operator>=(char const* characters, String const& string) {

    return string.view() <= characters;
}

bool operator>(char const* characters, String const& string) {

    return string.view() < characters;
}

bool operator<=(char const* characters, String const& string) {

    return string.view() >= characters;
}

bool String::operator==(char const* cstring) const {

    return view() == cstring;
}

InputStream& operator>>(InputStream& stream, String& string) {

    StringBuilder builder;

    for (;;) {

        char nextChar;
        
        stream >> nextChar;

        if (stream.hasAnyError()) {
            
            stream.setFatalError();
            
            string = nullptr;
            
            return stream;
        }

        if (nextChar) {

            builder.append(nextChar);
        } 
        else {
            
            string = builder.toString();
            
            return stream;
        }
    }
}

String String::vformatted(StringView fmtstr, TypeErasedFormatParams& params) {

    StringBuilder builder;
    
    MUST(vformat(builder, fmtstr, params));
    
    return builder.toString();
}

Vector<size_t> String::findAll(StringView needle) const {

    return StringUtils::findAll(*this, needle);
}

String& String::operator+=(String const& other) {

    if (other.isEmpty()) {

        return *this;
    }

    StringBuilder builder;
    
    builder.append(*this);
    
    builder.append(other);
    
    *this = builder.toString();
    
    return *this;
}

String operator+(String const& a, String const& b) {

    StringBuilder builder;
    
    builder.append(a);
    
    builder.append(b);
    
    return builder.toString();
}
