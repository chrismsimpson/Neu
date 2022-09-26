
#pragma once

#include "Format.h"
#include "std.h"
#include "Types.h"

#include <stdarg.h>
#include <wchar.h>

#ifndef OS

#    include <math.h>

#endif

#ifdef __os__

extern "C" size_t strlen(char const*);

#else

#    include <string.h>

#endif

template<typename PutChFunc, typename T, typename CharType>
ALWAYS_INLINE int printHex(PutChFunc putch, CharType*& bufptr, T number, bool upperCase, bool alternateForm, bool leftPad, bool zeroPad, UInt32 fieldWidth, bool hasPrecision, UInt32 precision) {

    constexpr char const* printfHexDigitsLower = "0123456789abcdef";

    constexpr char const* printfHexDigitsUpper = "0123456789ABCDEF";

    UInt32 digits = 0;

    for (T n = number; n > 0; n >>= 4) {

        ++digits;
    }

    if (digits == 0) {

        digits = 1;
    }

    bool notZero = number != 0;

    char buf[16];

    char* p = buf;

    if (!(hasPrecision && precision == 0 && !notZero)) {

        if (number == 0) {

            (*p++) = '0';
            
            if (precision > 0) {

                precision--;
            }
        } 
        else {

            UInt8 shiftCount = digits * 4;
            
            while (shiftCount) {

                shiftCount -= 4;
                
                (*p++) = upperCase
                    ? printfHexDigitsUpper[(number >> shiftCount) & 0x0f]
                    : printfHexDigitsLower[(number >> shiftCount) & 0x0f];
                
                if (precision > 0) {

                    precision--;
                }
            }
        }
    }

    size_t numlen = p - buf;

    if (!fieldWidth || fieldWidth < (numlen + hasPrecision * precision + (alternateForm * 2 * notZero))) {

        fieldWidth = numlen + hasPrecision * precision + alternateForm * 2 * notZero;
    }

    if ((zeroPad && !hasPrecision) && (alternateForm && notZero)) {

        putch(bufptr, '0');
        
        putch(bufptr, 'x');
    }

    if (!leftPad) {
        
        for (unsigned i = 0; i < fieldWidth - numlen - hasPrecision * precision - alternateForm * 2 * notZero; ++i) {
        
            putch(bufptr, (zeroPad && !hasPrecision) ? '0' : ' ');
        }
    }

    if (!(zeroPad && !hasPrecision) && (alternateForm && notZero)) {
        
        putch(bufptr, '0');
        
        putch(bufptr, 'x');
    }

    if (hasPrecision) {

        for (UInt32 i = 0; i < precision; ++i) {

            putch(bufptr, '0');
        }
    }

    for (unsigned i = 0; i < numlen; ++i) {

        putch(bufptr, buf[i]);
    }

    if (leftPad) {

        for (unsigned i = 0; i < fieldWidth - numlen - hasPrecision * precision - alternateForm * 2 * notZero; ++i) {

            putch(bufptr, ' ');
        }
    }

    return fieldWidth;
}


template<typename PutChFunc, typename CharType>
ALWAYS_INLINE int printDecimal(PutChFunc putch, CharType*& bufptr, UInt64 number, bool sign, bool alwaysSign, bool leftPad, bool zeroPad, UInt32 fieldWidth, bool hasPrecision, UInt32 precision) {

    UInt64 divisor = 10000000000000000000LLU;

    char ch;
    
    char padding = 1;
    
    char buf[21];
    
    char* p = buf;

    if (!(hasPrecision && precision == 0 && number == 0)) {

        for (;;) {

            ch = '0' + (number / divisor);
            
            number %= divisor;
            
            if (ch != '0') {

                padding = 0;
            }

            if (!padding || divisor == 1) {

                *(p++) = ch;

                if (precision > 0) {

                    precision--;
                }
            }

            if (divisor == 1) {

                break;
            }
            
            divisor /= 10;
        }
    }

    size_t numlen = p - buf;

    if (!fieldWidth || fieldWidth < (numlen + hasPrecision * precision + (sign || alwaysSign))) {

        fieldWidth = numlen + hasPrecision * precision + (sign || alwaysSign);
    }

    if ((zeroPad && !hasPrecision) && (sign || alwaysSign)) {

        putch(bufptr, sign ? '-' : '+');
    }

    if (!leftPad) {

        for (unsigned i = 0; i < fieldWidth - numlen - hasPrecision * precision - (sign || alwaysSign); ++i) {

            putch(bufptr, (zeroPad && !hasPrecision) ? '0' : ' ');
        }
    }

    if (!(zeroPad && !hasPrecision) && (sign || alwaysSign)) {

        putch(bufptr, sign ? '-' : '+');
    }

    if (hasPrecision) {

        for (UInt32 i = 0; i < precision; ++i) {

            putch(bufptr, '0');
        }
    }

    for (unsigned i = 0; i < numlen; ++i) {

        putch(bufptr, buf[i]);
    }

    if (leftPad) {

        for (unsigned i = 0; i < fieldWidth - numlen - hasPrecision * precision - (sign || alwaysSign); ++i) {

            putch(bufptr, ' ');
        }
    }


    return fieldWidth;
}

