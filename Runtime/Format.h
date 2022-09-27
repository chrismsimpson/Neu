
#pragma once

#include "CheckedFormatString.h"

#include "AllOf.h"
#include "AnyOf.h"
#include "Array.h"
#include "Error.h"
#include "FixedPoint.h"
#include "Forward.h"
#include "Optional.h"
#include "StringView.h"

#ifndef OS
#    include <stdio.h>
#    include <string.h>
#endif

class TypeErasedFormatParams;

class FormatParser;

class FormatBuilder;

template<typename T, typename = void>
struct Formatter {

    using __noFormatterDefined = void;
};

template<typename T, typename = void>
inline constexpr bool HasFormatter = true;

template<typename T>
inline constexpr bool HasFormatter<T, typename Formatter<T>::__noFormatterDefined> = false;

///

constexpr size_t maxFormatArguments = 256;

///

struct TypeErasedParameter {

    enum class Type {
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        Int8,
        Int16,
        Int32,
        Int64,
        Custom
    };
    
    template<size_t size, bool isUnsigned>
    static consteval Type getTypeFromSize() {

        if constexpr (isUnsigned) {

            if constexpr (size == 1) {

                return Type::UInt8;
            }
            
            if constexpr (size == 2) {

                return Type::UInt16;
            }
            
            if constexpr (size == 4) {

                return Type::UInt32;
            }

            if constexpr (size == 8) {

                return Type::UInt64;
            }
        } 
        else {

            if constexpr (size == 1) {

                return Type::Int8;
            }

            if constexpr (size == 2) {

                return Type::Int16;
            }

            if constexpr (size == 4) {

                return Type::Int32;
            }

            if constexpr (size == 8) {

                return Type::Int64;
            }
        }

        VERIFY_NOT_REACHED();
    }

    ///

    template<typename T>
    static consteval Type getType() {

        if constexpr (IsIntegral<T>) {
            
            return getTypeFromSize<sizeof(T), IsUnsigned<T>>();
        }
        else {

            return Type::Custom;
        }
    }

    ///

    template<typename Visitor>
    constexpr auto visit(Visitor&& visitor) const {

        switch (type) {

        case TypeErasedParameter::Type::UInt8:
            return visitor(*static_cast<UInt8 const*>(value));

        case TypeErasedParameter::Type::UInt16:
            return visitor(*static_cast<UInt16 const*>(value));

        case TypeErasedParameter::Type::UInt32:
            return visitor(*static_cast<UInt32 const*>(value));

        case TypeErasedParameter::Type::UInt64:
            return visitor(*static_cast<UInt64 const*>(value));

        case TypeErasedParameter::Type::Int8:
            return visitor(*static_cast<Int8 const*>(value));

        case TypeErasedParameter::Type::Int16:
            return visitor(*static_cast<Int16 const*>(value));

        case TypeErasedParameter::Type::Int32:
            return visitor(*static_cast<Int32 const*>(value));

        case TypeErasedParameter::Type::Int64:
            return visitor(*static_cast<Int64 const*>(value));

        default:
            TODO();
        }
    }

    constexpr size_t toSize() const {

        return visit([]<typename T>(T value) {

            if constexpr (sizeof(T) > sizeof(size_t)) {

                VERIFY(value < NumericLimits<size_t>::max());
            }
            
            if constexpr (IsSigned<T>) {

                VERIFY(value > 0);
            }

            return static_cast<size_t>(value);
        });
    }

    // FIXME: Getters and setters.

    void const* value;
    
    Type type;
    
    ErrorOr<void> (*formatter)(TypeErasedFormatParams&, FormatBuilder&, FormatParser&, void const* value);
};

///

class FormatBuilder {

public:

    enum class Align {
        
        Default,
        Left,
        Center,
        Right
    };

    enum class SignMode {
        
        OnlyIfNeeded,
        Always,
        Reserved,
        Default = OnlyIfNeeded
    };

    explicit FormatBuilder(StringBuilder& builder)
        : m_builder(builder) { }

    ErrorOr<void> putPadding(char fill, size_t amount);

    ErrorOr<void> putLiteral(StringView value);

    ErrorOr<void> putString(
        StringView value,
        Align align = Align::Left,
        size_t minWidth = 0,
        size_t maxWidth = NumericLimits<size_t>::max(),
        char fill = ' ');

