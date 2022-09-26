
#pragma once


#include "Assertions.h"
#include "Atomic.h"
#include "BitCast.h"
#include "NonCopyable.h"
#include "ScopeGuard.h"
#include "std.h"
#include "Types.h"

template<typename>
class Function;

template<typename F>
inline constexpr bool IsFunctionPointer = (IsPointer<F> && IsFunction<RemovePointer<F>>);

// Not a function pointer, and not an lvalue reference.
template<typename F>
inline constexpr bool IsFunctionObject = (!IsFunctionPointer<F> && IsRValueReference<F&&>);


template<typename Out, typename... In>
class Function<Out(In...)> {

    MAKE_NONCOPYABLE(Function);

public:

private:

    class CallableWrapperBase {

    public:
        
        virtual ~CallableWrapperBase() = default;
        
        // Note: This is not const to allow storing mutable lambdas.
        
        virtual Out call(In...) = 0;
        
        virtual void destroy() = 0;
        
        virtual void initAndSwap(UInt8*, size_t) = 0;
    };
    
    template<typename CallableType>
    class CallableWrapper final : public CallableWrapperBase {

        MAKE_NONMOVABLE(CallableWrapper);
        
        MAKE_NONCOPYABLE(CallableWrapper);

    public:
    
        explicit CallableWrapper(CallableType&& callable)
            : m_callable(move(callable)) { }

        Out call(In... in) final override {

            return m_callable(forward<In>(in)...);
        }

        void destroy() final override {

            delete this;
        }

        // NOLINTNEXTLINE(readability-non-const-parameter) False positive; destination is used in a placement new expression
        void initAndSwap(UInt8* destination, size_t size) final override {

            VERIFY(size >= sizeof(CallableWrapper));
            
            new (destination) CallableWrapper { move(m_callable) };
        }

    private:

        CallableType m_callable;
    };


    enum class FunctionKind {

        NullPointer,
        Inline,
        Outline
    };

    CallableWrapperBase* callableWrapper() const {

        switch (m_kind) {

        case FunctionKind::NullPointer:
            return nullptr;
        
        case FunctionKind::Inline:
            return bitCast<CallableWrapperBase*>(&m_storage);
        
        case FunctionKind::Outline:
            return *bitCast<CallableWrapperBase**>(&m_storage);
        
        default:
            VERIFY_NOT_REACHED();
        }
    }

    void clear(bool mayDefer = true) {

        bool calledFromInsideFunction = m_callNestingLevel > 0;
        
        // NOTE: This VERIFY could fail because a Function is destroyed from within itself.
        
        VERIFY(mayDefer || !calledFromInsideFunction);
        
        if (calledFromInsideFunction && mayDefer) {
        
            m_deferredClear = true;
        
            return;
        
        }
        
        m_deferredClear = false;
        
        auto* wrapper = callableWrapper();
        
        if (m_kind == FunctionKind::Inline) {
        
            VERIFY(wrapper);
        
            wrapper->~CallableWrapperBase();
        }
        else if (m_kind == FunctionKind::Outline) {
        
            VERIFY(wrapper);
        
            wrapper->destroy();
        }

        m_kind = FunctionKind::NullPointer;
    }

    template<typename Callable>
    void initWithCallable(Callable&& callable) {

        VERIFY(m_callNestingLevel == 0);
        
        using WrapperType = CallableWrapper<Callable>;
        
        if constexpr (sizeof(WrapperType) > inlineCapacity) {

            *bitCast<CallableWrapperBase**>(&m_storage) = new WrapperType(forward<Callable>(callable));
            
            m_kind = FunctionKind::Outline;
        } 
        else {

            new (m_storage) WrapperType(forward<Callable>(callable));
            
            m_kind = FunctionKind::Inline;
        }
    }

    void moveFrom(Function&& other) {

        VERIFY(m_callNestingLevel == 0 && other.m_callNestingLevel == 0);
        
        auto* otherWrapper = other.callableWrapper();
        
        switch (other.m_kind) {
        
        case FunctionKind::NullPointer:

            break;
        
        case FunctionKind::Inline:
        
            otherWrapper->initAndSwap(m_storage, inlineCapacity);
        
            m_kind = FunctionKind::Inline;
        
            break;
        
        case FunctionKind::Outline:

            *bitCast<CallableWrapperBase**>(&m_storage) = otherWrapper;
        
            m_kind = FunctionKind::Outline;
        
            break;
        
        default:

            VERIFY_NOT_REACHED();
        }

        other.m_kind = FunctionKind::NullPointer;
    }














    FunctionKind m_kind { FunctionKind::NullPointer };
    
    bool m_deferredClear { false };
    
    mutable Atomic<UInt16> m_callNestingLevel { 0 };
    
    // Empirically determined to fit most lambdas and functions.
    
    static constexpr size_t inlineCapacity = 4 * sizeof(void*);
    
    alignas(max(alignof(CallableWrapperBase), alignof(CallableWrapperBase*))) UInt8 m_storage[inlineCapacity];

};