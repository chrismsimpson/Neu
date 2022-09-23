
#pragma once

#include "Checked.h"

#if defined(KERNEL)
#    include <Kernel/Heap/kmalloc.h>
#else
#    include <new>
#    include <stdlib.h>

#    define kcalloc calloc
#    define kmalloc malloc
#    define kmallocGoodSize malloc_good_size

inline void kfreeSized(void* ptr, size_t) {

    free(ptr);
}

#endif

#ifndef __serenity__
#    include "Types.h"

#    ifndef OS_MACOS
extern "C" {
inline size_t malloc_good_size(size_t size) { return size; }
}
#    else
#        include <malloc/malloc.h>
#    endif
#endif

using std::nothrow;


inline void* kmallocArray(Checked<size_t> a, Checked<size_t> b) {

    auto size = a * b;
    
    VERIFY(!size.hasOverflow());
    
    return kmalloc(size.value());
}

inline void* kmallocArray(Checked<size_t> a, Checked<size_t> b, Checked<size_t> c) {

    auto size = a * b * c;
    
    VERIFY(!size.hasOverflow());
    
    return kmalloc(size.value());
}