    ErrorOr<void> putUInt64(
        UInt64 value,
        UInt8 base = 10,
        bool prefix = false,
        bool upperCase = false,
        bool zeroPad = false,
        Align align = Align::Right,
        size_t minWidth = 0,
        char fill = ' ',
        SignMode signMode = SignMode::OnlyIfNeeded,
        bool isNegative = false);

    ErrorOr<void> putInt64(
        Int64 value,
        UInt8 base = 10,
        bool prefix = false,
        bool upperCase = false,
        bool zeroPad = false,
        Align align = Align::Right,
        size_t minWidth = 0,
        char fill = ' ',
        SignMode signMode = SignMode::OnlyIfNeeded);

    ErrorOr<void> putFixedPoint(
        Int64 integerValue,
        UInt64 fractionValue,
        UInt64 fractionOne,
        UInt8 base = 10,
        bool upperCase = false,
        bool zeroPad = false,
        Align align = Align::Right,
        size_t minWidth = 0,
        size_t precision = 6,
        char fill = ' ',
        SignMode signMode = SignMode::OnlyIfNeeded);

#ifndef OS

    ErrorOr<void> putF80(
        long double value,
        UInt8 base = 10,
        bool upper_case = false,
        Align align = Align::Right,
        size_t minWidth = 0,
        size_t precision = 6,
        char fill = ' ',
        SignMode signMode = SignMode::OnlyIfNeeded);

    ErrorOr<void> putF64(
        double value,
        UInt8 base = 10,
        bool upperCase = false,
        bool zeroPad = false,
        Align align = Align::Right,
        size_t minWidth = 0,
        size_t precision = 6,
        char fill = ' ',
        SignMode signMode = SignMode::OnlyIfNeeded);

#endif

    ErrorOr<void> putHexDump(
        ReadOnlyBytes,
        size_t width,
        char fill = ' ');

    StringBuilder const& builder() const {

        return m_builder;
    }

    StringBuilder& builder() { return m_builder; }

private:

    StringBuilder& m_builder;
};

///

class TypeErasedFormatParams {

public:

    Span<const TypeErasedParameter> parameters() const { return m_parameters; }

    void setParameters(Span<const TypeErasedParameter> parameters) { m_parameters = parameters; }

    size_t takeNextIndex() { return m_nextIndex++; }

private:

    Span<const TypeErasedParameter> m_parameters;

    size_t m_nextIndex { 0 };
};

///

template<typename T>
ErrorOr<void> __format_value(TypeErasedFormatParams& params, FormatBuilder& builder, FormatParser& parser, void const* value) {

    Formatter<T> formatter;

    formatter.parse(params, parser);
    
    return formatter.format(builder, *static_cast<const T*>(value));
}

///

template<typename... Parameters>
class VariadicFormatParams : public TypeErasedFormatParams {

public:

    static_assert(sizeof...(Parameters) <= maxFormatArguments);

    explicit VariadicFormatParams(Parameters const&... parameters)
        : m_data({ TypeErasedParameter { &parameters, TypeErasedParameter::getType<Parameters>(), __format_value<Parameters> }... }) {

        this->setParameters(m_data);
    }

private:

    Array<TypeErasedParameter, sizeof...(Parameters)> m_data;
};

///

// We use the same format for most types for consistency. This is taken directly from
// std::format. One difference is that we are not counting the width or sign towards the
// total width when calculating zero padding for numbers.
// https://en.cppreference.com/w/cpp/utility/format/formatter#Standard_format_specification

struct StandardFormatter {

    enum class Mode {

        Default,
        Binary,
        BinaryUppercase,
        Decimal,
        Octal,
        Hexadecimal,
        HexadecimalUppercase,
        Character,
        String,
        Pointer,
        Float,
        Hexfloat,
        HexfloatUppercase,
        HexDump
    };

    FormatBuilder::Align m_align = FormatBuilder::Align::Default;
    
    FormatBuilder::SignMode m_signMode = FormatBuilder::SignMode::OnlyIfNeeded;
    
    Mode m_mode = Mode::Default;
    
    bool m_alternativeForm = false;
    
    char m_fill = ' ';
    
    bool m_zeroPad = false;
    
