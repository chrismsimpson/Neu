
#pragma once

#include "Badge.h"
#include "RefCounted.h"
#include "RefPointer.h"
#include "Span.h"
#include "Types.h"
#include "kmalloc.h"

enum ShouldChomp {

    NoChomp,
    Chomp
};

size_t allocationSizeForStringImpl(size_t length);

// class StringImpl : public RefCounted<StringImpl> {

// public:

// };