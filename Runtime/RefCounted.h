
#pragma once

#include "Assertions.h"
#include "Checked.h"
#include "NonCopyable.h"
#include "Platform.h"
#include "std.h"

class RefCountedBase {
    
    MAKE_NONCOPYABLE(RefCountedBase);
    
    MAKE_NONMOVABLE(RefCountedBase);

public:

    using RefCountType = unsigned int;

    using AllowOwnPointer = FalseType;

    
};