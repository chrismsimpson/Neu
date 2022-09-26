
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

// template<typename... Parameters>
// void foo(
//     CheckedFormatString<Parameters...>&& fmtstr,
//     Parameters const&... parameters
// ) {

// }

// void bar() {

//     foo("{}", 1);
// }

