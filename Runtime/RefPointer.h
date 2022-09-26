
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

    RefPointer(T const* ptr)
        : m_ptr(const_cast<T*>(ptr)) {

        refIfNotNull(m_ptr);
    }

    RefPointer(T const& object)
        : m_ptr(const_cast<T*>(&object)) {

        m_ptr->ref();
    }

    RefPointer(AdoptTag, T& object)
        : m_ptr(&object) { }

    RefPointer(RefPointer&& other)
        : m_ptr(other.leakRef()) { }

    ALWAYS_INLINE RefPointer(NonNullRefPointer<T> const& other)
        : m_ptr(const_cast<T*>(other.pointer())) {

        m_ptr->ref();
    }

    template<typename U>
    ALWAYS_INLINE RefPointer(NonNullRefPointer<U> const& other) requires(IsConvertible<U*, T*>)
        : m_ptr(const_cast<T*>(static_cast<T const*>(other.pointer()))) {

        m_ptr->ref();
    }

    template<typename U>
    ALWAYS_INLINE RefPointer(NonNullRefPointer<U>&& other) requires(IsConvertible<U*, T*>)
        : m_ptr(static_cast<T*>(&other.leakRef())) { }

    template<typename U>
    RefPointer(RefPointer<U>&& other) requires(IsConvertible<U*, T*>)
        : m_ptr(static_cast<T*>(other.leakRef())) { }

    RefPointer(RefPointer const& other)
        : m_ptr(other.m_ptr) {

        refIfNotNull(m_ptr);
    }

    template<typename U>
    RefPointer(RefPointer<U> const& other) requires(IsConvertible<U*, T*>)
        : m_ptr(const_cast<T*>(static_cast<T const*>(other.pointer()))) {

        refIfNotNull(m_ptr);
    }

    ALWAYS_INLINE ~RefPointer() {

        clear();

#    ifdef SANITIZE_PTRS

        m_ptr = reinterpret_cast<T*>(explodeByte(REFPTR_SCRUB_BYTE));

#    endif
    }

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

///

template<typename T>
struct Formatter<RefPointer<T>> : Formatter<const T*> {

    ErrorOr<void> format(FormatBuilder& builder, RefPointer<T> const& value) {

        return Formatter<const T*>::format(builder, value.ptr());
    }
};

template<typename T>
struct Traits<RefPointer<T>> : public GenericTraits<RefPointer<T>> {

    using PeekType = T*;
    
    using ConstPeekType = const T*;
    
    static unsigned hash(RefPointer<T> const& p) { return hashPointer(p.pointer()); }
    
    static bool equals(RefPointer<T> const& a, RefPointer<T> const& b) { return a.pointer() == b.pointer(); }
};

template<typename T, typename U>
inline NonNullRefPointer<T> staticPointerCast(NonNullRefPointer<U> const& ptr) {

    return NonNullRefPointer<T>(static_cast<const T&>(*ptr));
}

template<typename T, typename U>
inline RefPointer<T> staticPointerCast(RefPointer<U> const& ptr) {

    return RefPointer<T>(static_cast<const T*>(ptr.pointer()));
}

template<typename T, typename U>
inline void swap(RefPointer<T>& a, RefPointer<U>& b) requires(IsConvertible<U*, T*>) {

    a.swap(b);
}

template<typename T>
inline RefPointer<T> adoptRefIfNonNull(T* object) {

    if (object) {

        return RefPointer<T>(RefPointer<T>::Adopt, *object);
    }

    return { };
}

template<typename T, class... Args>
requires(IsConstructible<T, Args...>) inline ErrorOr<NonNullRefPointer<T>> tryMakeRefCounted(Args&&... args) {

    return adoptNonNullRefOrErrorNomem(new (nothrow) T(forward<Args>(args)...));
}

// FIXME: Remove once P0960R3 is available in Clang.

template<typename T, class... Args>
inline ErrorOr<NonNullRefPointer<T>> tryMakeRefCounted(Args&&... args) {

    return adoptNonNullRefOrErrorNomem(new (nothrow) T { forward<Args>(args)... });
}

template<typename T>
inline ErrorOr<NonNullRefPointer<T>> adoptNonNullRefOrErrorNomem(T* object) {

    auto result = adoptRefIfNonNull(object);

    if (!result) {

        return Error::fromError(ENOMEM);
    }

    return result.releaseNonNull();
}