
#pragma once

namespace Detail {



    ///

    template<class T>
    struct __RemoveConst {

        using Type = T;
    };

    template<class T>
    struct __RemoveConst<const T> {
    
        using Type = T;
    };
    
    template<class T>
    using RemoveConst = typename __RemoveConst<T>::Type;

    ///

    template<class T>
    struct __RemoveVolatile {
    
        using Type = T;
    };

    template<class T>
    struct __RemoveVolatile<volatile T> {
    
        using Type = T;
    };

    template<typename T>
    using RemoveVolatile = typename __RemoveVolatile<T>::Type;

    ///

    template<class T>
    using RemoveConstVolatile = RemoveVolatile<RemoveConst<T>>;


    ///

    template<typename T>
    struct __MakeUnsigned {
    
        using Type = void;
    };

    template<>
    struct __MakeUnsigned<signed char> {
    
        using Type = unsigned char;
    };

    template<>
    struct __MakeUnsigned<short> {
    
        using Type = unsigned short;
    };
    
    template<>
    struct __MakeUnsigned<int> {
    
        using Type = unsigned int;
    };

    template<>
    struct __MakeUnsigned<bool> {
    
        using Type = bool;
    };

    template<>
    struct __MakeUnsigned<long> {
    
        using Type = unsigned long;
    };
    
    template<>
    struct __MakeUnsigned<long long> {
    
        using Type = unsigned long long;
    };
    
    template<>
    struct __MakeUnsigned<unsigned char> {
    
        using Type = unsigned char;
    };
    
    template<>
    struct __MakeUnsigned<unsigned short> {
    
        using Type = unsigned short;
    };
    
    template<>
    struct __MakeUnsigned<unsigned int> {
    
        using Type = unsigned int;
    };
    
    template<>
    struct __MakeUnsigned<unsigned long> {
    
        using Type = unsigned long;
    };
    
    template<>
    struct __MakeUnsigned<unsigned long long> {
    
        using Type = unsigned long long;
    };
    
    template<>
    struct __MakeUnsigned<char> {
    
        using Type = unsigned char;
    };

    template<>
    struct __MakeUnsigned<char8_t> {
    
        using Type = char8_t;
    };

    template<>
    struct __MakeUnsigned<char16_t> {
    
        using Type = char16_t;
    };
    
    template<>
    struct __MakeUnsigned<char32_t> {
    
        using Type = char32_t;
    };
    
    template<typename T>
    using MakeUnsigned = typename __MakeUnsigned<T>::Type;

    ///








    // template<typename T>
    // inline constexpr bool IsIntegral = __IsIntegral<MakeUnsigned<RemoveCV<T>>>;




    template<typename T>
    struct __IdentityType {
        
        using Type = T;
    };

    template<typename T>
    using IdentityType = typename __IdentityType<T>::Type;


}

using Detail::IdentityType;