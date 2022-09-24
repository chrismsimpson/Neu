
#pragma once

#define REFPTR_SCRUB_BYTE 0xe0

#include "Assertions.h"
#include "Atomic.h"
#include "Error.h"
#include "Format.h"
#include "NonNullRefPointer.h"
#include "std.h"
#include "Traits.h"
#include "Types.h"

template<typename T>
class OwnPointer;

template<typename T>
class [[nodiscard]] RefPointer {

    template<typename U>
    friend class RefPointer;

    template<typename U>
    friend class WeakPointer;


public:

    enum AdoptTag {
        Adopt
    };

    RefPointer() = default;

    // RefPtr(T const* ptr)
    //     : m_ptr(const_cast<T*>(ptr)) {

    //     refIfNotNull(m_ptr);
    // }


//     ALWAYS_INLINE ~RefPtr() {

//         clear();
// #    ifdef SANITIZE_PTRS
//         m_ptr = reinterpret_cast<T*>(explode_byte(REFPTR_SCRUB_BYTE));
// #    endif
//     }

    ///

    template<typename U>
    RefPointer(OwnPointer<U> const&) = delete;

    template<typename U>
    RefPointer& operator=(OwnPointer<U> const&) = delete;

    ///

    void swap(RefPointer& other) {

        ::swap(m_ptr, other.m_ptr);
    }

    template<typename U>
    void swap(RefPointer<U>& other) requires(IsConvertible<U*, T*>) {

        ::swap(m_ptr, other.m_ptr);
    }

    ///

    ALWAYS_INLINE RefPointer& operator=(RefPointer&& other) {

        RefPointer tmp { move(other) };

        swap(tmp);
        
        return *this;
    }

    template<typename U>
    ALWAYS_INLINE RefPointer& operator=(RefPointer<U>&& other) requires(IsConvertible<U*, T*>) {

        RefPointer tmp { move(other) };
        
        swap(tmp);
        
        return *this;
    }

    template<typename U>
    ALWAYS_INLINE RefPointer& operator=(NonNullRefPointer<U>&& other) requires(IsConvertible<U*, T*>) {

        RefPointer tmp { move(other) };

        swap(tmp);
        
        return *this;
    }

    ALWAYS_INLINE RefPointer& operator=(NonNullRefPointer<T> const& other) {

        RefPointer tmp { other };
        
        swap(tmp);
        
        return *this;
    }

    template<typename U>
    ALWAYS_INLINE RefPointer& operator=(NonNullRefPointer<U> const& other) requires(IsConvertible<U*, T*>) {

        RefPointer tmp { other };

        swap(tmp);
        
        return *this;
    }

    ALWAYS_INLINE RefPointer& operator=(RefPointer const& other) {

        RefPointer tmp { other };
        
        swap(tmp);

        return *this;
    }

    template<typename U>
    ALWAYS_INLINE RefPointer& operator=(RefPointer<U> const& other) requires(IsConvertible<U*, T*>) {

        RefPointer tmp { other };

        swap(tmp);
        
        return *this;
    }

    ALWAYS_INLINE RefPointer& operator=(T const* ptr) {

        RefPointer tmp { ptr };
        
        swap(tmp);
        
        return *this;
    }

    ALWAYS_INLINE RefPointer& operator=(T const& object) {

        RefPointer tmp { object };
        
        swap(tmp);
        
        return *this;
    }

    RefPointer& operator=(std::nullptr_t) {

        clear();
        
        return *this;
    }

    ALWAYS_INLINE bool assignIfNull(RefPointer&& other) {

        if (this == &other) {

            return isNull();
        }

        *this = move(other);
        
        return true;
    }

    template<typename U>
    ALWAYS_INLINE bool assignIfNull(RefPointer<U>&& other) {

        if (this == &other) {

            return isNull();
        }

        *this = move(other);
        
        return true;
    }

    ALWAYS_INLINE void clear() {
        
        unrefIfNotNull(m_ptr);

        m_ptr = nullptr;
    }

    bool operator!() const { return !m_ptr; }

    [[nodiscard]] T* leakRef() {

        return exchange(m_ptr, nullptr);
    }

    NonNullRefPointer<T> releaseNonNull() {

        auto* ptr = leakRef();
        
        VERIFY(ptr);
        
        return NonNullRefPointer<T>(NonNullRefPointer<T>::Adopt, *ptr);
    }

    ALWAYS_INLINE T* pointer() { return asPointer(); }

    ALWAYS_INLINE const T* pointer() const { return asPointer(); }

    ///

    ALWAYS_INLINE T* operator->() {

        return asNonNullPointer();
    }

    ALWAYS_INLINE const T* operator->() const {

        return asNonNullPointer();
    }

    ALWAYS_INLINE T& operator*() {

        return *asNonNullPointer();
    }

    ALWAYS_INLINE const T& operator*() const {

        return *asNonNullPointer();
    }

    ///

    ALWAYS_INLINE operator const T*() const { return asPointer(); }
    
    ALWAYS_INLINE operator T*() { return asPointer(); }

    ///

    ALWAYS_INLINE operator bool() { return !isNull(); }

    ///

    bool operator==(std::nullptr_t) const { return isNull(); }

    bool operator!=(std::nullptr_t) const { return !isNull(); }

    ///

    bool operator==(RefPointer const& other) const { return asPointer() == other.asPointer(); }

    bool operator!=(RefPointer const& other) const { return asPointer() != other.asPointer(); }

    ///

    bool operator==(RefPointer& other) { return asPointer() == other.asPointer(); }

    bool operator!=(RefPointer& other) { return asPointer() != other.asPointer(); }

    ///

    bool operator==(const T* other) const { return asPointer() == other; }

    bool operator!=(const T* other) const { return asPointer() != other; }

    ///

    bool operator==(T* other) { return asPointer() == other; }

    bool operator!=(T* other) { return asPointer() != other; }

    ///

    ALWAYS_INLINE bool isNull() const { return !m_ptr; }

private:

    ALWAYS_INLINE T* asPointer() const {

        return m_ptr;
    }

    ALWAYS_INLINE T* asNonNullPointer() const {

        VERIFY(m_ptr);
        
        return m_ptr;
    }

    T* m_ptr { nullptr };
};