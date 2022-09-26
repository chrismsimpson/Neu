
#pragma once

// NOTE: This macro works with any result type that has the expected APIs.
//       It's designed with Result and Error in mind.

#define TRY(expression)                                 \
    ({                                                  \
        auto _temporaryResult = (expression);          \
        if (_temporaryResult.isError()) {               \
            return _temporaryResult.releaseError();     \
        }                                               \
        _temporaryResult.releaseValue();                \
    })

#define MUST(expression)                        \
    ({                                          \
        auto _temporaryResult = (expression);   \
        VERIFY(!_temporaryResult.isError());    \
        _temporaryResult.releaseValue();        \
    })
