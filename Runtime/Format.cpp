
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