
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
class [[nodiscard]] NonnNullOwnPointer {

public:

    using ElementType = T;

    enum AdoptTag { Adopt };







    // template<typename U>
    // NonnNullOwnPointer& operator=(NonNullRefPointer<U> const&) = delete;

    template<typename U>
    NonnNullOwnPointer& operator=(WeakPointer<U> const&) = delete;

    NonnNullOwnPointer& operator=(NonnNullOwnPointer&& other) {

        NonnNullOwnPointer ptr(move(other));
        
        swap(ptr);
        
        return *this;
    }

    template<typename U>
    NonnNullOwnPointer& operator=(NonnNullOwnPointer<U>&& other) {

        NonnNullOwnPointer ptr(move(other));
        
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

    void swap(NonnNullOwnPointer& other) {

        ::swap(m_ptr, other.m_ptr);
    }

    template<typename U>
    void swap(NonnNullOwnPointer<U>& other) {

        ::swap(m_ptr, other.m_ptr);
    }

    ///

    template<typename U>
    NonnNullOwnPointer<U> releaseNonNull() {

        VERIFY(m_ptr);

        return NonnNullOwnPointer<U>(NonnNullOwnPointer<U>::Adopt, static_cast<U&>(*leakPointer()));
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