    Optional<size_t> m_width;
    
    Optional<size_t> m_precision;

    void parse(TypeErasedFormatParams&, FormatParser&);
};

///

template<Integral T>
struct Formatter<T> : StandardFormatter {

    Formatter() = default;
    explicit Formatter(StandardFormatter formatter)
        : StandardFormatter(move(formatter)) { }

    ErrorOr<void> format(FormatBuilder&, T);
};

///

template<>
struct Formatter<StringView> : StandardFormatter {

    Formatter() = default;

    explicit Formatter(StandardFormatter formatter)
        : StandardFormatter(move(formatter)) { }

    ErrorOr<void> format(FormatBuilder&, StringView);
};

///

template<typename T, size_t inlineCapacity>
requires(HasFormatter<T>) struct Formatter<Vector<T, inlineCapacity>> : StandardFormatter {

    Formatter() = default;

    explicit Formatter(StandardFormatter formatter)
        : StandardFormatter(move(formatter)) { }

    ErrorOr<void> format(FormatBuilder& builder, Vector<T> value) {

        if (m_mode == Mode::Pointer) {

            Formatter<FlatPointer> formatter { *this };

            TRY(formatter.format(builder, reinterpret_cast<FlatPointer>(value.data())));
            
            return { };
        }

        if (m_signMode != FormatBuilder::SignMode::Default) {

            VERIFY_NOT_REACHED();
        }

        if (m_alternativeForm) {

            VERIFY_NOT_REACHED();
        }

        if (m_zeroPad) {

            VERIFY_NOT_REACHED();
        }

        if (m_mode != Mode::Default) {

            VERIFY_NOT_REACHED();
        }

        if (m_width.hasValue() && m_precision.hasValue()) {

            VERIFY_NOT_REACHED();
        }

        m_width = m_width.valueOr(0);
        
        m_precision = m_precision.valueOr(NumericLimits<size_t>::max());

        Formatter<T> contentFmt;
        
        TRY(builder.putLiteral("[ "sv));
        
        bool first = true;
        
        for (auto& content : value) {
            
            if (!first) {
            
                TRY(builder.putLiteral(", "sv));
            
                contentFmt = Formatter<T> {};
            }
            
            first = false;
            
            TRY(contentFmt.format(builder, content));
        }
        
        TRY(builder.putLiteral(" ]"sv));
        
        return { };
    }
};

///

template<>
struct Formatter<ReadOnlyBytes> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, ReadOnlyBytes value) {

        if (m_mode == Mode::Pointer) {
            
            Formatter<FlatPointer> formatter { *this };

            return formatter.format(builder, reinterpret_cast<FlatPointer>(value.data()));
        }

        if (m_mode == Mode::Default || m_mode == Mode::HexDump) {

            m_mode = Mode::HexDump;
            
            return Formatter<StringView>::format(builder, value);
        }

        return Formatter<StringView>::format(builder, value);
    }
};

///

template<>
struct Formatter<Bytes> : Formatter<ReadOnlyBytes> { };

template<>
struct Formatter<char const*> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, char const* value) {

        if (m_mode == Mode::Pointer) {
            
            Formatter<FlatPointer> formatter { *this };
            
            return formatter.format(builder, reinterpret_cast<FlatPointer>(value));
        }

        return Formatter<StringView>::format(builder, value);
    }
};

template<>
struct Formatter<char*> : Formatter<char const*> { };

template<size_t Size>
struct Formatter<char[Size]> : Formatter<char const*> { };

template<size_t Size>
struct Formatter<unsigned char[Size]> : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, unsigned char const* value) {

        if (m_mode == Mode::Pointer) {

            Formatter<FlatPointer> formatter { *this };

            return formatter.format(builder, reinterpret_cast<FlatPointer>(value));
        }

        return Formatter<StringView>::format(builder, { value, Size });
    }
};

template<>
struct Formatter<String> : Formatter<StringView> { };

template<>
struct Formatter<FlyString> : Formatter<StringView> { };

template<typename T>
struct Formatter<T*> : StandardFormatter {

    ErrorOr<void> format(FormatBuilder& builder, T* value) {

        if (m_mode == Mode::Default) {

            m_mode = Mode::Pointer;
        }

        Formatter<FlatPointer> formatter { *this };

        return formatter.format(builder, reinterpret_cast<FlatPointer>(value));
    }
};

