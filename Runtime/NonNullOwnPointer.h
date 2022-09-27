
#pragma once

#include "Assertions.h"
#include "Format.h"
#include "RefCounted.h"
#include "std.h"
#include "Traits.h"
#include "Types.h"

#define NONNULLOWNPTR_SCRUB_BYTE 0xf1

template<typename T>
class WeakPointer;

template<typename T>
class [[nodiscard]] NonNullOwnPointer {

public:

    using ElementType = T;

    enum AdoptTag { Adopt };

    NonNullOwnPointer(AdoptTag, T& ptr)
        : m_ptr(&ptr) {

        static_assert(
            requires { requires typename T::AllowOwnPointer()(); } || !requires { requires !typename T::AllowOwnPointer()(); declval<T>().ref(); declval<T>().unref(); },
            "Use NonNullRefPointer<> for RefCounted types");
    }

    NonNullOwnPointer(NonNullOwnPointer&& other)
        : m_ptr(other.leakPointer()) {

        VERIFY(m_ptr);
    }

    template<typename U>
    NonNullOwnPointer(NonNullOwnPointer<U>&& other)
        : m_ptr(other.leakPointer()) {

        VERIFY(m_ptr);
    }

    ~NonNullOwnPointer() {

        clear();

#ifdef SANITIZE_PTRS

        m_ptr = (T*) (explodeByte(NONNULLOWNPTR_SCRUB_BYTE));

#endif
    }

    NonNullOwnPointer(NonNullOwnPointer const&) = delete;

    template<typename U>
    NonNullOwnPointer(NonNullOwnPointer<U> const&) = delete;

    NonNullOwnPointer& operator=(NonNullOwnPointer const&) = delete;

    template<typename U>
    NonNullOwnPointer& operator=(NonNullOwnPointer<U> const&) = delete;

    template<typename U>
    NonNullOwnPointer(RefPointer<U> const&) = delete;

    template<typename U>
    NonNullOwnPointer(NonNullRefPointer<U> const&) = delete;

    template<typename U>
    NonNullOwnPointer(WeakPointer<U> const&) = delete;

    template<typename U>
    NonNullOwnPointer& operator=(RefPointer<U> const&) = delete;

    template<typename U>
    NonNullOwnPointer& operator=(NonNullRefPointer<U> const&) = delete;

    template<typename U>
    NonNullOwnPointer& operator=(WeakPointer<U> const&) = delete;

    NonNullOwnPointer& operator=(NonNullOwnPointer&& other) {

        NonNullOwnPointer ptr(move(other));
        
        swap(ptr);
        
        return *this;
    }

    template<typename U>
    NonNullOwnPointer& operator=(NonNullOwnPointer<U>&& other) {

        NonNullOwnPointer ptr(move(other));
        
        swap(ptr);
        
        return *this;
    }

    [[nodiscard]] T* leakPointer() {

        return exchange(m_ptr, nullptr);
    }

    ALWAYS_INLINE RETURNS_NONNULL T* pointer() {

        VERIFY(m_ptr);
        
        return m_ptr;
    }

    ALWAYS_INLINE RETURNS_NONNULL const T* pointer() const {

        VERIFY(m_ptr);
        
        return m_ptr;
    }

    ALWAYS_INLINE RETURNS_NONNULL T* operator->() { return pointer(); }
    
    ALWAYS_INLINE RETURNS_NONNULL const T* operator->() const { return pointer(); }

    ALWAYS_INLINE T& operator*() { return *pointer(); }
    
    ALWAYS_INLINE const T& operator*() const { return *pointer(); }

    ALWAYS_INLINE RETURNS_NONNULL operator const T*() const { return pointer(); }

    ALWAYS_INLINE RETURNS_NONNULL operator T*() { return pointer(); }

    operator bool() const = delete;

    bool operator!() const = delete;


    ///

    void swap(NonNullOwnPointer& other) {

        ::swap(m_ptr, other.m_ptr);
    }

    template<typename U>
    void swap(NonNullOwnPointer<U>& other) {

        ::swap(m_ptr, other.m_ptr);
    }

    ///

    template<typename U>
    NonNullOwnPointer<U> releaseNonNull() {

        VERIFY(m_ptr);

        return NonNullOwnPointer<U>(NonNullOwnPointer<U>::Adopt, static_cast<U&>(*leakPointer()));
    }

private:

    void clear() {

        if (!m_ptr) {

            return;
        }
        
        delete m_ptr;
        
        m_ptr = nullptr;
    }

    T* m_ptr = nullptr;
};

#if !defined(OS)

template<typename T>
inline NonNullOwnPointer<T> adoptOwn(T& object) {

    return NonNullOwnPointer<T>(NonNullOwnPointer<T>::Adopt, object);
}

template<class T, class... Args>
requires(IsConstructible<T, Args...>) inline NonNullOwnPointer<T> make(Args&&... args) {

    return NonNullOwnPointer<T>(NonNullOwnPointer<T>::Adopt, *new T(forward<Args>(args)...));
}

// FIXME: Remove once P0960R3 is available in Clang.
template<class T, class... Args>
inline NonNullOwnPointer<T> make(Args&&... args) {

    return NonNullOwnPointer<T>(NonNullOwnPointer<T>::Adopt, *new T { forward<Args>(args)... });
}

#endif

template<typename T>
struct Traits<NonNullOwnPointer<T>> : public GenericTraits<NonNullOwnPointer<T>> {
    
    using PeekType = T*;
    
    using ConstPeekType = const T*;
    
    static unsigned hash(NonNullOwnPointer<T> const& p) { return hashPointer((FlatPointer) p.pointer()); }
    
    static bool equals(NonNullOwnPointer<T> const& a, NonNullOwnPointer<T> const& b) { return a.pointer() == b.pointer(); }
};

template<typename T, typename U>
inline void swap(NonNullOwnPointer<T>& a, NonNullOwnPointer<U>& b) {

    a.swap(b);
}

template<typename T>
struct Formatter<NonNullOwnPointer<T>> : Formatter<const T*> {

    ErrorOr<void> format(FormatBuilder& builder, NonNullOwnPointer<T> const& value) {

        return Formatter<const T*>::format(builder, value.pointer());
    }
};