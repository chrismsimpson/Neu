/*
 * Copyright (c) 2021, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

// NOTE: This macro works with any result type that has the expected APIs.
//       It's designed with Result and Error in mind.

#define TRY(...)                                        \
    ({                                                  \
        auto _temporaryResult = (__VA_ARGS__);          \
        if (_temporaryResult.isError()) {               \
            return _temporaryResult.releaseError();     \
        }                                               \
        _temporaryResult.releaseValue();                \
    })

#define MUST(...)                                   \
    ({                                              \
        auto _temporaryResult = (__VA_ARGS__);      \
        VERIFY(!_temporaryResult.isError());        \
        _temporaryResult.releaseValue();            \
    })
