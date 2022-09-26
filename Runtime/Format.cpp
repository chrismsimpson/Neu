
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

static constexpr size_t convertUnsignedToString(UInt64 value, Array<UInt8, 128>& buffer, UInt8 base, bool upperCase) {

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

// ErrorOr<void> FormatBuilder::putPadding(char fill, size_t amount) {

//     for (size_t i = 0; i < amount; ++i) {

//         TRY(m_builder.tryAppend(fill));
//     }

//     return { };
// }