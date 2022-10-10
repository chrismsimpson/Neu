/*
 * Copyright (c) 2020, the SerenityOS developers.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#include "CharacterTypes.h"
#include "Format.h"
#include "GenericLexer.h"
#include "IntegralMath.h"
#include "StringBuilder.h"
#include "kstdio.h"

#if defined(__os__) && !defined(OS)
#    include <os.h>
#endif

#ifdef OS

#else
#    include <math.h>
#    include <stdio.h>
#    include <string.h>
#endif

class FormatParser : public GenericLexer {

public:

    struct FormatSpecifier {
        
        StringView flags;
        size_t index;
    };

    explicit FormatParser(StringView input);

    StringView consumeLiteral();
    
    bool consumeNumber(size_t& value);
    
    bool consumeSpecifier(FormatSpecifier& specifier);
    
    bool consumeReplacementField(size_t& index);
};

static constexpr size_t useNextIndex = NumericLimits<size_t>::max();

// The worst case is that we have the largest 64-bit value formatted as binary number, this would take
// 65 bytes. Choosing a larger power of two won't hurt and is a bit of mitigation against out-of-bounds accesses.

static constexpr size_t convertUnsignedToString(UInt64 value, LinearArray<UInt8, 128>& buffer, UInt8 base, bool upperCase) {

    VERIFY(base >= 2 && base <= 16);

    constexpr char const* lowercaseLookup = "0123456789abcdef";
    
    constexpr char const* uppercaseLookup = "0123456789ABCDEF";

    if (value == 0) {

        buffer[0] = '0';
        
        return 1;
    }

    size_t used = 0;
    
    while (value > 0) {

        if (upperCase) {

            buffer[used++] = uppercaseLookup[value % base];
        }
        else {

            buffer[used++] = lowercaseLookup[value % base];
        }

        value /= base;
    }

    for (size_t i = 0; i < used / 2; ++i) {

        swap(buffer[i], buffer[used - i - 1]);
    }

    return used;
}


ErrorOr<void> vformatImpl(TypeErasedFormatParams& params, FormatBuilder& builder, FormatParser& parser) {

    auto const literal = parser.consumeLiteral();
    
    TRY(builder.putLiteral(literal));

    FormatParser::FormatSpecifier specifier;
    
    if (!parser.consumeSpecifier(specifier)) {
        
        VERIFY(parser.isEof());
        
        return { };
    }

    if (specifier.index == useNextIndex) {

        specifier.index = params.takeNextIndex();
    }

    auto& parameter = params.parameters().at(specifier.index);

    FormatParser argparser { specifier.flags };
    
    TRY(parameter.formatter(params, builder, argparser, parameter.value));
    
    TRY(vformatImpl(params, builder, parser));
    
    return { };
}

FormatParser::FormatParser(StringView input)
    : GenericLexer(input) { }

StringView FormatParser::consumeLiteral() {

    auto const begin = tell();

    while (!isEof()) {

        if (consumeSpecific("{{"))
            continue;

        if (consumeSpecific("}}"))
            continue;

        if (nextIs(isAnyOf("{}"))) {

            return m_input.substringView(begin, tell() - begin);
        }

        consume();
    }

    return m_input.substringView(begin);
}

bool FormatParser::consumeNumber(size_t& value) {

    value = 0;

    bool consumedAtLeastOne = false;

    while (nextIs(isAsciiDigit)) {

        value *= 10;
        
        value += parseAsciiDigit(consume());
        
        consumedAtLeastOne = true;
    }

    return consumedAtLeastOne;
}

bool FormatParser::consumeSpecifier(FormatSpecifier& specifier) {

    VERIFY(!nextIs('}'));

    if (!consumeSpecific('{'))
        return false;

    if (!consumeNumber(specifier.index)) {

        specifier.index = useNextIndex;
    }

    if (consumeSpecific(':')) {

        auto const begin = tell();

        size_t level = 1;

        while (level > 0) {

            VERIFY(!isEof());

            if (consumeSpecific('{')) {
                
                ++level;
                
                continue;
            }

            if (consumeSpecific('}')) {
                
                --level;
                
                continue;
            }

            consume();
        }

        specifier.flags = m_input.substringView(begin, tell() - begin - 1);
    } 
    else {

        if (!consumeSpecific('}')) {

            VERIFY_NOT_REACHED();
        }

        specifier.flags = "";
    }

    return true;
}

bool FormatParser::consumeReplacementField(size_t& index)
{
    if (!consumeSpecific('{')) {

        return false;
    }

    if (!consumeNumber(index)) {

        index = useNextIndex; 
    }

    if (!consumeSpecific('}')) {

        VERIFY_NOT_REACHED();
    }

    return true;
}

ErrorOr<void> FormatBuilder::putPadding(char fill, size_t amount) {

    for (size_t i = 0; i < amount; ++i) {

        TRY(m_builder.tryAppend(fill));
    }

    return { };
}

ErrorOr<void> FormatBuilder::putLiteral(StringView value) {

    for (size_t i = 0; i < value.length(); ++i) {

        TRY(m_builder.tryAppend(value[i]));

        if (value[i] == '{' || value[i] == '}') {

            ++i;
        }
    }

    return { };
}

ErrorOr<void> FormatBuilder::putString(
    StringView value,
    Align align,
    size_t minWidth,
    size_t maxWidth,
    char fill) {

    auto const usedByString = min(maxWidth, value.length());

    auto const usedByPadding = max(minWidth, usedByString) - usedByString;

    if (usedByString < value.length()) {

        value = value.substringView(0, usedByString);
    }

    if (align == Align::Left || align == Align::Default) {
        
        TRY(m_builder.tryAppend(value));
        
        TRY(putPadding(fill, usedByPadding));
    } 
    else if (align == Align::Center) {
        
        auto const usedByLeftPadding = usedByPadding / 2;
        
        auto const usedByRightPadding = ceilingDivision<size_t, size_t>(usedByPadding, 2);

        TRY(putPadding(fill, usedByLeftPadding));
        
        TRY(m_builder.tryAppend(value));
        
        TRY(putPadding(fill, usedByRightPadding));
    } 
    else if (align == Align::Right) {
        
        TRY(putPadding(fill, usedByPadding));
        
        TRY(m_builder.tryAppend(value));
    }

    return { };
}

ErrorOr<void> FormatBuilder::putUInt64(
    UInt64 value,
    UInt8 base,
    bool prefix,
    bool upperCase,
    bool zeroPad,
    Align align,
    size_t minWidth,
    char fill,
    SignMode signMode,
    bool isNegative) {

    if (align == Align::Default) {

        align = Align::Right;
    }

    LinearArray<UInt8, 128> buffer;

    auto const usedByDigits = convertUnsignedToString(value, buffer, base, upperCase);

    size_t usedByPrefix = 0;

    if (align == Align::Right && zeroPad) {

        // We want String::formatted("{:#08x}", 32) to produce '0x00000020' instead of '0x000020'. This
        // behavior differs from both fmtlib and printf, but is more intuitive.
        
        usedByPrefix = 0;
    } 
    else {

        if (isNegative || signMode != SignMode::OnlyIfNeeded) {

            usedByPrefix += 1;
        }

        if (prefix) {

            if (base == 8) {

                usedByPrefix += 1;
            }
            else if (base == 16) {

                usedByPrefix += 2;
            }
            else if (base == 2) {

                usedByPrefix += 2;
            }
        }
    }

    auto const usedByField = usedByPrefix + usedByDigits;
    
    auto const usedByPadding = max(usedByField, minWidth) - usedByField;

    auto const putPrefix = [&]() -> ErrorOr<void> {

        if (isNegative) {

            TRY(m_builder.tryAppend('-'));
        }
        else if (signMode == SignMode::Always) {

            TRY(m_builder.tryAppend('+'));
        }
        else if (signMode == SignMode::Reserved) {

            TRY(m_builder.tryAppend(' '));
        }

        if (prefix) {

            if (base == 2) {

                if (upperCase) {

                    TRY(m_builder.tryAppend("0B"));
                }
                else {

                    TRY(m_builder.tryAppend("0b"));
                }
            } 
            else if (base == 8) {
                
                TRY(m_builder.tryAppend("0"));
            } 
            else if (base == 16) {

                if (upperCase) {

                    TRY(m_builder.tryAppend("0X"));
                }
                else {

                    TRY(m_builder.tryAppend("0x"));
                }
            }
        }

        return { };
    };

    auto const putDigits = [&]() -> ErrorOr<void> {

        for (size_t i = 0; i < usedByDigits; ++i) {

            TRY(m_builder.tryAppend(buffer[i]));
        }

        return { };
    };

    if (align == Align::Left) {

        auto const usedByRightPadding = usedByPadding;

        TRY(putPrefix());
        
        TRY(putDigits());
        
        TRY(putPadding(fill, usedByRightPadding));
    } 
    else if (align == Align::Center) {
        
        auto const usedByLeftPadding = usedByPadding / 2;
        
        auto const usedByRightPadding = ceilingDivision<size_t, size_t>(usedByPadding, 2);

        TRY(putPadding(fill, usedByLeftPadding));
        
        TRY(putPrefix());
        
        TRY(putDigits());
        
        TRY(putPadding(fill, usedByRightPadding));
    } 
    else if (align == Align::Right) {
        
        auto const usedByLeftPadding = usedByPadding;

        if (zeroPad) {

            TRY(putPrefix());
            
            TRY(putPadding('0', usedByLeftPadding));
            
            TRY(putDigits());
        } 
        else {
            
            TRY(putPadding(fill, usedByLeftPadding));
            
            TRY(putPrefix());
            
            TRY(putDigits());
        }
    }

    return { };
}

ErrorOr<void> FormatBuilder::putInt64(
    Int64 value,
    UInt8 base,
    bool prefix,
    bool upperCase,
    bool zeroPad,
    Align align,
    size_t minWidth,
    char fill,
    SignMode signMode) {

    auto const isNegative = value < 0;
    
    value = isNegative ? -value : value;

    TRY(putUInt64(static_cast<UInt64>(value), base, prefix, upperCase, zeroPad, align, minWidth, fill, signMode, isNegative));
    
    return { };
}

ErrorOr<void> FormatBuilder::putFixedPoint(
    Int64 integerValue,
    UInt64 fractionValue,
    UInt64 fractionOne,
    UInt8 base,
    bool upperCase,
    bool zeroPad,
    Align align,
    size_t minWidth,
    size_t precision,
    char fill,
    SignMode signMode) {

    StringBuilder stringBuilder;
    FormatBuilder formatBuilder { stringBuilder };

    bool isNegative = integerValue < 0;

    if (isNegative) {

        integerValue = -integerValue;
    }

    TRY(formatBuilder.putUInt64(static_cast<UInt64>(integerValue), base, false, upperCase, false, Align::Right, 0, ' ', signMode, isNegative));

    if (precision > 0) {

        // FIXME: This is a terrible approximation but doing it properly would be a lot of work. If someone is up for that, a good
        // place to start would be the following video from CppCon 2019:
        // https://youtu.be/4P_kbF0EbZM (Stephan T. Lavavej “Floating-Point <charconv>: Making Your Code 10x Faster With C++17's Final Boss”)

        UInt64 scale = pow<UInt64>(10, precision);

        auto fraction = (scale * fractionValue) / fractionOne; // TODO: overflows
        
        if (isNegative) {
            
            fraction = scale - fraction;
        }

        while (fraction != 0 && fraction % 10 == 0) {

            fraction /= 10;
        }

        size_t visiblePrecision = 0; {

            auto fractionTmp = fraction;
            
            for (; visiblePrecision < precision; ++visiblePrecision) {

                if (fractionTmp == 0) {

                    break;
                }

                fractionTmp /= 10;
            }
        }

        if (zeroPad || visiblePrecision > 0) {

            TRY(stringBuilder.tryAppend('.'));
        }

        if (visiblePrecision > 0) {

            TRY(formatBuilder.putUInt64(fraction, base, false, upperCase, true, Align::Right, visiblePrecision));
        }

        if (zeroPad && (precision - visiblePrecision) > 0) {

            TRY(formatBuilder.putUInt64(0, base, false, false, true, Align::Right, precision - visiblePrecision));
        }
    }

    TRY(putString(stringBuilder.stringView(), align, minWidth, NumericLimits<size_t>::max(), fill));

    return { };
}

#ifndef KERNEL

ErrorOr<void> FormatBuilder::putF64(
    double value,
    UInt8 base,
    bool upperCase,
    bool zeroPad,
    Align align,
    size_t minWidth,
    size_t precision,
    char fill,
    SignMode signMode) {

    StringBuilder stringBuilder;

    FormatBuilder formatBuilder { stringBuilder };

    if (isnan(value) || isinf(value)) {

        if (value < 0.0) {

            TRY(stringBuilder.tryAppend('-'));
        }
        else if (signMode == SignMode::Always) {

            TRY(stringBuilder.tryAppend('+'));
        }
        else if (signMode == SignMode::Reserved) {

            TRY(stringBuilder.tryAppend(' '));
        }

        if (isnan(value)) {

            TRY(stringBuilder.tryAppend(upperCase ? "NAN"sv : "nan"sv));
        }
        else {

            TRY(stringBuilder.tryAppend(upperCase ? "INF"sv : "inf"sv));
        }
        
        TRY(putString(stringBuilder.stringView(), align, minWidth, NumericLimits<size_t>::max(), fill));
        
        return {};
    }

    bool isNegative = value < 0.0;

    if (isNegative) {

        value = -value;
    }

    TRY(formatBuilder.putUInt64(static_cast<UInt64>(value), base, false, upperCase, false, Align::Right, 0, ' ', signMode, isNegative));

    if (precision > 0) {

        // FIXME: This is a terrible approximation but doing it properly would be a lot of work. If someone is up for that, a good
        // place to start would be the following video from CppCon 2019:
        // https://youtu.be/4P_kbF0EbZM (Stephan T. Lavavej “Floating-Point <charconv>: Making Your Code 10x Faster With C++17's Final Boss”)
        
        value -= static_cast<Int64>(value);

        double epsilon = 0.5;

        for (size_t i = 0; i < precision; ++i) {

            epsilon /= 10.0;
        }

        size_t visiblePrecision = 0;

        for (; visiblePrecision < precision; ++visiblePrecision) {

            if (value - static_cast<Int64>(value) < epsilon) {

                break;
            }
            
            value *= 10.0;
            
            epsilon *= 10.0;
        }

        if (zeroPad || visiblePrecision > 0) {

            TRY(stringBuilder.tryAppend('.'));
        }

        if (visiblePrecision > 0) {

            TRY(formatBuilder.putUInt64(static_cast<UInt64>(value), base, false, upperCase, true, Align::Right, visiblePrecision));
        }

        if (zeroPad && (precision - visiblePrecision) > 0) {

            TRY(formatBuilder.putUInt64(0, base, false, false, true, Align::Right, precision - visiblePrecision));
        }
    }

    TRY(putString(stringBuilder.stringView(), align, minWidth, NumericLimits<size_t>::max(), fill));

    return { };
}

ErrorOr<void> FormatBuilder::putF80(
    long double value,
    UInt8 base,
    bool upperCase,
    Align align,
    size_t minWidth,
    size_t precision,
    char fill,
    SignMode signMode) {

    StringBuilder stringBuilder;
    
    FormatBuilder formatBuilder { stringBuilder };

    if (isnan(value) || isinf(value)) {

        if (value < 0.0l) {

            TRY(stringBuilder.tryAppend('-'));
        }
        else if (signMode == SignMode::Always) {

            TRY(stringBuilder.tryAppend('+'));
        }
        else if (signMode == SignMode::Reserved) {

            TRY(stringBuilder.tryAppend(' '));
        }

        if (isnan(value)) {

            TRY(stringBuilder.tryAppend(upperCase ? "NAN"sv : "nan"sv));
        }
        else {

            TRY(stringBuilder.tryAppend(upperCase ? "INF"sv : "inf"sv));
        }

        TRY(putString(stringBuilder.stringView(), align, minWidth, NumericLimits<size_t>::max(), fill));
        
        return { };
    }

    bool isNegative = value < 0.0l;

    if (isNegative) {

        value = -value;
    }

    TRY(formatBuilder.putUInt64(static_cast<UInt64>(value), base, false, upperCase, false, Align::Right, 0, ' ', signMode, isNegative));

    if (precision > 0) {

        // FIXME: This is a terrible approximation but doing it properly would be a lot of work. If someone is up for that, a good
        // place to start would be the following video from CppCon 2019:
        // https://youtu.be/4P_kbF0EbZM (Stephan T. Lavavej “Floating-Point <charconv>: Making Your Code 10x Faster With C++17's Final Boss”)
        
        value -= static_cast<Int64>(value);

        long double epsilon = 0.5l;
        
        for (size_t i = 0; i < precision; ++i) {

            epsilon /= 10.0l;
        }

        size_t visiblePrecision = 0;

        for (; visiblePrecision < precision; ++visiblePrecision) {

            if (value - static_cast<Int64>(value) < epsilon) {

                break;
            }
            
            value *= 10.0l;
            
            epsilon *= 10.0l;
        }

        if (visiblePrecision > 0) {

            stringBuilder.append('.');
            
            TRY(formatBuilder.putUInt64(static_cast<UInt64>(value), base, false, upperCase, true, Align::Right, visiblePrecision));
        }
    }

    TRY(putString(stringBuilder.stringView(), align, minWidth, NumericLimits<size_t>::max(), fill));

    return { };
}

#endif

ErrorOr<void> FormatBuilder::putHexDump(ReadOnlyBytes bytes, size_t width, char fill) {

    auto putCharView = [&](auto i) -> ErrorOr<void> {

        TRY(putPadding(fill, 4));
        
        for (size_t j = i - width; j < i; ++j) {
        
            auto ch = bytes[j];
        
            TRY(m_builder.tryAppend(ch >= 32 && ch <= 127 ? ch : '.')); // silly hack
        }
        
        return {};
    };

    for (size_t i = 0; i < bytes.size(); ++i) {
    
        if (width > 0) {
    
            if (i % width == 0 && i) {
    
                TRY(putCharView(i));
    
                TRY(putLiteral("\n"sv));
            }
        }
    
        TRY(putUInt64(bytes[i], 16, false, false, true, Align::Right, 2));
    }

    if (width > 0 && bytes.size() && bytes.size() % width == 0) {

        TRY(putCharView(bytes.size()));
    }

    return { };
}

ErrorOr<void> vformat(StringBuilder& builder, StringView fmtstr, TypeErasedFormatParams& params) {

    FormatBuilder fmtbuilder { builder };
    FormatParser parser { fmtstr };

    TRY(vformatImpl(params, fmtbuilder, parser));
    
    return { };
}

void StandardFormatter::parse(TypeErasedFormatParams& params, FormatParser& parser) {

    if (StringView { "<^>" }.contains(parser.peek(1))) {

        VERIFY(!parser.nextIs(isAnyOf("{}")));

        m_fill = parser.consume();
    }

    if (parser.consumeSpecific('<')) {

        m_align = FormatBuilder::Align::Left;
    }
    else if (parser.consumeSpecific('^')) {

        m_align = FormatBuilder::Align::Center;
    }
    else if (parser.consumeSpecific('>')) {

        m_align = FormatBuilder::Align::Right;
    }

    if (parser.consumeSpecific('-')) {

        m_signMode = FormatBuilder::SignMode::OnlyIfNeeded;
    }
    else if (parser.consumeSpecific('+')) {

        m_signMode = FormatBuilder::SignMode::Always;
    }
    else if (parser.consumeSpecific(' ')) {

        m_signMode = FormatBuilder::SignMode::Reserved;
    }

    if (parser.consumeSpecific('#')) {

        m_alternativeForm = true;
    }

    if (parser.consumeSpecific('0')) {

        m_zeroPad = true;
    }

    if (size_t index = 0; parser.consumeReplacementField(index)) {

        if (index == useNextIndex) {

            index = params.takeNextIndex();
        }

        m_width = params.parameters().at(index).toSize();
    } 
    else if (size_t width = 0; parser.consumeNumber(width)) {

        m_width = width;
    }

    if (parser.consumeSpecific('.')) {

        if (size_t index = 0; parser.consumeReplacementField(index)) {

            if (index == useNextIndex) {

                index = params.takeNextIndex();
            }

            m_precision = params.parameters().at(index).toSize();
        } 
        else if (size_t precision = 0; parser.consumeNumber(precision)) {

            m_precision = precision;
        }
    }

    if (parser.consumeSpecific('b')) {

        m_mode = Mode::Binary;
    }
    else if (parser.consumeSpecific('B')) {

        m_mode = Mode::BinaryUppercase;
    }
    else if (parser.consumeSpecific('d')) {

        m_mode = Mode::Decimal;
    }
    else if (parser.consumeSpecific('o')) {

        m_mode = Mode::Octal;
    }
    else if (parser.consumeSpecific('x')) {

        m_mode = Mode::Hexadecimal;
    }
    else if (parser.consumeSpecific('X')) {

        m_mode = Mode::HexadecimalUppercase;
    }
    else if (parser.consumeSpecific('c')) {

        m_mode = Mode::Character;
    }
    else if (parser.consumeSpecific('s')) {

        m_mode = Mode::String;
    }
    else if (parser.consumeSpecific('p')) {

        m_mode = Mode::Pointer;
    }
    else if (parser.consumeSpecific('f')) {

        m_mode = Mode::Float;
    }
    else if (parser.consumeSpecific('a')) {

        m_mode = Mode::Hexfloat;
    }
    else if (parser.consumeSpecific('A')) {

        m_mode = Mode::HexfloatUppercase;
    }
    else if (parser.consumeSpecific("hex-dump")) {

        m_mode = Mode::HexDump;
    }

    if (!parser.isEof()) {

        debugLine("{} did not consume '{}'", __PRETTY_FUNCTION__, parser.remaining());
    }

    VERIFY(parser.isEof());
}

ErrorOr<void> Formatter<StringView>::format(FormatBuilder& builder, StringView value) {

    if (m_signMode != FormatBuilder::SignMode::Default) {

        VERIFY_NOT_REACHED();
    }

    if (m_alternativeForm) {

        VERIFY_NOT_REACHED();
    }

    if (m_zeroPad) {

        VERIFY_NOT_REACHED();
    }

    if (m_mode != Mode::Default && m_mode != Mode::String && m_mode != Mode::Character && m_mode != Mode::HexDump) {

        VERIFY_NOT_REACHED();
    }

    m_width = m_width.valueOr(0);
    
    m_precision = m_precision.valueOr(NumericLimits<size_t>::max());

    if (m_mode == Mode::HexDump) {

        return builder.putHexDump(value.bytes(), m_width.value(), m_fill);
    }

    return builder.putString(value, m_align, m_width.value(), m_precision.value(), m_fill);
}

ErrorOr<void> Formatter<FormatString>::vformat(FormatBuilder& builder, StringView fmtstr, TypeErasedFormatParams& params) {

    StringBuilder stringBuilder;
    
    TRY(::vformat(stringBuilder, fmtstr, params));
    
    TRY(Formatter<StringView>::format(builder, stringBuilder.stringView()));
    
    return { };
}

template<Integral T>
ErrorOr<void> Formatter<T>::format(FormatBuilder& builder, T value) {

    if (m_mode == Mode::Character) {

        // FIXME: We just support ASCII for now, in the future maybe unicode?
        
        VERIFY(value >= 0 && value <= 127);

        m_mode = Mode::String;

        Formatter<StringView> formatter { *this };
        
        return formatter.format(builder, StringView { reinterpret_cast<char const*>(&value), 1 });
    }

    if (m_precision.hasValue()) {

        VERIFY_NOT_REACHED();
    }

    if (m_mode == Mode::Pointer) {

        if (m_signMode != FormatBuilder::SignMode::Default) {

            VERIFY_NOT_REACHED();
        }

        if (m_align != FormatBuilder::Align::Default) {

            VERIFY_NOT_REACHED();
        }

        if (m_alternativeForm) {

            VERIFY_NOT_REACHED();
        }

        if (m_width.hasValue()) {

            VERIFY_NOT_REACHED();
        }

        m_mode = Mode::Hexadecimal;
        
        m_alternativeForm = true;
        
        m_width = 2 * sizeof(void*);
        
        m_zeroPad = true;
    }

    UInt8 base = 0;
    
    bool upperCase = false;
    
    if (m_mode == Mode::Binary) {
    
        base = 2;
    } 
    else if (m_mode == Mode::BinaryUppercase) {
    
        base = 2;
    
        upperCase = true;
    } 
    else if (m_mode == Mode::Octal) {
    
        base = 8;
    } 
    else if (m_mode == Mode::Decimal || m_mode == Mode::Default) {
    
        base = 10;
    } 
    else if (m_mode == Mode::Hexadecimal) {
    
        base = 16;
    } 
    else if (m_mode == Mode::HexadecimalUppercase) {
    
        base = 16;
    
        upperCase = true;
    } 
    else if (m_mode == Mode::HexDump) {
    
        m_width = m_width.valueOr(32);
    
        return builder.putHexDump({ &value, sizeof(value) }, m_width.value(), m_fill);
    } 
    else {
    
        VERIFY_NOT_REACHED();
    }

    m_width = m_width.valueOr(0);
 
    if constexpr (IsSame<MakeUnsigned<T>, T>) {

        return builder.putUInt64(value, base, m_alternativeForm, upperCase, m_zeroPad, m_align, m_width.value(), m_fill, m_signMode);
    }
    else {

        return builder.putInt64(value, base, m_alternativeForm, upperCase, m_zeroPad, m_align, m_width.value(), m_fill, m_signMode);
    }
}

ErrorOr<void> Formatter<char>::format(FormatBuilder& builder, char value) {

    if (m_mode == Mode::Binary || m_mode == Mode::BinaryUppercase || m_mode == Mode::Decimal || m_mode == Mode::Octal || m_mode == Mode::Hexadecimal || m_mode == Mode::HexadecimalUppercase) {

        // Trick: signed char != char. (Sometimes weird features are actually helpful.)
        
        Formatter<signed char> formatter { *this };
        
        return formatter.format(builder, static_cast<signed char>(value));
    } 
    else {
        
        Formatter<StringView> formatter { *this };
        
        return formatter.format(builder, { &value, 1 });
    }
}

ErrorOr<void> Formatter<wchar_t>::format(FormatBuilder& builder, wchar_t value) {

    if (m_mode == Mode::Binary || m_mode == Mode::BinaryUppercase || m_mode == Mode::Decimal || m_mode == Mode::Octal || m_mode == Mode::Hexadecimal || m_mode == Mode::HexadecimalUppercase) {
        
        Formatter<UInt32> formatter { *this };
        
        return formatter.format(builder, static_cast<UInt32>(value));
    } 
    else {
        
        StringBuilder codepoint;
        
        codepoint.appendCodePoint(value);

        Formatter<StringView> formatter { *this };
        
        return formatter.format(builder, codepoint.stringView());
    }
}

ErrorOr<void> Formatter<bool>::format(FormatBuilder& builder, bool value) {

    if (m_mode == Mode::Binary || m_mode == Mode::BinaryUppercase || m_mode == Mode::Decimal || m_mode == Mode::Octal || m_mode == Mode::Hexadecimal || m_mode == Mode::HexadecimalUppercase) {

        Formatter<UInt8> formatter { *this };
        
        return formatter.format(builder, static_cast<UInt8>(value));
    } 
    else if (m_mode == Mode::HexDump) {

        return builder.putHexDump({ &value, sizeof(value) }, m_width.valueOr(32), m_fill);
    } 
    else {

        Formatter<StringView> formatter { *this };

        return formatter.format(builder, value ? "true" : "false");
    }
}

#ifndef OS

ErrorOr<void> Formatter<long double>::format(FormatBuilder& builder, long double value) {

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

    return builder.putF80(value, base, upperCase, m_align, m_width.value(), m_precision.value(), m_fill, m_signMode);
}

ErrorOr<void> Formatter<double>::format(FormatBuilder& builder, double value) {
    
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

    return builder.putF64(value, base, upperCase, m_zeroPad, m_align, m_width.value(), m_precision.value(), m_fill, m_signMode);
}

ErrorOr<void> Formatter<float>::format(FormatBuilder& builder, float value) {

    Formatter<double> formatter { *this };
    
    return formatter.format(builder, value);
}

#endif

#ifndef OS

void vout(FILE* file, StringView fmtstr, TypeErasedFormatParams& params, bool newline) {

    StringBuilder builder;

    MUST(vformat(builder, fmtstr, params));

    if (newline) {

        builder.append('\n');
    }

    auto const string = builder.stringView();
    
    auto const retval = ::fwrite(string.charactersWithoutNullTermination(), 1, string.length(), file);
    
    if (static_cast<size_t>(retval) != string.length()) {

        auto error = ferror(file);
        
        debugLine("vout() failed ({} written out of {}), error was {} ({})", retval, string.length(), error, strerror(error));
    }
}

#endif

static bool isDebugEnabled = true;

void setDebugEnabled(bool value) {

    isDebugEnabled = value;
}

void vDebugLine(StringView fmtstr, TypeErasedFormatParams& params) {

    if (!isDebugEnabled) {

        return;
    }

    StringBuilder builder;

    // TODO: __os__

    MUST(vformat(builder, fmtstr, params));
    
    builder.append('\n');

    auto const string = builder.stringView();

    debugPutString(string.charactersWithoutNullTermination(), string.length());
}

#ifdef OS

    // TODO: 
    //      vDebugMessageLine
    //      v_critical_dmesgln

#endif

template struct Formatter<unsigned char, void>;
template struct Formatter<unsigned short, void>;
template struct Formatter<unsigned int, void>;
template struct Formatter<unsigned long, void>;
template struct Formatter<unsigned long long, void>;
template struct Formatter<short, void>;
template struct Formatter<int, void>;
template struct Formatter<long, void>;
template struct Formatter<long long, void>;
template struct Formatter<signed char, void>;