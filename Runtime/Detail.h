
#pragma once

namespace Detail {

    template<typename T>
    struct __IdentityType {
        
        using Type = T;
    };

    template<typename T>
    using IdentityType = typename __IdentityType<T>::Type;


}

using Detail::IdentityType;