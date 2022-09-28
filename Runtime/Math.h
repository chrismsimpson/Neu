
#pragma once

#include "BuiltinWrappers.h"
#include "Concepts.h"
#include "Detail.h"
#include "Types.h"


#ifdef OS

#    error "Including Math.h from the OS is never correct! Floating point is disabled."

#endif

template<FloatingPoint T>
constexpr T NaN = __builtin_nan("");

template<FloatingPoint T>
constexpr T Pi = 3.141592653589793238462643383279502884L;

template<FloatingPoint T>
constexpr T E = 2.718281828459045235360287471352662498L;

namespace Details {

    template<size_t>
    constexpr size_t productEven();
    
    template<>
    constexpr size_t productEven<2>() { return 2; }
    
    template<size_t value>
    constexpr size_t productEven() { return value * productEven<value - 2>(); }

    template<size_t>
    constexpr size_t productOdd();
    
    template<>
    constexpr size_t productOdd<1>() { return 1; }
    
    template<size_t value>
    constexpr size_t productOdd() { return value * productOdd<value - 2>(); }
}

#define CONSTEXPR_STATE(function, args...)          \
    if (isConstantEvaluated()) {                    \
        if (IsSame<T, long double>)                 \
            return __builtin_##function##l(args);   \
        if (IsSame<T, double>)                      \
            return __builtin_##function(args);      \
        if (IsSame<T, float>)                       \
            return __builtin_##function##f(args);   \
    }

namespace Division {

    template<FloatingPoint T>
    constexpr T fmod(T x, T y) {

        CONSTEXPR_STATE(fmod, x, y);

    #if ARCH(I386) || ARCH(X86_64)
        
        UInt16 fpuStatus;

        do {
            asm(
                "fprem\n"
                "fnstsw %%ax\n"
                : "+t"(x), "=a"(fpuStatus)
                : "u"(y));
        }
        while (fpuStatus & 0x400);
        
        return x;
    #else
        return __builtin_fmod(x, y);
    #endif
    }

    ///

    template<FloatingPoint T>
    constexpr T remainder(T x, T y) {

        CONSTEXPR_STATE(remainder, x, y);

    #if ARCH(I386) || ARCH(X86_64)

        u16 fpuStatus;

        do {
            asm(
                "fprem1\n"
                "fnstsw %%ax\n"
                : "+t"(x), "=a"(fpuStatus)
                : "u"(y));
        } 
        while (fpuStatus & 0x400);

        return x;

    #else

        return __builtin_fmod(x, y);

    #endif
    }
}

using Division::fmod;
using Division::remainder;

template<FloatingPoint T>
constexpr T sqrt(T x) {

    CONSTEXPR_STATE(sqrt, x);

#if ARCH(I386) || ARCH(X86_64)

    T res;

    asm("fsqrt"
        : "=t"(res)
        : "0"(x));

    return res;

#else

    return __builtin_sqrt(x);

#endif
}


template<FloatingPoint T>
constexpr T rsqrt(T x) {

    return (T)1. / sqrt(x);
}

#ifdef __SSE__

template<>
constexpr float sqrt(float x) {

    if (isConstantEvaluated()) {

        return __builtin_sqrtf(x);
    }

    float res;

    asm("sqrtss %1, %0"
        : "=x"(res)
        : "x"(x));

    return res;
}

template<>
constexpr float rsqrt(float x) {

    if (isConstantEvaluated()) {

        return 1.f / __builtin_sqrtf(x);
    }

    float res;
    
    asm("rsqrtss %1, %0"
        : "=x"(res)
        : "x"(x));
    
    return res;
}

#endif


template<FloatingPoint T>
constexpr T cbrt(T x) {

    CONSTEXPR_STATE(cbrt, x);

    if (__builtin_isinf(x) || x == 0) {

        return x;
    }

    if (x < 0) {

        return -cbrt(-x);
    }

    T r = x;
    
    T ex = 0;

    while (r < 0.125l) {
        
        r *= 8;
        
        ex--;
    }
    
    while (r > 1.0l) {
        
        r *= 0.125l;
        
        ex++;
    }

    r = (-0.46946116l * r + 1.072302l) * r + 0.3812513l;

    while (ex < 0) {
        
        r *= 0.5l;
        
        ex++;
    }
    
    while (ex > 0) {
    
        r *= 2.0l;
    
        ex--;
    }

    r = (2.0l / 3.0l) * r + (1.0l / 3.0l) * x / (r * r);
    r = (2.0l / 3.0l) * r + (1.0l / 3.0l) * x / (r * r);
    r = (2.0l / 3.0l) * r + (1.0l / 3.0l) * x / (r * r);
    r = (2.0l / 3.0l) * r + (1.0l / 3.0l) * x / (r * r);

    return r;
}

