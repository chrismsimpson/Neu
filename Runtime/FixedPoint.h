

#pragma once

#include "Concepts.h"
#include "Format.h"
#include "IntegralMath.h"
#include "NumericLimits.h"
#include "Types.h"

#ifndef OS

#    include "Math.h"

#endif


// FIXME: this always uses round to nearest break-tie to even
// FIXME: use the Integral concept to constrain Underlying
template<size_t precision, typename Underlying>
class FixedPoint {

    using This = FixedPoint<precision, Underlying>;
    
    constexpr static Underlying radixMask = (static_cast<Underlying>(1) << precision) - 1;

    template<size_t P, typename U>
    friend class FixedPoint;

public:

    constexpr FixedPoint() = default;
    
    template<Integral I>
    constexpr FixedPoint(I value)
        : m_value(static_cast<Underlying>(value) << precision) { }

    template<FloatingPoint F>
    constexpr FixedPoint(F value)
        : m_value(static_cast<Underlying>(value * (static_cast<Underlying>(1) << precision))) { }

    template<size_t P, typename U>
    explicit constexpr FixedPoint(FixedPoint<P, U> const& other)
        : m_value(other.template castTo<precision, Underlying>().m_value) { }

#ifndef OS

    template<FloatingPoint F>
    explicit ALWAYS_INLINE operator F() const {

        return (F)m_value * pow<F>(0.5, precision);
    }

#endif

    template<Integral I>
    explicit constexpr operator I() const {

        I value = m_value >> precision;

        // fract(m_value) >= .5?
        
        if (m_value & (1u << (precision - 1))) {
            
            // fract(m_value) > .5?
            
            if (m_value & (radixMask >> 2u)) {

                // yes: round "up";
                
                value += (m_value > 0 ? 1 : -1);
            } 
            else {
                
                //  no: round to even;
                
                value += value & 1;
            }
        }

        return value;
    }

    constexpr Underlying raw() const {

        return m_value;
    }

    constexpr Underlying& raw() {

        return m_value;
    }

    constexpr This fract() const {

        return createRaw(m_value & radixMask);
    }

    constexpr This round() const {

        return This { static_cast<Underlying>(*this) };
    }

    constexpr This floor() const {

        return createRaw(m_value & ~radixMask);
    }

    constexpr This ceil() const {

        return createRaw((m_value & ~radixMask)
            + (m_value & radixMask ? 1 << precision : 0));
    }

    constexpr This trunk() const {

        return createRaw((m_value & ~radixMask)
            + ((m_value & radixMask)
                    ? (m_value > 0 ? 0 : (1 << precision))
                    : 0));
    }

    constexpr Underlying lround() const { return static_cast<Underlying>(*this); }
    
    constexpr Underlying lfloor() const { return m_value >> precision; }
    
    constexpr Underlying lceil() const {

        return (m_value >> precision)
            + (m_value & radixMask ? 1 : 0);
    }

    constexpr Underlying ltrunk() const {

        return (m_value >> precision)
            + ((m_value & radixMask)
                    ? m_value > 0 ? 0 : 1
                    : 0);
    }

    // http://www.claysturner.com/dsp/BinaryLogarithm.pdf

    constexpr This log2() const {

        // 0.5
        
        This b = createRaw(1 << (precision - 1));
        This y = 0;
        This x = *this;

        // FIXME: There's no negative infinity.
        
        if (x.raw() <= 0) {

            return createRaw(NumericLimits<Underlying>::min());
        }

        if (x != 1) {

            Int32 shiftAmount = ::log2<Underlying>(x.raw()) - precision;

            if (shiftAmount > 0) {

                x >>= shiftAmount;
            }
            else {

                x <<= -shiftAmount;
            }

            y += shiftAmount;
        }

        for (size_t i = 0; i < precision; ++i) {
            
            x *= x;
            
            if (x >= 2) {
                
                x >>= 1;
                
                y += b;
            }

            b >>= 1;
        }

        return y;
    }

    constexpr bool signbit() const requires(IsSigned<Underlying>) {

        return m_value >> (sizeof(Underlying) * 8 - 1);
    }

    constexpr This operator-() const requires(IsSigned<Underlying>) {

        return createRaw(-m_value);
    }

    constexpr This operator+(This const& other) const {

        return createRaw(m_value + other.m_value);
    }
    
    constexpr This operator-(This const& other) const {

        return createRaw(m_value - other.m_value);
    }
    
    constexpr This operator*(This const& other) const {

        // FIXME: Potential Overflow, although result could be represented accurately
        
        Underlying value = m_value * other.raw();
        
        This ret { };
        
        ret.raw() = value >> precision;
        
        // fract(value) >= .5?
        
        if (value & (1u << (precision - 1))) {
            
            // fract(value) > .5?
            
            if (value & (radixMask >> 2u)) {
                
                // yes: round up;
                
                ret.raw() += (value > 0 ? 1 : -1);
            } 
            else {
                
                //  no: round to even (aka unset last sigificant bit);
                
                ret.raw() += m_value & 1;
            }
        }

        return ret;
    }
    
