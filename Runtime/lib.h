
#pragma once

#include <stdint.h>

#include <Core/AllOf.h>
#include <Core/AnyOf.h>
#include <Core/Assertions.h>
#include <Core/Atomic.h>
#include <Core/BitCast.h>
#include <Core/CharacterTypes.h>
#include <Core/Checked.h>
#include <Core/CheckedFormatString.h>
#include <Core/Concepts.h>
#include <Core/Debug.h>
#include <Core/Error.h>
#include <Core/Find.h>
#include <Core/Format.h>
#include <Core/Function.h>
#include <Core/GenericLexer.h>
#include <Core/HashFunctions.h>
#include <Core/HashMap.h>
#include <Core/HashTable.h>
#include <Core/IterationDecision.h>
#include <Core/Iterator.h>
#include <Core/LinearArray.h>
#include <Core/Memory.h>
#include <Core/NonCopyable.h>
#include <Core/NonNullRefPointer.h>
#include <Core/NumericLimits.h>
#include <Core/Optional.h>
#include <Core/Platform.h>
#include <Core/RefCounted.h>
#include <Core/RefPointer.h>
#include <Core/ReverseIterator.h>
#include <Core/ScopeGuard.h>
#include <Core/Span.h>
#include <Core/Detail.h>
#include <Core/std.h>
#include <Core/String.h>
#include <Core/StringBuilder.h>
#include <Core/StringHash.h>
#include <Core/StringImpl.h>
#include <Core/StringUtils.h>
#include <Core/StringView.h>
#include <Core/Traits.h>
#include <Core/Try.h>
#include <Core/Tuple.h>
#include <Core/TypeCasts.h>
#include <Core/TypeList.h>
#include <Core/TypedTransfer.h>
#include <Core/Types.h>
#include <Core/UnicodeUtils.h>
#include <Core/Variant.h>
#include <Core/Vector.h>
#include <Core/WeakPointer.h>
#include <Core/Weakable.h>
#include <Core/kmalloc.h>
#include <Core/kstdio.h>

#include <Core/Format.cpp>
#include <Core/GenericLexer.cpp>
#include <Core/String.cpp>
#include <Core/StringBuilder.cpp>
#include <Core/StringImpl.cpp>
#include <Core/StringUtils.cpp>
#include <Core/StringView.cpp>

template<typename T, typename TraitsForT = Traits<T>, bool IsOrdered = false>
class Set;

#include <Builtins/Array.h>
#include <Builtins/Dictionary.h>
#include <Builtins/Set.h>

#include <IO/File.h>

#include <IO/File.cpp>

using Float = float; // 32-bit
using Double = double; // 64-bit

template<typename T>
struct Range {

    using IndexType = T;

    T start { };

    T end { };

    T current { };

    Range(T start, T end)
        : start(start), 
          end(end), 
          current(start) { }

    Optional<T> next() {

        if (current == end) {

            return { };
        }

        return current++;
    }
};

namespace NeuInternal {

#ifdef NEU_CONTINUE_ON_PANIC
constexpr auto continue_on_panic = true;
#else
constexpr auto continue_on_panic = false;
#endif

    using OptionalNone = NullOptional;

    ErrorOr<int> main(Array<String>);

    inline void panic(StringView message) {

        warnLine("Panic: {}", message);

        if (continue_on_panic) {

            return;
        }

        VERIFY_NOT_REACHED();
    }

    template<typename T>
    inline constexpr T checkedAdd(T value, T other) {

        Checked<T> checked = value;
        
        checked += other;
        
        if (checked.hasOverflow()) {

            panic(String::formatted("Overflow in checked addition '{} + {}'", value, other));
        }

        return checked.valueUnchecked();
    }

    template<typename T>
    inline constexpr T checkedSub(T value, T other) {

        Checked<T> checked = value;

        checked -= other;

        if (checked.hasOverflow()) {

            panic(String::formatted("Overflow in checked subtraction '{} - {}'", value, other));
        }

        return checked.valueUnchecked();
    }

    template<typename T>
    inline constexpr T checkedMul(T value, T other) {

        Checked<T> checked = value;
        
        checked *= other;
        
        if (checked.hasOverflow()) {

            panic(String::formatted("Overflow in checked multiplication '{} * {}'", value, other));
        }

        return checked.valueUnchecked();
    }

    template<typename T>
    inline constexpr T checkedDiv(T value, T other) {

        Checked<T> checked = value;
        
        checked /= other;
        
        if (checked.hasOverflow()) {

            if (other == 0) {

                panic(String::formatted("Division by zero in checked division '{} / {}'", value, other));
            }
            else {

                panic(String::formatted("Overflow in checked division '{} / {}'", value, other));
            }
        }

        return checked.valueUnchecked();
    }

    template<typename T>
    inline constexpr T checkedMod(T value, T other) {

        Checked<T> checked = value;

        checked %= other;

        if (checked.hasOverflow()) {

            if (other == 0) {

                panic(String::formatted("Division by zero in checked modulo '{} % {}'", value, other));
            }
            else {

                panic(String::formatted("Overflow in checked modulo '{} % {}'", value, other));
            }
        }

        return checked.valueUnchecked();
    }

    template<typename T>
    inline constexpr T arithmeticShiftRight(T value, size_t steps) {

        if constexpr (IsSigned<T>) {

            if constexpr (sizeof(T) == 1) {

                auto sign = (value & 0x80);
                
                // 8-bit variant
                
                return ((value >> steps) | sign);
            }
            else if constexpr (sizeof(T) == 2) {
                
                auto sign = (value & 0x8000);
                
                // 16-bit variant
                
                return ((value >> steps) | sign);
            }
            else if constexpr (sizeof(T) == 4) {
                
                auto sign = (value & 0x80000000);
                
                // 32-bit variant
                
                return ((value >> steps) | sign);
            }
            else if constexpr (sizeof(T) == 8) {
                
                auto sign = (value & 0x8000000000000000);
                
                // 64-bit variant
                
                return ((value >> steps) | sign);
            }
        } 
        else {

            return (value >> steps);
        }
    }

