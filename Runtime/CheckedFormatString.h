
#pragma once

#include "AllOf.h"
#include "AnyOf.h"
#include "Array.h"
#include "std.h"
#include "StringView.h"

#ifdef ENABLE_COMPILETIME_FORMAT_CHECK
// FIXME: Seems like clang doesn't like calling 'consteval' functions inside 'consteval' functions quite the same way as GCC does,
//        it seems to entirely forget that it accepted that parameters to a 'consteval' function to begin with.
#    if defined(__clang__) || defined(__CLION_IDE_)
#        undef ENABLE_COMPILETIME_FORMAT_CHECK
#    endif
#endif

#ifdef ENABLE_COMPILETIME_FORMAT_CHECK

namespace Format::Detail {

    // We have to define a local "purely constexpr" Array that doesn't lead back to us (via e.g. VERIFY)
    template<typename T, size_t Size>
    struct Array {

        constexpr static size_t size() { return Size; }
        
        constexpr const T& operator[](size_t index) const { return __data[index]; }

        constexpr T& operator[](size_t index) { return __data[index]; }
        
        using ConstIterator = SimpleIterator<const Array, const T>;
        
        using Iterator = SimpleIterator<Array, T>;

        constexpr ConstIterator begin() const { return ConstIterator::begin(*this); }
        
        constexpr Iterator begin() { return Iterator::begin(*this); }

        constexpr ConstIterator end() const { return ConstIterator::end(*this); }
        
        constexpr Iterator end() { return Iterator::end(*this); }

        T __data[Size];
    };

    template<typename... Args>
    void compileTimeFail(Args...);

    template<size_t N>
    consteval auto extractUsedArgumentIndex(char const (&fmt)[N], size_t specifierStartIndex, size_t specifierEndIndex, size_t& nextImplicitArgumentIndex) {

        struct {
            size_t indexValue { 0 };
            bool sawExplicitIndex { false };
        } state;

        for (size_t i = specifierStartIndex; i < specifierEndIndex; ++i) {

            auto c = fmt[i];

            if (c > '9' || c < '0') {

                break;
            }

            state.indexValue *= 10;
            
            state.indexValue += c - '0';
            
            state.sawExplicitIndex = true;
        }

        if (!state.sawExplicitIndex) {

            return nextImplicitArgumentIndex++;
        }

        return state.indexValue;
    }

    ///

    // FIXME: We should rather parse these format strings at compile-time if possible.

    template<size_t N>
    consteval auto countFormatParams(char const (&fmt)[N]) {

        struct {
            
            // FIXME: Switch to variable-sized storage whenever we can come up with one :)
            
            Array<size_t, 128> usedArguments { 0 };
            
            size_t totalUsedArgumentCount { 0 };
            
            size_t nextImplicitArgumentIndex { 0 };
            
            bool hasExplicitArgumentReferences { false };

            size_t unclosedBraces { 0 };
            
            size_t extraClosedBraces { 0 };
            
            size_t nestingLevel { 0 };

            Array<size_t, 4> lastFormatSpecifierStart { 0 };
            
            size_t totalUsedLastFormatSpecifierStartCount { 0 };

        } result;

        for (size_t i = 0; i < N; ++i) {
            
            auto ch = fmt[i];
            
            switch (ch) {

            case '{':

                if (i + 1 < N && fmt[i + 1] == '{') {

                    ++i;
                    
                    continue;
                }

                // Note: There's no compile-time throw, so we have to abuse a compile-time string to store errors.

                if (result.totalUsedLastFormatSpecifierStartCount >= result.lastFormatSpecifierStart.size() - 1) {

                    compileTimeFail("Format-String Checker internal error: Format specifier nested too deep"); 
                }

                result.lastFormatSpecifierStart[result.totalUsedLastFormatSpecifierStartCount++] = i + 1;

                ++result.unclosedBraces;
                
                ++result.nestingLevel;

                break;

            ///

            case '}':

                if (result.nestingLevel == 0) {

                    if (i + 1 < N && fmt[i + 1] == '}') {

                        ++i;
                        
                        continue;
                    }
                }

                if (result.unclosedBraces) {
                    
                    --result.nestingLevel;
                    
                    --result.unclosedBraces;

                    if (result.totalUsedLastFormatSpecifierStartCount == 0) {

                        compileTimeFail("Format-String Checker internal error: Expected location information");
                    }

                    auto const specifierStartIndex = result.lastFormatSpecifierStart[--result.totalUsedLastFormatSpecifierStartCount];

                    if (result.totalUsedArgumentCount >= result.usedArguments.size()) {

                        compileTimeFail("Format-String Checker internal error: Too many format arguments in format string");
                    }

                    auto usedArgumentIndex = extractUsedArgumentIndex<N>(fmt, specifierStartIndex, i, result.nextImplicitArgumentIndex);

                    if (usedArgumentIndex + 1 != result.nextImplicitArgumentIndex) {

                        result.hasExplicitArgumentReferences = true;
                    }

                    result.usedArguments[result.totalUsedArgumentCount++] = usedArgumentIndex;

                }
                else {

                    ++result.extraClosedBraces;
                }
                break;

            ///

            default:

                continue;
            }
        }

        return result;
    }
}