#ifndef OS

template<typename PutChFunc, typename CharType>
ALWAYS_INLINE int printDouble(PutChFunc putch, CharType*& bufptr, double number, bool alwaysSign, bool leftPad, bool zeroPad, UInt32 fieldWidth, UInt32 precision) {

    int length = 0;

    UInt32 wholeWidth = (fieldWidth >= precision + 1) ? fieldWidth - precision - 1 : 0;

    bool sign = signbit(number);
    
    bool nan = isnan(number);
    
    bool inf = isinf(number);

    if (nan || inf) {

        for (unsigned i = 0; i < fieldWidth - 3 - sign; i++) {

            putch(bufptr, ' ');
            
            length++;
        }

        if (sign) {

            putch(bufptr, '-');
            
            length++;
        }

        if (nan) {
            
            putch(bufptr, 'n');
            
            putch(bufptr, 'a');
            
            putch(bufptr, 'n');
        } 
        else {
            
            putch(bufptr, 'i');
            
            putch(bufptr, 'n');
            
            putch(bufptr, 'f');
        }

        return length + 3;
    }

    if (sign) {

        number = -number;
    }

    length = printDecimal(putch, bufptr, (Int64) number, sign, alwaysSign, leftPad, zeroPad, wholeWidth, false, 1);

    if (precision > 0) {

        putch(bufptr, '.');

        length++;
        
        double fraction = number - (Int64) number;

        for (UInt32 i = 0; i < precision; ++i) {

            fraction = fraction * 10;
        }

        return length + printDecimal(putch, bufptr, (Int64) fraction, false, false, false, true, precision, false, 1);
    }

    return length;
}

#endif

template<typename PutChFunc, typename CharType>
ALWAYS_INLINE int printOctalNumber(PutChFunc putch, CharType*& bufptr, UInt64 number, bool alternateForm, bool leftPad, bool zeroPad, UInt32 fieldWidth, bool hasPrecision, UInt32 precision) {

    UInt32 divisor = 134217728;    
    char ch;
    
    char padding = 1;
    
    char buf[32];
    
    char* p = buf;

    if (alternateForm) {

        (*p++) = '0';
        
        if (precision > 0) {

            precision--;
        }
    }

    if (!(hasPrecision && precision == 0 && number == 0)) {

        for (;;) {

            ch = '0' + (number / divisor);

            number %= divisor;
            
            if (ch != '0') {

                padding = 0;
            }

            if (!padding || divisor == 1) {

                *(p++) = ch;

                if (precision > 0) {

                    precision--;
                }
            }
            
            if (divisor == 1) {

                break;
            }

            divisor /= 8;
        }
    }

    size_t numlen = p - buf;

    if (!fieldWidth || fieldWidth < (numlen + hasPrecision * precision)) {

        fieldWidth = numlen + hasPrecision * precision;
    }

    if (!leftPad) {

        for (unsigned i = 0; i < fieldWidth - numlen - hasPrecision * precision; ++i) {

            putch(bufptr, (zeroPad && !hasPrecision) ? '0' : ' ');
        }
    }

    if (hasPrecision) {

        for (UInt32 i = 0; i < precision; ++i) {

            putch(bufptr, '0');
        }
    }

    for (unsigned i = 0; i < numlen; ++i) {

        putch(bufptr, buf[i]);
    }

    if (leftPad) {

        for (unsigned i = 0; i < fieldWidth - numlen - hasPrecision * precision; ++i) {

            putch(bufptr, ' ');
        }
    }

    return fieldWidth;
}

