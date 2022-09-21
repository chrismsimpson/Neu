
#pragma once

#include "Forward.h"
// TODO: include iteration decision
#include "std.h"

namespace Concepts {

    template<typename T>
    concept Integral = IsIntegral<T>;
    
}