#endif

namespace Format::Detail {

    template<typename... Args>
    struct CheckedFormatString {
            
        template<size_t N>
        consteval CheckedFormatString(char const (&fmt)[N])
            : m_string { fmt } {
    
        #ifdef ENABLE_COMPILETIME_FORMAT_CHECK

            checkFormatParameterConsistency<N, sizeof...(Args)>(fmt);

        #endif
        }

        template<typename T>
        CheckedFormatString(const T& uncheckedFmt) requires(requires(T t) { StringView { t }; })
            : m_string(uncheckedFmt) { }

        auto view() const { return m_string; }

    private:

    #ifdef ENABLE_COMPILETIME_FORMAT_CHECK

        template<size_t N, size_t paramCount>
        consteval static bool checkFormatParameterConsistency(char const (&fmt)[N]) {

            auto check = countFormatParams<N>(fmt);

            if (check.unclosedBraces != 0) {

                compileTimeFail("Extra unclosed braces in format string");
            }

            if (check.extraClosedBraces != 0) {

                compileTimeFail("Extra closing braces in format string");
            }

            {
                auto begin = check.usedArguments.begin();
                
                auto end = check.usedArguments.begin() + check.totalUsedArgumentCount;
                
                auto hasAllReferencedArguments = !::anyOf(begin, end, [](auto& entry) { return entry >= paramCount; });
                
                if (!hasAllReferencedArguments) {

                    compileTimeFail("Format string references nonexistent parameter");
                }
            }

            if (!check.hasExplicitArgumentReferences && check.totalUsedArgumentCount != paramCount) {

                compileTimeFail("Format string does not reference all passed parameters");
            }

            // Ensure that no passed parameter is ignored or otherwise not referenced in the format
            // As this check is generally pretty expensive, try to avoid it where it cannot fail.
            // We will only do this check if the format string has explicit argument refs
            // otherwise, the check above covers this check too, as implicit refs
            // monotonically increase, and cannot have 'gaps'.

            if (check.hasExplicitArgumentReferences) {
                
                auto allParameters = iotaArray<size_t, paramCount>(0);
                
                constexpr auto contains = [](auto begin, auto end, auto entry) {

                    for (; begin != end; begin++) {

                        if (*begin == entry) {

                            return true;
                        }
                    }

                    return false;
                };

                auto referencesAllArguments = ::allOf(
                    allParameters,
                    [&](auto& entry) {

                        return contains(
                            check.usedArguments.begin(),
                            check.usedArguments.begin() + check.totalUsedArgumentCount,
                            entry);
                    });

                if (!referencesAllArguments) {

                    compileTimeFail("Format string does not reference all passed parameters");
                }
            }

            return true;
        }

    #endif
    
        StringView m_string;
    };
}

template<typename... Args>
using CheckedFormatString = Format::Detail::CheckedFormatString<IdentityType<Args>...>;
