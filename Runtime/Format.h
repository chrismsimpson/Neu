
#pragma once

#include "CheckedFormatString.h"

#include "Array.h"
#include "Forward.h"
#include "StringView.h"

#include <stdio.h>
#include <string.h>

class TypeErasedFormatParams;

class FormatParser;

class FormatBuilder;

template<typename T, typename = void>
struct Formatter {

    using __noFormatterDefined = void;
};

template<typename T, typename = void>
inline constexpr bool HasFormatter = true;

template<typename T>
inline constexpr bool HasFormatter<T, typename Formatter<T>::__noFormatterDefined> = false;

///

constexpr size_t maxFormatArguments = 256;

///

struct TypeErasedParameter {

    enum class Type {
        UInt8,
        UInt16,
        UInt32,
        UInt64,
        Int8,
        Int16,
        Int32,
        Int64,
        Custom
    };
    
    template<size_t size, bool isUnsigned>
    static consteval Type getTypeFromSize()
    {
        if constexpr (isUnsigned) {

            if constexpr (size == 1) {

                return Type::UInt8;
            }
            
            if constexpr (size == 2) {

                return Type::UInt16;
            }
            
            if constexpr (size == 4) {

                return Type::UInt32;
            }

            if constexpr (size == 8) {

                return Type::UInt64;
            }
        } 
        else {

            if constexpr (size == 1) {

                return Type::Int8;
            }

            if constexpr (size == 2) {

                return Type::Int16;
            }

            if constexpr (size == 4) {

                return Type::Int32;
            }

            if constexpr (size == 8) {

                return Type::Int64;
            }
        }

        VERIFY_NOT_REACHED();
    }

    ///

    template<typename T>
    static consteval Type getType() {

        if constexpr (IsIntegral<T>) {
            
            return getTypeFromSize<sizeof(T), IsUnsigned<T>>();
        }
        else {

            return Type::Custom;
        }
    }

    ///

    // template<typename Visitor>
    // constexpr auto visit(Visitor&& visitor) const {

    //     switch (type) {

    //     case TypeErasedParameter::Type::UInt8:
    //         return visitor(*static_cast<UInt8 const*>(value));

    //     case TypeErasedParameter::Type::UInt16:
    //         return visitor(*static_cast<UInt16 const*>(value));

    //     case TypeErasedParameter::Type::UInt32:
    //         return visitor(*static_cast<UInt32 const*>(value));

    //     case TypeErasedParameter::Type::UInt64:
    //         return visitor(*static_cast<UInt64 const*>(value));

    //     case TypeErasedParameter::Type::Int8:
    //         return visitor(*static_cast<Int8 const*>(value));

    //     case TypeErasedParameter::Type::Int16:
    //         return visitor(*static_cast<Int16 const*>(value));

    //     case TypeErasedParameter::Type::Int32:
    //         return visitor(*static_cast<Int32 const*>(value));

    //     case TypeErasedParameter::Type::Int64:
    //         return visitor(*static_cast<Int64 const*>(value));

    //     default:
    //         TODO();
    //     }
    // }





    // FIXME: Getters and setters.

    // void const* value;
    // Type type;
    // ErrorOr<void> (*formatter)(TypeErasedFormatParams&, FormatBuilder&, FormatParser&, void const* value);

};



// void outLine(const char * format, ...) {

//     va_list arglist;

//     va_start(arglist, format);

//     vprintf(format, arglist);

//     fputc('\n', stdout);

//     va_end(arglist);
// }


template<typename... Parameters>
void out(
    FILE* file, 
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) {

}

template<typename... Parameters>
void outLine(
    FILE* file,
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) { 
    
}

// inline 
void outLine(FILE* file) { 
    
    fputc('\n', file);
}

template<typename... Parameters>
void out(
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) { 
    
    out(stdout, move(fmtstr), parameters...);
}

template<typename... Parameters>
void outLine(
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) { 
    
    outLine(stdout, move(fmtstr), parameters...);
}

// inline 
void outLine() { 
    
    outLine(stdout);
}