template<typename PutChFunc, typename T, typename CharType>
ALWAYS_INLINE int printString(PutChFunc putch, CharType*& bufptr, T str, size_t len, bool leftPad, size_t fieldWidth, bool dot, size_t precision, bool hasFraction) {
    
    if (hasFraction) {

        len = min(len, precision);
    }

    if (!dot && (!fieldWidth || fieldWidth < len)) {

        fieldWidth = len;
    }

    if (hasFraction && !fieldWidth) {

        fieldWidth = len;
    }

    size_t padAmount = fieldWidth > len ? fieldWidth - len : 0;

    if (!leftPad) {

        for (size_t i = 0; i < padAmount; ++i) {

            putch(bufptr, ' ');
        }
    }

    for (size_t i = 0; i < min(len, fieldWidth); ++i) {

        putch(bufptr, str[i]);
    }

    if (leftPad) {

        for (size_t i = 0; i < padAmount; ++i) {

            putch(bufptr, ' ');
        }
    }
    
    return fieldWidth;
}

template<typename PutChFunc, typename CharType>
ALWAYS_INLINE int printSignedNumber(PutChFunc putch, CharType*& bufptr, Int64 number, bool alwaysSign, bool leftPad, bool zeroPad, UInt32 fieldWidth, bool hasPrecision, UInt32 precision) {

    // FIXME: `0 - number` overflows if we are trying to negate the smallest possible value.
    
    return printDecimal(putch, bufptr, (number < 0) ? 0 - number : number, number < 0, alwaysSign, leftPad, zeroPad, fieldWidth, hasPrecision, precision);
}

struct ModifierState {
    
    bool leftPad { false };
    
    bool zeroPad { false };
    
    bool dot { false };
    
    unsigned fieldWidth { 0 };
    
    bool hasPrecision { false };
    
    unsigned precision { 6 };
    
    unsigned shortQualifiers { 0 }; // TODO: Unimplemented.
    
    unsigned longQualifiers { 0 };
    
    bool intMaxQualifier { false };      // TODO: Unimplemented.
    
    bool ptrDiffQualifier { false };     // TODO: Unimplemented.
    
    bool longDoubleQualifier { false }; // TODO: Unimplemented.
    
    bool sizeQualifier { false };        // TODO: Unimplemented.
    
    bool alternateForm { 0 };
    
    bool alwaysSign { false };
};

template<typename PutChFunc, typename ArgumentListRefT, template<typename T, typename U = ArgumentListRefT> typename NextArgument, typename CharType = char>
struct PrintfImpl {

    ALWAYS_INLINE PrintfImpl(PutChFunc& putch, CharType*& bufptr, int const& nwritten)
        : m_bufptr(bufptr), m_nwritten(nwritten), m_putch(putch) { }

    ALWAYS_INLINE int format_s(ModifierState const& state, ArgumentListRefT ap) const {

        // FIXME: Narrow characters should be converted to wide characters on the fly and vice versa.
        // https://pubs.opengroup.org/onlinepubs/9699919799/functions/printf.html
        // https://pubs.opengroup.org/onlinepubs/9699919799/functions/wprintf.html

#ifndef OS

        if (state.longQualifiers) {
            
            wchar_t const* sp = NextArgument<wchar_t const*>()(ap);
            
            if (!sp) {

                sp = L"(null)";
            }

            return printString(m_putch, m_bufptr, sp, wcslen(sp), state.leftPad, state.fieldWidth, state.dot, state.precision, state.hasPrecision);
        }
#endif

        char const* sp = NextArgument<char const*>()(ap);

        if (!sp) {

            sp = "(null)";
        }

        return printString(m_putch, m_bufptr, sp, strlen(sp), state.leftPad, state.fieldWidth, state.dot, state.precision, state.hasPrecision);
    }

    ALWAYS_INLINE int format_d(ModifierState const& state, ArgumentListRefT ap) const {

        Int64 number = [&]() -> Int64 {

            if (state.longQualifiers >= 2) {

                return NextArgument<long long int>()(ap);
            }

            if (state.longQualifiers == 1) {

                return NextArgument<long int>()(ap);
            }
            
            return NextArgument<int>()(ap);
        }();

        return printSignedNumber(m_putch, m_bufptr, number, state.alwaysSign, state.leftPad, state.zeroPad, state.fieldWidth, state.hasPrecision, state.precision);
    }

    ALWAYS_INLINE int format_i(ModifierState const& state, ArgumentListRefT ap) const {

        return format_d(state, ap);
    }