template<FloatingPoint T>
constexpr T fabs(T x) {

    if (isConstantEvaluated()) {

        return x < 0 ? -x : x;
    }

#if ARCH(I386) || ARCH(X86_64)

    asm(
        "fabs"
        : "+t"(x));

    return x;

#else

    return __builtin_fabs(x);

#endif
}

namespace Trigonometry {

    template<FloatingPoint T>
    constexpr T hypot(T x, T y) {

        return sqrt(x * x + y * y);
    }


    template<FloatingPoint T>
    constexpr T sin(T angle) {
        
        CONSTEXPR_STATE(sin, angle);

    #if ARCH(I386) || ARCH(X86_64)

        T ret;

        asm(
            "fsin"
            : "=t"(ret)
            : "0"(angle));

        return ret;

    #else

        return __builtin_sin(angle);

    #endif
    }

    template<FloatingPoint T>
    constexpr T cos(T angle) {

        CONSTEXPR_STATE(cos, angle);

    #if ARCH(I386) || ARCH(X86_64)

        T ret;
    
        asm(
            "fcos"
            : "=t"(ret)
            : "0"(angle));

        return ret;

    #else

        return __builtin_cos(angle);

    #endif
    }

    template<FloatingPoint T>
    constexpr void sincos(T angle, T& sinVal, T& cosVal) {

        if (isConstantEvaluated()) {

            sinVal = sin(angle);
            
            cosVal = cos(angle);

            return;
        }

    #if ARCH(I386) || ARCH(X86_64)
        
        asm(
            "fsincos"
            : "=t"(cosVal), "=u"(sinVal)
            : "0"(angle));

    #else

        __builtin_sincosf(angle, sinVal, cosVal);

    #endif
    }

    template<FloatingPoint T>
    constexpr T tan(T angle) {

        CONSTEXPR_STATE(tan, angle);

    #if ARCH(I386) || ARCH(X86_64)

        double ret, one;

        asm(
            "fptan"
            : "=t"(one), "=u"(ret)
            : "0"(angle));

        return ret;

    #else

        return __builtin_tan(angle);

    #endif
    }

    template<FloatingPoint T>
    constexpr T atan(T value) {

        CONSTEXPR_STATE(atan, value);

    #if ARCH(I386) || ARCH(X86_64)

        T ret;

        asm(
            "fld1\n"
            "fpatan\n"
            : "=t"(ret)
            : "0"(value));

        return ret;

    #else

        return __builtin_atan(value);

    #endif
    }

    template<FloatingPoint T>
    constexpr T asin(T x) {

        CONSTEXPR_STATE(asin, x);

        if (x > 1 || x < -1) {

            return NaN<T>;
        }

        if (x > (T)0.5 || x < (T)-0.5) {

            return 2 * atan<T>(x / (1 + sqrt<T>(1 - x * x)));
        }

        T squared = x * x;
        
        T value = x;
        
        T i = x * squared;
        
        value += i * Details::productOdd<1>() / Details::productEven<2>() / 3;
        
        i *= squared;
        
        value += i * Details::productOdd<3>() / Details::productEven<4>() / 5;
        
        i *= squared;
        
        value += i * Details::productOdd<5>() / Details::productEven<6>() / 7;
        
        i *= squared;
        
        value += i * Details::productOdd<7>() / Details::productEven<8>() / 9;
        
        i *= squared;
        
        value += i * Details::productOdd<9>() / Details::productEven<10>() / 11;
        
        i *= squared;
        
        value += i * Details::productOdd<11>() / Details::productEven<12>() / 13;
        
        i *= squared;
        
        value += i * Details::productOdd<13>() / Details::productEven<14>() / 15;
        
        i *= squared;
        
        value += i * Details::productOdd<15>() / Details::productEven<16>() / 17;
        
        return value;
    }


    template<FloatingPoint T>
    constexpr T acos(T value) {

        CONSTEXPR_STATE(acos, value);

        // FIXME: I am naive

        return static_cast<T>(0.5) * Pi<T> - asin<T>(value);
    }

    template<FloatingPoint T>
    constexpr T atan2(T y, T x) {

        CONSTEXPR_STATE(atan2, y, x);

    #if ARCH(I386) || ARCH(X86_64)

        T ret;

        asm("fpatan"
            : "=t"(ret)
            : "0"(x), "u"(y)
            : "st(1)");

        return ret;

    #else

        return __builtin_atan2(y, x);

    #endif
    }
}

using Trigonometry::acos;
using Trigonometry::asin;
using Trigonometry::atan;
using Trigonometry::atan2;
using Trigonometry::cos;
using Trigonometry::hypot;
using Trigonometry::sin;
using Trigonometry::sincos;
using Trigonometry::tan;

///

namespace Exponentials {

