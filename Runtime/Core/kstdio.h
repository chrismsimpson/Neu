/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#ifdef __os__

#    ifdef KERNEL
#        include <Kernel/kstdio.h>
#    else
#        include <Core/Types.h>
#        include <stdarg.h>

extern "C" {

    void debugPutString(char const*, size_t);
    
    int sprintf(char* buf, char const* fmt, ...) __attribute__((format(printf, 2, 3)));

    int snprintf(char* buffer, size_t, char const* fmt, ...) __attribute__((format(printf, 3, 4)));
}

#    endif

#else

#    include <stdio.h>

inline void debugPutString(char const* characters, size_t length) {

    fwrite(characters, 1, length, stderr);
}

#endif

template<size_t N>
inline void debugPutString(char const (&array)[N]) {

    return ::debugPutString(array, N);
}