template<>
struct Formatter<char> : StandardFormatter {
    
    ErrorOr<void> format(FormatBuilder&, char);
};

template<>
struct Formatter<wchar_t> : StandardFormatter {
    
    ErrorOr<void> format(FormatBuilder& builder, wchar_t);
};

template<>
struct Formatter<bool> : StandardFormatter {

    ErrorOr<void> format(FormatBuilder&, bool);
};

#ifndef OS

template<>
struct Formatter<float> : StandardFormatter {
    
    ErrorOr<void> format(FormatBuilder&, float value);
};

template<>
struct Formatter<double> : StandardFormatter {

    Formatter() = default;

    explicit Formatter(StandardFormatter formatter)
        : StandardFormatter(formatter) { }

    ErrorOr<void> format(FormatBuilder&, double);
};

template<>
struct Formatter<long double> : StandardFormatter {
    
    Formatter() = default;
    
    explicit Formatter(StandardFormatter formatter)
        : StandardFormatter(formatter) { }

    ErrorOr<void> format(FormatBuilder&, long double value);
};

#endif


template<size_t precision, typename Underlying>
struct Formatter<FixedPoint<precision, Underlying>> : StandardFormatter {

    Formatter() = default;

    explicit Formatter(StandardFormatter formatter)
        : StandardFormatter(formatter) { }

    ErrorOr<void> format(FormatBuilder& builder, FixedPoint<precision, Underlying> value) {

        UInt8 base;
        
        bool upperCase;

        if (m_mode == Mode::Default || m_mode == Mode::Float) {
            
            base = 10;
            
            upperCase = false;
        } 
        else if (m_mode == Mode::Hexfloat) {
            
            base = 16;
            
            upperCase = false;
        } 
        else if (m_mode == Mode::HexfloatUppercase) {
            
            base = 16;
            
            upperCase = true;
        } 
        else {
            
            VERIFY_NOT_REACHED();
        }

        m_width = m_width.valueOr(0);
        
        m_precision = m_precision.valueOr(6);

        Int64 integer = value.ltrunk();
        
        constexpr UInt64 one = static_cast<Underlying>(1) << precision;
        
        UInt64 fractionRaw = value.raw() & (one - 1);
        
        return builder.putFixedPoint(integer, fractionRaw, one, base, upperCase, m_zeroPad, m_align, m_width.value(), m_precision.value(), m_fill, m_signMode);
    }
};

template<>
struct Formatter<std::nullptr_t> : Formatter<FlatPointer> {

    ErrorOr<void> format(FormatBuilder& builder, std::nullptr_t) {

        if (m_mode == Mode::Default) {

            m_mode = Mode::Pointer;
        }

        return Formatter<FlatPointer>::format(builder, 0);
    }
};

ErrorOr<void> vformat(StringBuilder&, StringView fmtstr, TypeErasedFormatParams&);

#ifndef OS

void vout(FILE*, StringView fmtstr, TypeErasedFormatParams&, bool newline = false);

template<typename... Parameters>
void out(FILE* file, CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) {

    VariadicFormatParams variadicFormatParams { parameters... };
    
    vout(file, fmtstr.view(), variadicFormatParams);
}

template<typename... Parameters>
void outLine(FILE* file, CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) {

    VariadicFormatParams variadicFormatParams { parameters... };
    
    vout(file, fmtstr.view(), variadicFormatParams, true);
}

inline void outLine(FILE* file) { fputc('\n', file); }

template<typename... Parameters>
void out(CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) { out(stdout, move(fmtstr), parameters...); }

template<typename... Parameters>
void outLine(CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) { outLine(stdout, move(fmtstr), parameters...); }

inline void outLine() { outLine(stdout); }

template<typename... Parameters>
void warn(CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) {

    out(stderr, move(fmtstr), parameters...);
}

template<typename... Parameters>
void warnLine(CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) { outln(stderr, move(fmtstr), parameters...); }

inline void warnLine() { outLine(stderr); }