    template<FloatingPoint T>
    constexpr T log(T x) {

        CONSTEXPR_STATE(log, x);

    #if ARCH(I386) || ARCH(X86_64)

        T ret;

        asm(
            "fldln2\n"
            "fxch %%st(1)\n"
            "fyl2x\n"
            : "=t"(ret)
            : "0"(x));

        return ret;

    #else

        return __builtin_log(x);

    #endif
    }
        
    template<FloatingPoint T>
    constexpr T log2(T x) {

        CONSTEXPR_STATE(log2, x);

    #if ARCH(I386) || ARCH(X86_64)

        T ret;

        asm(
            "fld1\n"
            "fxch %%st(1)\n"
            "fyl2x\n"
            : "=t"(ret)
            : "0"(x));

        return ret;

    #else

        return __builtin_log2(x);

    #endif
    }
   
    template<FloatingPoint T>
    constexpr T log10(T x) {

        CONSTEXPR_STATE(log10, x);

    #if ARCH(I386) || ARCH(X86_64)

        T ret;

        asm(
            "fldlg2\n"
            "fxch %%st(1)\n"
            "fyl2x\n"
            : "=t"(ret)
            : "0"(x));

        return ret;

    #else

        return __builtin_log10(x);

    #endif
    }

    template<FloatingPoint T>
    constexpr T exp(T exponent) {

        CONSTEXPR_STATE(exp, exponent);

    #if ARCH(I386) || ARCH(X86_64)

        T res;

        asm("fldl2e\n"
            "fmulp\n"
            "fld1\n"
            "fld %%st(1)\n"
            "fprem\n"
            "f2xm1\n"
            "faddp\n"
            "fscale\n"
            "fstp %%st(1)"
            : "=t"(res)
            : "0"(exponent));

        return res;

    #else

        return __builtin_exp(exponent);

    #endif
    }

    template<FloatingPoint T>
    constexpr T exp2(T exponent) {

        CONSTEXPR_STATE(exp2, exponent);

    #if ARCH(I386) || ARCH(X86_64)

        T res;

        asm("fld1\n"
            "fld %%st(1)\n"
            "fprem\n"
            "f2xm1\n"
            "faddp\n"
            "fscale\n"
            "fstp %%st(1)"
            : "=t"(res)
            : "0"(exponent));

        return res;

    #else

        return __builtin_exp2(exponent);

    #endif
    }
}

using Exponentials::exp;
using Exponentials::exp2;
using Exponentials::log;
using Exponentials::log10;
using Exponentials::log2;

///

namespace Hyperbolic {

    template<FloatingPoint T>
    constexpr T sinh(T x) {

        T exponentiated = exp<T>(x);

        if (x > 0) {

            return (exponentiated * exponentiated - 1) / 2 / exponentiated;
        }

        return (exponentiated - 1 / exponentiated) / 2;
    }

    template<FloatingPoint T>
    constexpr T cosh(T x) {

        CONSTEXPR_STATE(cosh, x);

        T exponentiated = exp(-x);

        if (x < 0) {

            return (1 + exponentiated * exponentiated) / 2 / exponentiated;
        }

        return (1 / exponentiated + exponentiated) / 2;
    }

    template<FloatingPoint T>
    constexpr T tanh(T x) {

        if (x > 0) {
            
            T exponentiated = exp<T>(2 * x);
            
            return (exponentiated - 1) / (exponentiated + 1);
        }
        
        T plusX = exp<T>(x);
        
        T minusX = 1 / plusX;
        
        return (plusX - minusX) / (plusX + minusX);
    }

    template<FloatingPoint T>
    constexpr T asinh(T x) {

        return log<T>(x + sqrt<T>(x * x + 1));
    }

    template<FloatingPoint T>
    constexpr T acosh(T x) {

        return log<T>(x + sqrt<T>(x * x - 1));
    }

    template<FloatingPoint T>
    constexpr T atanh(T x) {

        return log<T>((1 + x) / (1 - x)) / (T)2.0l;
    }
}

using Hyperbolic::acosh;
using Hyperbolic::asinh;
using Hyperbolic::atanh;
using Hyperbolic::cosh;
using Hyperbolic::sinh;
using Hyperbolic::tanh;

///

template<FloatingPoint T>
constexpr T pow(T x, T y) {

    CONSTEXPR_STATE(pow, x, y);

    // fixme I am naive
    
    if (__builtin_isnan(y)) {

        return y;
    }

    if (y == 0) {
        
        return 1;
    }

    if (x == 0) {

        return 0;
    }

    if (y == 1) {

        return x;
    }

    int yAsInt = (int)y;

    if (y == (T)yAsInt) {

        T result = x;
        
        for (int i = 0; i < fabs<T>(y) - 1; ++i) {

            result *= x;
        }

        if (y < 0) {

            result = 1.0l / result;
        }

        return result;
    }

    return exp2<T>(y * log2<T>(x));
}

#undef CONSTEXPR_STATE