/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include "Error.h"
#include "NonNullOwnPointer.h"
#include "RefCounted.h"

#define OWNPTR_SCRUB_BYTE 0xf0

template<typename T>
class [[nodiscard]] OwnPointer {

public:

    OwnPointer() = default;

    OwnPointer(decltype(nullptr))
        : m_ptr(nullptr) { }

    OwnPointer(OwnPointer&& other)
        : m_ptr(other.leakPointer()) { }

    template<typename U>
    OwnPointer(NonNullOwnPointer<U>&& other)
        : m_ptr(other.leakPointer()) { }

    template<typename U>
    OwnPointer(OwnPointer<U>&& other)
        : m_ptr(other.leakPointer()) { }

    ~OwnPointer() {

        clear();

#ifdef SANITIZE_PTRS

        m_ptr = (T*)(explodeByte(OWNPTR_SCRUB_BYTE));

#endif
    }

    OwnPointer(OwnPointer const&) = delete;
    
    template<typename U>
    OwnPointer(OwnPointer<U> const&) = delete;
    
    OwnPointer& operator=(OwnPointer const&) = delete;
    
    template<typename U>
    OwnPointer& operator=(OwnPointer<U> const&) = delete;

    template<typename U>
    OwnPointer(NonNullOwnPointer<U> const&) = delete;
    
    template<typename U>
    OwnPointer& operator=(NonNullOwnPointer<U> const&) = delete;
    
    template<typename U>
    OwnPointer(RefPointer<U> const&) = delete;
    
    template<typename U>
    OwnPointer(NonNullRefPointer<U> const&) = delete;
    
    template<typename U>
    OwnPointer(WeakPointer<U> const&) = delete;
    
    template<typename U>
    OwnPointer& operator=(RefPointer<U> const&) = delete;
    
    template<typename U>
    OwnPointer& operator=(NonNullRefPointer<U> const&) = delete;
    
    template<typename U>
    OwnPointer& operator=(WeakPointer<U> const&) = delete;

    OwnPointer& operator=(OwnPointer&& other) {

        OwnPointer ptr(move(other));
        
        swap(ptr);
        
        return *this;
    }

    template<typename U>
    OwnPointer& operator=(OwnPointer<U>&& other) {

        OwnPointer ptr(move(other));
        
        swap(ptr);
        
        return *this;
    }

    template<typename U>
    OwnPointer& operator=(NonNullOwnPointer<U>&& other) {

        OwnPointer ptr(move(other));
        
        swap(ptr);
        
        VERIFY(m_ptr);
        
        return *this;
    }

    OwnPointer& operator=(T* ptr) = delete;

    OwnPointer& operator=(std::nullptr_t) {

        clear();
        
        return *this;
    }

    void clear() {

        delete m_ptr;
        
        m_ptr = nullptr;
    }

    bool operator!() const { return !m_ptr; }

    [[nodiscard]] T* leakPointer() {
        
        T* leakedPtr = m_ptr;

        m_ptr = nullptr;
        
        return leakedPtr;
    }

    NonNullOwnPointer<T> releaseNonNull() {
        
        VERIFY(m_ptr);
        
        return NonNullOwnPointer<T>(NonNullOwnPointer<T>::Adopt, *leakPointer());
    }

    template<typename U>
    NonNullOwnPointer<U> releaseNonNull() {

        VERIFY(m_ptr);
        
        return NonNullOwnPointer<U>(NonNullOwnPointer<U>::Adopt, static_cast<U&>(*leakPointer()));
    }

    ///

    T* pointer() { return m_ptr; }

    const T* pointer() const { return m_ptr; }


    /// 

    T* operator->() {

        VERIFY(m_ptr);
        
        return m_ptr;
    }

    const T* operator->() const {

        VERIFY(m_ptr);

        return m_ptr;
    }

    T& operator*() {

        VERIFY(m_ptr);

        return *m_ptr;
    }

    const T& operator*() const {

        VERIFY(m_ptr);
        
        return *m_ptr;
    }

    ///

    operator const T*() const { return m_ptr; }

    operator T*() { return m_ptr; }

    ///

    operator bool() { return !!m_ptr; }

    ///

    void swap(OwnPointer& other) {

        ::swap(m_ptr, other.m_ptr);
    }

    template<typename U>
    void swap(OwnPointer<U>& other) {

        ::swap(m_ptr, other.m_ptr);
    }

    ///

    static OwnPointer lift(T* ptr) {

        return OwnPointer { ptr };
    }

protected:

    explicit OwnPointer(T* ptr)
        : m_ptr(ptr) {

        static_assert(
            requires { requires typename T::AllowOwnPointer()(); } || !requires { requires !typename T::AllowOwnPointer()(); declval<T>().ref(); declval<T>().unref(); }, "Use RefPointer<> for RefCounted types");
    }

private:

    T* m_ptr = nullptr;
};

///

template<typename T, typename U>
inline void swap(OwnPointer<T>& a, OwnPointer<U>& b) {

    a.swap(b);
}

template<typename T>
inline OwnPointer<T> adoptOwnIfNonNull(T* object) {

    if (object) {

        return OwnPointer<T>::lift(object);
    }

    return { };
}

template<typename T>
inline ErrorOr<NonNullOwnPointer<T>> adoptNonNullOwnOrErrorNoMem(T* object) {

    auto result = adoptOwnIfNonNull(object);

    if (!result) {

        return Error::fromError(ENOMEM);
    }

    return result.releaseNonNull();
}

template<typename T, class... Args>
requires(IsConstructible<T, Args...>) inline ErrorOr<NonNullOwnPointer<T>> tryMake(Args&&... args) {

    return adoptNonNullOwnOrErrorNoMem(new (nothrow) T(forward<Args>(args)...));
}

// FIXME: Remove once P0960R3 is available in Clang.
template<typename T, class... Args>
inline ErrorOr<NonNullOwnPointer<T>> tryMake(Args&&... args)  {

    return adoptNonNullOwnOrErrorNoMem(new (nothrow) T { forward<Args>(args)... });
}

template<typename T>
struct Traits<OwnPointer<T>> : public GenericTraits<OwnPointer<T>> {

    using PeekType = T*;

    using ConstPeekType = const T*;

    static unsigned hash(OwnPointer<T> const& p) { return hashPointer(p.pointer()); }

    static bool equals(OwnPointer<T> const& a, OwnPointer<T> const& b) { return a.pointer() == b.pointer(); }
};
