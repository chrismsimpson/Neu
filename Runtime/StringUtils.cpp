

#include "CharacterTypes.h"
// #include "MemMem.h"
#include "Memory.h"
#include "Optional.h"
#include "StringBuilder.h"
#include "StringUtils.h"
#include "StringView.h"
#include "Vector.h"

#ifndef OS
#    include "String.h"
#endif

namespace StringUtils {

    bool matches(StringView str, StringView mask, CaseSensitivity caseSensitivity, Vector<MaskSpan>* matchSpans) {
        
        auto record_span = [&matchSpans](size_t start, size_t length) {

            if (matchSpans) {

                matchSpans->append({ start, length });
            }
        };

        // if (str.isNull() || mask.isNull()) {

        //     return str.isNull() && mask.isNull();
        // }

        // if (mask == "*"sv) {

        //     recordSpan(0, str.length());

        //     return true;
        // }


        return false;
    }
}