#    define warnLineIf(flag, fmt, ...)          \
        do {                                    \
            if constexpr (flag) {               \
                warnLine(fmt, ##__VA_ARGS__);   \
            }                                   \
        } while (0)

#endif

void vDebugLine(StringView fmtstr, TypeErasedFormatParams&);

template<typename... Parameters>
void debugLine(CheckedFormatString<Parameters...>&& fmtstr, Parameters const&... parameters) {

    VariadicFormatParams variadicFormatParams { parameters... };
    
    vDebugLine(fmtstr.view(), variadicFormatParams);
}

inline void debugLine() { debugLine(""); }

void setDebugEnabled(bool);

#ifdef OS

void vDebugMessageLine(StringView fmtstr, TypeErasedFormatParams&);

template<typename... Parameters>
void debugMessageLine(CheckedFormatString<Parameters...>&& fmt, Parameters const&... parameters)
{
    VariadicFormatParams variadicFormatParams { parameters... };

    vDebugMessageLine(fmt.view(), variadicFormatParams);
}

///

void vCriticalDebugMessageLine(StringView fmtstr, TypeErasedFormatParams&);

// be very careful to not cause any allocations here, since we could be in
// a very unstable situation

template<typename... Parameters>
void criticalDebugMessageLine(CheckedFormatString<Parameters...>&& fmt, Parameters const&... parameters)
{
    VariadicFormatParams variadicFormatParams { parameters... };

    vCriticalDebugMessageLine(fmt.view(), variadicFormatParams);
}

#endif

///

template<typename T>
class FormatIfSupported {

public:

    explicit FormatIfSupported(const T& value)
        : m_value(value) { }

    const T& value() const { return m_value; }

private:

    const T& m_value;
};

template<typename T, bool Supported = false>
struct __FormatIfSupported : Formatter<StringView> {

    ErrorOr<void> format(FormatBuilder& builder, FormatIfSupported<T> const&) {

        return Formatter<StringView>::format(builder, "?");
    }
};

template<typename T>
struct __FormatIfSupported<T, true> : Formatter<T> {

    ErrorOr<void> format(FormatBuilder& builder, FormatIfSupported<T> const& value) {

        return Formatter<T>::format(builder, value.value());
    }
};

template<typename T>
struct Formatter<FormatIfSupported<T>> : __FormatIfSupported<T, HasFormatter<T>> { };

// This is a helper class, the idea is that if you want to implement a formatter you can inherit
// from this class to "break down" the formatting.
struct FormatString { };

template<>
struct Formatter<FormatString> : Formatter<StringView> {

    template<typename... Parameters>
    ErrorOr<void> format(FormatBuilder& builder, StringView fmtstr, Parameters const&... parameters) {

        VariadicFormatParams variadicFormatParams { parameters... };
        
        return vformat(builder, fmtstr, variadicFormatParams);
    }

    ErrorOr<void> vformat(FormatBuilder& builder, StringView fmtstr, TypeErasedFormatParams& params);
};

template<>
struct Formatter<Error> : Formatter<FormatString> {

    ErrorOr<void> format(FormatBuilder& builder, Error const& error) {

    #if defined(__os__) && defined(OS)

        if (error.isErrno()) {

            return Formatter<FormatString>::format(builder, "Error(errno={})", error.code());
        }

        return Formatter<FormatString>::format(builder, "Error({})", error.stringLiteral());

    #else

        if (error.isSyscall()) {

            return Formatter<FormatString>::format(builder, "{}: {} (errno={})", error.stringLiteral(), strerror(error.code()), error.code());
        }

        if (error.isErrno()) {

            return Formatter<FormatString>::format(builder, "{} (errno={})", strerror(error.code()), error.code());
        }

        return Formatter<FormatString>::format(builder, "{}", error.stringLiteral());

    #endif
    }
};


template<typename T, typename ErrorType>
struct Formatter<ErrorOr<T, ErrorType>> : Formatter<FormatString> {

    ErrorOr<void> format(FormatBuilder& builder, ErrorOr<T, ErrorType> const& errorOr) {

        if (errorOr.isError()) {

            return Formatter<FormatString>::format(builder, "{}", errorOr.error());
        }

        return Formatter<FormatString>::format(builder, "{{{}}}", errorOr.value());
    }
};

#define debugLineIf(flag, fmt, ...)         \
    do {                                    \
        if constexpr (flag) {               \
            debugLine(fmt, ##__VA_ARGS__);  \
        }                                   \
    } while (0)