    ALWAYS_INLINE int format_u(ModifierState const& state, ArgumentListRefT ap) const {

        UInt64 number = [&]() -> UInt64 {
            
            if (state.longQualifiers >= 2) {

                return NextArgument<unsigned long long int>()(ap);
            }

            if (state.longQualifiers == 1) {

                return NextArgument<unsigned long int>()(ap);
            }
            
            return NextArgument<unsigned int>()(ap);
        }();

        return printDecimal(m_putch, m_bufptr, number, false, false, state.leftPad, state.zeroPad, state.fieldWidth, state.hasPrecision, state.precision);
    }

    ALWAYS_INLINE int format_Q(ModifierState const& state, ArgumentListRefT ap) const {

        return printDecimal(m_putch, m_bufptr, NextArgument<UInt64>()(ap), false, false, state.leftPad, state.zeroPad, state.fieldWidth, state.hasPrecision, state.precision);
    }

    ALWAYS_INLINE int format_q(ModifierState const& state, ArgumentListRefT ap) const {

        return printHex(m_putch, m_bufptr, NextArgument<UInt64>()(ap), false, false, state.leftPad, state.zeroPad, 16, false, 1);
    }

#ifndef OS

    ALWAYS_INLINE int format_g(ModifierState const& state, ArgumentListRefT ap) const {

        return format_f(state, ap);
    }

    ALWAYS_INLINE int format_f(ModifierState const& state, ArgumentListRefT ap) const {

        return printDouble(m_putch, m_bufptr, NextArgument<double>()(ap), state.alwaysSign, state.leftPad, state.zeroPad, state.fieldWidth, state.precision);
    }

#endif

    ALWAYS_INLINE int format_o(ModifierState const& state, ArgumentListRefT ap) const {

        return printOctalNumber(m_putch, m_bufptr, NextArgument<UInt32>()(ap), state.alternateForm, state.leftPad, state.zeroPad, state.fieldWidth, state.hasPrecision, state.precision);
    }

    ALWAYS_INLINE int formatUnsignedHex(ModifierState const& state, ArgumentListRefT ap, bool uppercase) const {

        UInt64 number = [&]() -> UInt64 {

            if (state.longQualifiers >= 2) {

                return NextArgument<unsigned long long int>()(ap);
            }

            if (state.longQualifiers == 1) {

                return NextArgument<unsigned long int>()(ap);
            }

            return NextArgument<unsigned int>()(ap);
        }();

        return printHex(m_putch, m_bufptr, number, uppercase, state.alternateForm, state.leftPad, state.zeroPad, state.fieldWidth, state.hasPrecision, state.precision);
    }

    ALWAYS_INLINE int format_x(ModifierState const& state, ArgumentListRefT ap) const {

        return formatUnsignedHex(state, ap, false);
    }
    
    ALWAYS_INLINE int format_X(ModifierState const& state, ArgumentListRefT ap) const
    {
        return formatUnsignedHex(state, ap, true);
    }

    ALWAYS_INLINE int format_n(ModifierState const&, ArgumentListRefT ap) const {

        *NextArgument<int*>()(ap) = m_nwritten;
        
        return 0;
    }
    
    ALWAYS_INLINE int format_p(ModifierState const&, ArgumentListRefT ap) const {

        return print_hex(m_putch, m_bufptr, NextArgument<FlatPointer>()(ap), false, true, false, true, 8, false, 1);
    }
    
    ALWAYS_INLINE int format_P(ModifierState const&, ArgumentListRefT ap) const {

        return print_hex(m_putch, m_bufptr, NextArgument<FlatPointer>()(ap), true, true, false, true, 8, false, 1);
    }

    ALWAYS_INLINE int formatPercent(ModifierState const&, ArgumentListRefT) const {

        m_putch(m_bufptr, '%');
        
        return 1;
    }

    ALWAYS_INLINE int format_c(ModifierState const& state, ArgumentListRefT ap) const {

        char c = NextArgument<int>()(ap);

        return printString(m_putch, m_bufptr, &c, 1, state.leftPad, state.fieldWidth, state.dot, state.precision, state.hasPrecision);
    }

    ALWAYS_INLINE int formatUnrecognized(CharType formatOp, CharType const* fmt, ModifierState const&, ArgumentListRefT) const {

        debugLine("printf_internal: Unimplemented format specifier {} (fmt: {})", formatOp, fmt);

        return 0;
    }

protected:

    CharType*& m_bufptr;

    int const& m_nwritten;

    PutChFunc& m_putch;
};

template<typename T, typename V>
struct VaArgNextArgument {