    template<typename Value>
    struct ExplicitValue {

        ExplicitValue(Value&& v)
            : value(move(v)) { }

        ExplicitValue(Value const& v)
            : value(v) { }

        Value value;
    };

    template<>
    struct ExplicitValue<void> {

        ExplicitValue() { }
    };

    template<typename Value, typename Return>
    struct ExplicitValueOrReturn {

        template<typename U>
        ExplicitValueOrReturn(ExplicitValue<U>&& v)
            : value(ExplicitValue<Value> { move(v.value) }) { }

        ExplicitValueOrReturn(ExplicitValue<void>&&)
            : value(ExplicitValue<void> { }) { }


        template<typename U>
        ExplicitValueOrReturn(U&& v) requires(!IsVoid<Return>)
            : value(Return { forward<U>(v) }) { }

        ExplicitValueOrReturn(void) requires(IsVoid<Return>)
            : value(Empty { }) { }

        bool isReturn() const { 
            
            return value.template has<Conditional<IsVoid<Return>, Empty, Return>>();
        }

        Return releaseReturn() {

            if constexpr (IsVoid<Return>) {

                return;
            }
            else {

                return move(value).template get<Return>();
            }
        }

        Value releaseValue() {

            if constexpr (IsVoid<Value>) {

                return;
            }
            else {

                return move(value).template get<ExplicitValue<Value>>().value;
            }
        }

        Variant<Conditional<IsVoid<Return>, Empty, Return>, ExplicitValue<Value>> value;
    };

#define NEU_RESOLVE_EXPLICIT_VALUE_OR_RETURN(x) ({  \
    auto&& _neu_value = x;                          \
    if (_neu_value.isReturn()) {                    \
        return _neu_value.releaseReturn();          \
    }                                               \
    _neu_value.releaseValue();                      \
})

    template<typename OutputType, typename InputType>
    ALWAYS_INLINE Optional<OutputType> fallibleIntegerCast(InputType input) {

        static_assert(IsIntegral<InputType>);
        
        if (!isWithinRange<OutputType>(input)) {

            return { };
        }

        return static_cast<OutputType>(input);
    }

    template<typename... Ts>
    void compileTimeFail(Ts...) { }

    template<typename OutputType, typename InputType>
    ALWAYS_INLINE constexpr OutputType infallibleIntegerCast(InputType input) {

        if constexpr (IsEnum<InputType>) {

            return infallibleIntegerCast<OutputType>(toUnderlying(input));
        }
        else {

            static_assert(IsIntegral<InputType>);

            if (isConstantEvaluated()) {

                if (!isWithinRange<OutputType>(input)) {

                    compileTimeFail("Integer cast out of range");
                }
            } 
            else {

                VERIFY(isWithinRange<OutputType>(input));
            }

            return static_cast<OutputType>(input);
        }
    }

    template<typename OutputType, typename InputType>
    ALWAYS_INLINE constexpr OutputType asSaturated(InputType input) {

        if constexpr (IsEnum<InputType>) {
        
            return asSaturated<OutputType>(toUnderlying(input));
        }
        else {

            static_assert(IsIntegral<InputType>);

            if (!isWithinRange<OutputType>(input)) {

                if constexpr (IsSigned<InputType>) {

                    if (input < 0) {

                        return NumericLimits<OutputType>::min();
                    }
                }

                return NumericLimits<OutputType>::max();
            }

            return static_cast<OutputType>(input);
        }
    }

    template<typename OutputType, typename InputType>
    ALWAYS_INLINE constexpr OutputType asTruncated(InputType input) {

        if constexpr (IsEnum<InputType>) {
            
            return asTruncated<OutputType>(toUnderlying(input));
        } 
        else {

            static_assert(IsIntegral<InputType>);

            return static_cast<OutputType>(input);
        }
    }




    template<typename T>
    struct _RemoveRefPointer {
        
        using Type = T;
    };

    template<typename T>
    struct _RemoveRefPointer<NonNullRefPointer<T>> {
        
        using Type = T;
    };

    template<typename T>
    using RemoveRefPointer = typename _RemoveRefPointer<RemoveConstVolatileReference<T>>::Type;

    template<typename T>
    ALWAYS_INLINE decltype(auto) derefIfRefPointer(T&& value) {

        if constexpr (IsSpecializationOf<RemoveConstVolatileReference<T>, NonNullRefPointer>) {

            return static_cast<CopyConst<RemoveReference<T>, RemoveRefPointer<T>>&>(*value);
        }
        else {

            return static_cast<Conditional<IsRValueReference<T>, RemoveReference<T>, T>>(value);
        }
    }

}

using NeuInternal::fallibleIntegerCast;
using NeuInternal::infallibleIntegerCast;
using NeuInternal::asSaturated;
using NeuInternal::asTruncated;

int main(int argc, char** argv) {

    Array<String> args;

    for (int i = 0; i < argc; ++i) {

        MUST(args.push(argv[i]));
    }

    auto result = NeuInternal::main(move(args));

    if (result.isError()) {

        warnLine("Runtime error: {}", result.error());

        return 1;
    }

    return result.value();

}

static_assert(sizeof(Float) == 4);
static_assert(sizeof(Double) == 8);