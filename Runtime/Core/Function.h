/*
 * Copyright (C) 2016 Apple Inc. All rights reserved.
 * Copyright (c) 2021, Gunnar Beutner <gbeutner@serenityos.org>
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY APPLE INC. AND ITS CONTRIBUTORS ``AS IS''
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL APPLE INC. OR ITS CONTRIBUTORS
 * BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
 * THE POSSIBILITY OF SUCH DAMAGE.
 */

#pragma once

#include <Core/Assertions.h>
#include <Core/Atomic.h>
#include <Core/BitCast.h>
#include <Core/NonCopyable.h>
#include <Core/ScopeGuard.h>
#include <Core/std.h>
#include <Core/Types.h>

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






























    Function() = default;

    Function(std::nullptr_t) { }

    ~Function() {

        clear(false);
    }

    template<typename CallableType>
    Function(CallableType&& callable) requires((IsFunctionObject<CallableType> && IsCallableWithArguments<CallableType, In...> && !IsSame<RemoveConstVolatileReference<CallableType>, Function>)) {

        initWithCallable(forward<CallableType>(callable));
    }

    template<typename FunctionType>
    Function(FunctionType f) requires((IsFunctionPointer<FunctionType> && IsCallableWithArguments<RemovePointer<FunctionType>, In...> && !IsSame<RemoveConstVolatileReference<FunctionType>, Function>)) {

        initWithCallable(move(f));
    }

    Function(Function&& other) {

        moveFrom(move(other));
    }

    // Note: Despite this method being const, a mutable lambda _may_ modify its own captures.
    Out operator()(In... in) const {
        
        auto* wrapper = callableWrapper();
        
        VERIFY(wrapper);
        
        ++m_callNestingLevel;
        
        ScopeGuard guard([this] {
        
            if (--m_callNestingLevel == 0 && m_deferredClear) {

                const_cast<Function*>(this)->clear(false);
            }
        });
        
        return wrapper->call(forward<In>(in)...);
    }

    explicit operator bool() const { return !!callableWrapper(); }

    template<typename CallableType>
    Function& operator=(CallableType&& callable) requires((IsFunctionObject<CallableType> && IsCallableWithArguments<CallableType, In...>)) {

        clear();
        
        initWithCallable(forward<CallableType>(callable));
        
        return *this;
    }

    template<typename FunctionType>
    Function& operator=(FunctionType f) requires((IsFunctionPointer<FunctionType> && IsCallableWithArguments<RemovePointer<FunctionType>, In...>)) {

        clear();

        if (f) {

            initWithCallable(move(f));
        }

        return *this;
    }

    Function& operator=(std::nullptr_t) {

        clear();
        
        return *this;
    }

    Function& operator=(Function&& other) {

        if (this != &other) {

            clear();
            
            moveFrom(move(other));
        }

        return *this;
    }
















































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