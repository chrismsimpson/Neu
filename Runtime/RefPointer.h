
#pragma once

#define REFPTR_SCRUB_BYTE 0xe0

#include "Assertions.h"
// #include "Atomic.h"
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