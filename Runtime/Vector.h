
#pragma once

#include "Assertions.h"
#include "Error.h"
#include "Find.h"
#include "Forward.h"
#include "Iterator.h"
#include "Optional.h"
// #include "ReverseIterator.h"
#include "Span.h"
#include "std.h"
#include "Traits.h"
#include "TypedTransfer.h"
#include "kmalloc.h"
#include <initializer_list>

namespace Detail {

    template<typename StorageType, bool>
    struct CanBePlacedInsideVectorHelper;


    template<typename StorageType>
    struct CanBePlacedInsideVectorHelper<StorageType, true> {
        
        template<typename U>
        static constexpr bool value = requires(U&& u) { StorageType { &u }; };
    };
}