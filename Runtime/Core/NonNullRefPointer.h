/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#define NONNULLREFPTR_SCRUB_BYTE 0xe1

#include <Core/Assertions.h>
#include <Core/Atomic.h>
#include <Core/Format.h>
#include <Core/Traits.h>
#include <Core/Types.h>

template<typename T>
class RefPointer;

template<typename T>
ALWAYS_INLINE void refIfNotNull(T* ptr) {

    if (ptr) {

        ptr->ref();
    }
}

template<typename T>
ALWAYS_INLINE void unrefIfNotNull(T* ptr) {

    if (ptr) {

        ptr->unref();
    }
}

template<typename T>
class [[nodiscard]] NonNullRefPointer {

    template<typename U>
    friend class RefPointer;

    template<typename U>
    friend class NonNullRefPointer;

    template<typename U>
    friend class WeakPointer;

public:

    using ElementType = T;

    enum AdoptTag { Adopt };

    ALWAYS_INLINE NonNullRefPointer(T const& object)
        : m_ptr(const_cast<T*>(&object)) {

        m_ptr->ref();
    }

    template<typename U>
    ALWAYS_INLINE NonNullRefPointer(U const& object) requires(IsConvertible<U*, T*>)
        : m_ptr(const_cast<T*>(static_cast<T const*>(&object))) {

        m_ptr->ref();
    }

    ALWAYS_INLINE NonNullRefPointer(AdoptTag, T& object)
        : m_ptr(&object) { }

    ALWAYS_INLINE NonNullRefPointer(NonNullRefPointer&& other)
        : m_ptr(&other.leakRef()) { }

    template<typename U>
    ALWAYS_INLINE NonNullRefPointer(NonNullRefPointer<U>&& other) requires(IsConvertible<U*, T*>)
        : m_ptr(static_cast<T*>(&other.leakRef())) { }

    ALWAYS_INLINE NonNullRefPointer(NonNullRefPointer const& other)
        : m_ptr(const_cast<T*>(other.pointer())) {

        m_ptr->ref();
    }

    template<typename U>
    ALWAYS_INLINE NonNullRefPointer(NonNullRefPointer<U> const& other) requires(IsConvertible<U*, T*>)
        : m_ptr(const_cast<T*>(static_cast<T const*>(other.pointer()))) {

        m_ptr->ref();
    }

    ALWAYS_INLINE ~NonNullRefPointer() {

        unrefIfNotNull(m_ptr);
        
        m_ptr = nullptr;

#    ifdef SANITIZE_PTRS

        m_ptr = reinterpret_cast<T*>(explode_byte(NONNULLREFPTR_SCRUB_BYTE));

#    endif
    }

    template<typename U>
    NonNullRefPointer(RefPointer<U> const&) = delete;
    
    template<typename U>
    NonNullRefPointer& operator=(RefPointer<U> const&) = delete;
    
    NonNullRefPointer(RefPointer<T> const&) = delete;
    
    NonNullRefPointer& operator=(RefPointer<T> const&) = delete;

    NonNullRefPointer& operator=(NonNullRefPointer const& other) {

        NonNullRefPointer tmp { other };
        
        swap(tmp);
        
        return *this;
    }

    template<typename U>
    NonNullRefPointer& operator=(NonNullRefPointer<U> const& other) requires(IsConvertible<U*, T*>) {

        NonNullRefPointer tmp { other };
        
        swap(tmp);
        
        return *this;
    }

    ALWAYS_INLINE NonNullRefPointer& operator=(NonNullRefPointer&& other) {

        NonNullRefPointer tmp { move(other) };
        
        swap(tmp);
        
        return *this;
    }

    template<typename U>
    NonNullRefPointer& operator=(NonNullRefPointer<U>&& other) requires(IsConvertible<U*, T*>) {

        NonNullRefPointer tmp { move(other) };
        
        swap(tmp);

        return *this;
    }

    NonNullRefPointer& operator=(T const& object) {

        NonNullRefPointer tmp { object };
        
        swap(tmp);
        
        return *this;
    }

    ///

    [[nodiscard]] ALWAYS_INLINE T& leakRef() {

        T* ptr = exchange(m_ptr, nullptr);
        
        VERIFY(ptr);
        
        return *ptr;
    }

    ///

    ALWAYS_INLINE RETURNS_NONNULL T* pointer() {

        return asNonNullPointer();
    }

    ALWAYS_INLINE RETURNS_NONNULL const T* pointer() const {

        return asNonNullPointer();
    }

    ALWAYS_INLINE RETURNS_NONNULL T* operator->() {

        return asNonNullPointer();
    }

    ALWAYS_INLINE RETURNS_NONNULL const T* operator->() const {

        return asNonNullPointer();
    }

    ALWAYS_INLINE T& operator*() {

        return *asNonNullPointer();
    }

    ALWAYS_INLINE const T& operator*() const {

        return *asNonNullPointer();
    }

    ALWAYS_INLINE RETURNS_NONNULL operator T*() {

        return asNonNullPointer();
    }

    ALWAYS_INLINE RETURNS_NONNULL operator const T*() const {

        return asNonNullPointer();
    }

    ALWAYS_INLINE operator T&() {

        return *asNonNullPointer();
    }

    ALWAYS_INLINE operator const T&() const {

        return *asNonNullPointer();
    }

    ///

    operator bool() const = delete;

    bool operator!() const = delete;

    ///

    void swap(NonNullRefPointer& other) {

        ::swap(m_ptr, other.m_ptr);
    }

    template<typename U>
    void swap(NonNullRefPointer<U>& other) requires(IsConvertible<U*, T*>) {

        ::swap(m_ptr, other.m_ptr);
    }

private:

    NonNullRefPointer() = delete;

    ALWAYS_INLINE RETURNS_NONNULL T* asNonNullPointer() const {

        VERIFY(m_ptr);
        
        return m_ptr;
    }

    T* m_ptr { nullptr };
};

///

template<typename T>
inline NonNullRefPointer<T> adoptRef(T& object) {

    return NonNullRefPointer<T>(NonNullRefPointer<T>::Adopt, object);
}

template<typename T>
struct Formatter<NonNullRefPointer<T>> : Formatter<const T*> {

    ErrorOr<void> format(FormatBuilder& builder, NonNullRefPointer<T> const& value) {

        return Formatter<const T*>::format(builder, value.pointer());
    }
};

template<typename T, typename U>
inline void swap(NonNullRefPointer<T>& a, NonNullRefPointer<U>& b) requires(IsConvertible<U*, T*>) {

    a.swap(b);
}

template<typename T, class... Args>
requires(IsConstructible<T, Args...>) inline NonNullRefPointer<T> makeRefCounted(Args&&... args) {

    return NonNullRefPointer<T>(NonNullRefPointer<T>::Adopt, *new T(forward<Args>(args)...));
}

// FIXME: Remove once P0960R3 is available in Clang.
template<typename T, class... Args>
inline NonNullRefPointer<T> makeRefCounted(Args&&... args) {

    return NonNullRefPointer<T>(NonNullRefPointer<T>::Adopt, *new T { forward<Args>(args)... });
}

template<typename T>
struct Traits<NonNullRefPointer<T>> : public GenericTraits<NonNullRefPointer<T>> {

    using PeekType = T*;

    using ConstPeekType = const T*;
    
    static unsigned hash(NonNullRefPointer<T> const& p) { return hashPointer(p.pointer()); }
    
    static bool equals(NonNullRefPointer<T> const& a, NonNullRefPointer<T> const& b) { return a.pointer() == b.pointer(); }
};