    ALWAYS_INLINE T operator()(V ap) const {

        return va_arg(ap, T);
    }
};

#define PRINTF_IMPL_DELEGATE_TO_IMPL(c)    \
    case* #c:                              \
        ret += impl.format_##c(state, ap); \
        break;
    
template<typename PutChFunc, template<typename T, typename U, template<typename X, typename Y> typename V, typename C = char> typename Impl = PrintfImpl, typename ArgumentListT = va_list, template<typename T, typename V = decltype(declval<ArgumentListT&>())> typename NextArgument = VaArgNextArgument, typename CharType = char>
ALWAYS_INLINE int printf_internal(PutChFunc putch, IdentityType<CharType>* buffer, CharType const*& fmt, ArgumentListT ap) {

    int ret = 0;

    CharType* bufptr = buffer;

    Impl<PutChFunc, ArgumentListT&, NextArgument, CharType> impl { putch, bufptr, ret };

    for (CharType const* p = fmt; *p; ++p) {

        ModifierState state;

        if (*p == '%' && *(p + 1)) {

        oneMore:
            
            ++p;

            if (*p == '.') {
                
                state.dot = true;
               
                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (*p == '-') {
            
                state.leftPad = true;

                if (*(p + 1)) {

                    goto oneMore;
                }
            }
            
            if (*p == '+') {

                state.alwaysSign = true;

                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (!state.zeroPad && !state.fieldWidth && !state.dot && *p == '0') {

                state.zeroPad = true;

                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (*p >= '0' && *p <= '9') {

                if (!state.dot) {

                    state.fieldWidth *= 10;

                    state.fieldWidth += *p - '0';

                    if (*(p + 1)) {

                        goto oneMore;
                    }
                } 
                else {

                    if (!state.hasPrecision) {

                        state.hasPrecision = true;

                        state.precision = 0;
                    }

                    state.precision *= 10;
                    
                    state.precision += *p - '0';
                    
                    if (*(p + 1)) {

                        goto oneMore;
                    }
                }
            }

            if (*p == '*') {

                if (state.dot) {
                    
                    state.hasPrecision = true;
                    
                    state.zeroPad = true;
                    
                    state.precision = NextArgument<int>()(ap);
                } 
                else {

                    state.fieldWidth = NextArgument<int>()(ap);
                }

                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (*p == 'h') {

                ++state.shortQualifiers;

                if (*(p + 1)) {

                    goto oneMore;
                }
            }
            
            if (*p == 'l') {

                ++state.longQualifiers;

                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (*p == 'j') {

                state.intMaxQualifier = true;
                
                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (*p == 't') {

                state.ptrDiffQualifier = true;
                
                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (*p == 'L') {

                state.longDoubleQualifier = true;
                
                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (*p == 'z') {

                state.sizeQualifier = true;
                
                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            if (*p == '#') {

                state.alternateForm = true;
                
                if (*(p + 1)) {

                    goto oneMore;
                }
            }

            switch (*p) {

            case '%':
                ret += impl.formatPercent(state, ap);
                break;

                PRINTF_IMPL_DELEGATE_TO_IMPL(P);
                PRINTF_IMPL_DELEGATE_TO_IMPL(Q);
                PRINTF_IMPL_DELEGATE_TO_IMPL(X);
                PRINTF_IMPL_DELEGATE_TO_IMPL(c);
                PRINTF_IMPL_DELEGATE_TO_IMPL(d);
#ifndef OS
                PRINTF_IMPL_DELEGATE_TO_IMPL(f);
                PRINTF_IMPL_DELEGATE_TO_IMPL(g);
#endif
                PRINTF_IMPL_DELEGATE_TO_IMPL(i);
                PRINTF_IMPL_DELEGATE_TO_IMPL(n);
                PRINTF_IMPL_DELEGATE_TO_IMPL(o);
                PRINTF_IMPL_DELEGATE_TO_IMPL(p);
                PRINTF_IMPL_DELEGATE_TO_IMPL(q);
                PRINTF_IMPL_DELEGATE_TO_IMPL(s);
                PRINTF_IMPL_DELEGATE_TO_IMPL(u);
                PRINTF_IMPL_DELEGATE_TO_IMPL(x);

            default:
                ret += impl.formatUnrecognized(*p, fmt, state, ap);
                break;
            }
        } 
        else {

            putch(bufptr, *p);
            
            ++ret;
        }
    }
    /// TODO

    return ret;
}