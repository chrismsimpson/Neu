/*
 * Copyright (c) 2018-2020, Andreas Kling <awesomekling@gmail.com>
 * Copyright (c) 2020, Fei Wu <f.eiwu@yahoo.com>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#include <Core/CharacterTypes.h>
#include <Core/MemMem.h>
#include <Core/Memory.h>
#include <Core/Optional.h>
#include <Core/String.h>
#include <Core/StringBuilder.h>
#include <Core/StringUtils.h>
#include <Core/StringView.h>

namespace StringUtils {

    template<typename T>
    Optional<T> convertToInt(StringView str, TrimWhitespace trimWhitespace) {

        auto string = trimWhitespace == TrimWhitespace::Yes
            ? str.trimWhitespace()
            : str;

        if (string.isEmpty()) {

            return { };
        }

        T sign = 1;
        
        size_t i = 0;
        
        auto const characters = string.charactersWithoutNullTermination();

        if (characters[0] == '-' || characters[0] == '+') {

            if (string.length() == 1) {

                return { };
            }
            
            i++;
            
            if (characters[0] == '-') {

                sign = -1;
            }
        }

        T value = 0;
        
        for (; i < string.length(); i++) {
            
            if (characters[i] < '0' || characters[i] > '9') {

                return { };
            }

            if (__builtin_mul_overflow(value, 10, &value)) {

                return { };
            }

            if (__builtin_add_overflow(value, sign * (characters[i] - '0'), &value)) {

                return { };
            }
        }

        return value;
    }

    template Optional<Int8> convertToInt(StringView str, TrimWhitespace);
    
    template Optional<Int16> convertToInt(StringView str, TrimWhitespace);
    
    template Optional<Int32> convertToInt(StringView str, TrimWhitespace);
    
    template Optional<long> convertToInt(StringView str, TrimWhitespace);
    
    template Optional<long long> convertToInt(StringView str, TrimWhitespace);

    template<typename T>
    Optional<T> convertToUInt(StringView str, TrimWhitespace trimWhitespace) {

        auto string = trimWhitespace == TrimWhitespace::Yes
            ? str.trimWhitespace()
            : str;

        if (string.isEmpty()) {

            return { };
        }

        T value = 0;

        auto const characters = string.charactersWithoutNullTermination();

        for (size_t i = 0; i < string.length(); i++) {

            if (characters[i] < '0' || characters[i] > '9') {

                return { };
            }

            if (__builtin_mul_overflow(value, 10, &value)) {
 
                return { };
            }

            if (__builtin_add_overflow(value, characters[i] - '0', &value)) {

                return { };
            }
        }

        return value;
    }

    template Optional<UInt8> convertToUInt(StringView str, TrimWhitespace);
    
    template Optional<UInt16> convertToUInt(StringView str, TrimWhitespace);
    
    template Optional<UInt32> convertToUInt(StringView str, TrimWhitespace);
    
    template Optional<unsigned long> convertToUInt(StringView str, TrimWhitespace);
    
    template Optional<unsigned long long> convertToUInt(StringView str, TrimWhitespace);
    
    template Optional<long> convertToUInt(StringView str, TrimWhitespace);
    
    template Optional<long long> convertToUInt(StringView str, TrimWhitespace);

    template<typename T>
    Optional<T> convertToUIntFromHex(StringView str, TrimWhitespace trimWhitespace) {

        auto string = trimWhitespace == TrimWhitespace::Yes
            ? str.trimWhitespace()
            : str;

        if (string.isEmpty()) {

            return { };
        }

        T value = 0;
        
        auto const count = string.length();
        
        const T upperBound = NumericLimits<T>::max();

        for (size_t i = 0; i < count; i++) {

            char digit = string[i];

            UInt8 digitVal;

            if (value > (upperBound >> 4)) {

                return { };
            }

            if (digit >= '0' && digit <= '9') {
                
                digitVal = digit - '0';
            } 
            else if (digit >= 'a' && digit <= 'f') {
                
                digitVal = 10 + (digit - 'a');
            } 
            else if (digit >= 'A' && digit <= 'F') {
                
                digitVal = 10 + (digit - 'A');
            } 
            else {

                return {};
            }

            value = (value << 4) + digitVal;
        }

        return value;
    }

    template Optional<UInt8> convertToUIntFromHex(StringView str, TrimWhitespace);
    
    template Optional<UInt16> convertToUIntFromHex(StringView str, TrimWhitespace);
    
    template Optional<UInt32> convertToUIntFromHex(StringView str, TrimWhitespace);
    
    template Optional<UInt64> convertToUIntFromHex(StringView str, TrimWhitespace);

    template<typename T>
    Optional<T> convertToUIntFromOctal(StringView str, TrimWhitespace trimWhitespace) {

        auto string = trimWhitespace == TrimWhitespace::Yes
            ? str.trimWhitespace()
            : str;

        if (string.isEmpty()) {

            return { };
        }

        T value = 0;
        
        auto const count = string.length();
        
        const T upperBound = NumericLimits<T>::max();

        for (size_t i = 0; i < count; i++) {
            
            char digit = string[i];
            
            UInt8 digitVal;
            
            if (value > (upperBound >> 3)) {

                return { };
            }

            if (digit >= '0' && digit <= '7') {
                
                digitVal = digit - '0';
            } 
            else {

                return { };
            }

            value = (value << 3) + digitVal;
        }

        return value;
    }

    template Optional<UInt8> convertToUIntFromOctal(StringView str, TrimWhitespace);
    
    template Optional<UInt16> convertToUIntFromOctal(StringView str, TrimWhitespace);
    
    template Optional<UInt32> convertToUIntFromOctal(StringView str, TrimWhitespace);
    
    template Optional<UInt64> convertToUIntFromOctal(StringView str, TrimWhitespace);

    bool equalsIgnoringCase(StringView a, StringView b) {

        if (a.length() != b.length()) {

            return false;
        }

        for (size_t i = 0; i < a.length(); ++i) {

            if (toAsciiLowercase(a.charactersWithoutNullTermination()[i]) != toAsciiLowercase(b.charactersWithoutNullTermination()[i])) {

                return false;
            }
        }

        return true;
    }

    bool endsWith(StringView str, StringView end, CaseSensitivity caseSensitivity) {

        if (end.isEmpty()) {

            return true;
        }

        if (str.isEmpty()) {

            return false;
        }

        if (end.length() > str.length()) {

            return false;
        }

        if (caseSensitivity == CaseSensitivity::CaseSensitive) {

            return !memcmp(str.charactersWithoutNullTermination() + (str.length() - end.length()), end.charactersWithoutNullTermination(), end.length());
        }

        auto strChars = str.charactersWithoutNullTermination();
        
        auto endChars = end.charactersWithoutNullTermination();

        size_t si = str.length() - end.length();
        
        for (size_t ei = 0; ei < end.length(); ++si, ++ei) {

            if (toAsciiLowercase(strChars[si]) != toAsciiLowercase(endChars[ei])) {

                return false;
            }
        }

        return true;
    }

    bool startsWith(StringView str, StringView start, CaseSensitivity caseSensitivity) {

        if (start.isEmpty()) {

            return true;
        }

        if (str.isEmpty()) {

            return false;
        }

        if (start.length() > str.length()) {

            return false;
        }

        if (str.charactersWithoutNullTermination() == start.charactersWithoutNullTermination()) {

            return true;
        }

        if (caseSensitivity == CaseSensitivity::CaseSensitive) {

            return !memcmp(str.charactersWithoutNullTermination(), start.charactersWithoutNullTermination(), start.length());
        }

        auto strChars = str.charactersWithoutNullTermination();
        
        auto startChars = start.charactersWithoutNullTermination();

        size_t si = 0;
        
        for (size_t starti = 0; starti < start.length(); ++si, ++starti) {
        
            if (toAsciiLowercase(strChars[si]) != toAsciiLowercase(startChars[starti])) {

                return false;
            }
        }

        return true;
    }

    bool contains(StringView str, StringView needle, CaseSensitivity caseSensitivity) {
        
        if (str.isNull() || needle.isNull() || str.isEmpty() || needle.length() > str.length()) {

            return false;
        }

        if (needle.isEmpty()) {

            return true;
        }

        auto strChars = str.charactersWithoutNullTermination();
        
        auto needleChars = needle.charactersWithoutNullTermination();
        
        if (caseSensitivity == CaseSensitivity::CaseSensitive) {

            return ::memmem(strChars, str.length(), needleChars, needle.length()) != nullptr;
        }

        auto needleFirst = toAsciiLowercase(needleChars[0]);
        
        for (size_t si = 0; si < str.length(); si++) {

            if (toAsciiLowercase(strChars[si]) != needleFirst) {

                continue;
            }

            for (size_t ni = 0; si + ni < str.length(); ni++) {

                if (toAsciiLowercase(strChars[si + ni]) != toAsciiLowercase(needleChars[ni])) {

                    if (ni > 0) {

                        si += ni - 1;
                    }

                    break;
                }

                if (ni + 1 == needle.length()) {

                    return true;
                }
            }
        }

        return false;
    }

    bool isWhitespace(StringView str) {

        return allOf(str, isAsciiSpace);
    }

    StringView trim(StringView str, StringView characters, TrimMode mode) {

        size_t substringStart = 0;
        
        size_t substringLength = str.length();

        if (mode == TrimMode::Left || mode == TrimMode::Both) {

            for (size_t i = 0; i < str.length(); ++i) {

                if (substringLength == 0) {

                    return "";
                }

                if (!characters.contains(str[i])) {

                    break;
                }

                ++substringStart;
                
                --substringLength;
            }
        }

        if (mode == TrimMode::Right || mode == TrimMode::Both) {

            for (size_t i = str.length() - 1; i > 0; --i) {

                if (substringLength == 0) {

                    return "";
                }

                if (!characters.contains(str[i])) {

                    break;
                }

                --substringLength;
            }
        }

        return str.substringView(substringStart, substringLength);
    }

    StringView trimWhitespace(StringView str, TrimMode mode) {

        return trim(str, " \n\t\v\f\r", mode);
    }

    Optional<size_t> find(StringView haystack, char needle, size_t start) {

        if (start >= haystack.length()) {

            return { };
        }

        for (size_t i = start; i < haystack.length(); ++i) { 

            if (haystack[i] == needle) {

                return i;
            }
        }

        return { };
    }

    Optional<size_t> find(StringView haystack, StringView needle, size_t start) {

        if (start > haystack.length()) {

            return { };
        }

        auto index = ::memmemOptional(
            haystack.charactersWithoutNullTermination() + start, haystack.length() - start,
            needle.charactersWithoutNullTermination(), needle.length());

        return index.hasValue() ? (*index + start) : index;
    }

    Optional<size_t> findLast(StringView haystack, char needle) {

        for (size_t i = haystack.length(); i > 0; --i) {

            if (haystack[i - 1] == needle) {

                return i - 1;
            }
        }

        return { };
    }

    ErrorOr<Array<size_t>> findAll(StringView haystack, StringView needle) {

        Array<size_t> positions;

        size_t currentPosition = 0;
        
        while (currentPosition <= haystack.length()) {

            auto maybePosition = ::memmemOptional(
                haystack.charactersWithoutNullTermination() + currentPosition, haystack.length() - currentPosition,
                needle.charactersWithoutNullTermination(), needle.length());

            if (!maybePosition.hasValue()) {

                break;
            }
            
            TRY(positions.push(currentPosition + *maybePosition));
            
            currentPosition += *maybePosition + 1;
        }

        return positions;
    }

    Optional<size_t> find_any_of(StringView haystack, StringView needles, SearchDirection direction) {
        
        if (haystack.isEmpty() || needles.isEmpty()) {

            return { };
        }

        if (direction == SearchDirection::Forward) {

            for (size_t i = 0; i < haystack.length(); ++i) {

                if (needles.contains(haystack[i])) {

                    return i;
                }
            }
        } 
        else if (direction == SearchDirection::Backward) {

            for (size_t i = haystack.length(); i > 0; --i) {

                if (needles.contains(haystack[i - 1])) {

                    return i - 1;
                }
            }
        }

        return { };
    }

    // TODO: Benchmark against KMP (MemMem.h) and switch over if it's faster for short strings too

    size_t count(StringView str, StringView needle) {

        if (needle.isEmpty()) {

            return str.length();
        }

        size_t count = 0;

        for (size_t i = 0; i < str.length() - needle.length() + 1; ++i) {

            if (str.substringView(i).startsWith(needle)) {

                count++;
            }
        }

        return count;
    }
}