    constexpr This operator/(This const& other) const {

        // FIXME: Better rounding?
        
        return createRaw((m_value / other.m_value) << (precision));
    }

    template<Integral I>
    constexpr This operator+(I other) const {

        return createRaw(m_value + (other << precision));
    }
    
    template<Integral I>
    constexpr This operator-(I other) const {

        return createRaw(m_value - (other << precision));
    }
    
    template<Integral I>
    constexpr This operator*(I other) const {

        return createRaw(m_value * other);
    }
    
    template<Integral I>
    constexpr This operator/(I other) const {

        return createRaw(m_value / other);
    }

    template<Integral I>
    constexpr This operator>>(I other) const {

        return createRaw(m_value >> other);
    }

    template<Integral I>
    constexpr This operator<<(I other) const {

        return createRaw(m_value << other);
    }






    This& operator+=(This const& other) {

        m_value += other.raw();
        
        return *this;
    }

    This& operator-=(This const& other) {

        m_value -= other.raw();
        
        return *this;
    }

    This& operator*=(This const& other) {

        Underlying value = m_value * other.raw();
        
        m_value = value >> precision;
        
        // fract(value) >= .5?
        
        if (value & (1u << (precision - 1))) {
            
            // fract(value) > .5?
            
            if (value & (radixMask >> 2u)) {
                
                // yes: round up;
                
                m_value += (value > 0 ? 1 : -1);
            } 
            else {

                //  no: round to even (aka unset last sigificant bit);
                
                m_value += m_value & 1;
            }
        }

        return *this;
    }

    This& operator/=(This const& other) {

        // FIXME: See above
        
        m_value /= other.raw();
        
        m_value <<= precision;
        
        return *this;
    }

    template<Integral I>
    This& operator+=(I other) {

        m_value += other << precision;
        
        return *this;
    }

    template<Integral I>
    This& operator-=(I other) {

        m_value -= other << precision;
        
        return *this;
    }

    template<Integral I>
    This& operator*=(I other) {

        m_value *= other;
        
        return *this;
    }

    template<Integral I>
    This& operator/=(I other) {

        m_value /= other;
        
        return *this;
    }
    template<Integral I>
    This& operator>>=(I other)
    {
        m_value >>= other;
        return *this;
    }

    template<Integral I>
    This& operator<<=(I other) {

        m_value <<= other;
        
        return *this;
    }

    bool operator==(This const& other) const { return raw() == other.raw(); }
    
    bool operator!=(This const& other) const { return raw() != other.raw(); }
    
    bool operator>(This const& other) const { return raw() > other.raw(); }
    
    bool operator>=(This const& other) const { return raw() >= other.raw(); }
    
    bool operator<(This const& other) const { return raw() < other.raw(); }
    
    bool operator<=(This const& other) const { return raw() <= other.raw(); }

    // FIXE: There are probably better ways to do these
    template<Integral I>
    bool operator==(I other) const {

        return m_value >> precision == other && !(m_value & radixMask);
    }
    
    template<Integral I>
    bool operator!=(I other) const {
        
        return (m_value >> precision) != other || m_value & radixMask;
    }

    template<Integral I>
    bool operator>(I other) const {
        
        return !(*this <= other);
    }

    template<Integral I>
    bool operator>=(I other) const {

        return !(*this < other);
    }

    template<Integral I>
    bool operator<(I other) const {

        return (m_value >> precision) < other || m_value < (other << precision);
    }

    template<Integral I>
    bool operator<=(I other) const {

        return *this < other || *this == other;
    }

    // Casting from a float should be faster than casting to a float

    template<FloatingPoint F>
    bool operator==(F other) const { return *this == (This)other; }
    
    template<FloatingPoint F>
    bool operator!=(F other) const { return *this != (This)other; }
    
    template<FloatingPoint F>
    bool operator>(F other) const { return *this > (This)other; }
    
    template<FloatingPoint F>
    bool operator>=(F other) const { return *this >= (This)other; }
    
    template<FloatingPoint F>
    bool operator<(F other) const { return *this < (This)other; }
    
    template<FloatingPoint F>
    bool operator<=(F other) const { return *this <= (This)other; }

    template<size_t P, typename U>
    operator FixedPoint<P, U>() const {

        return cast_to<P, U>();
    }

private:

    template<size_t P, typename U>
    constexpr FixedPoint<P, U> castTo() const {

        U rawValue = static_cast<U>(m_value >> precision) << P;

        if constexpr (precision > P) {

            rawValue |= (m_value & radixMask) >> (precision - P);
        }
        else if constexpr (precision < P) {

            rawValue |= static_cast<U>(m_value & radixMask) << (P - precision);
        }
        else {

            rawValue |= m_value & radixMask;
        }

        return FixedPoint<P, U>::createRaw(rawValue);
    }

    static This createRaw(Underlying value) {

        This t { };
        
        t.raw() = value;
        
        return t;
    }

    Underlying m_value;
};