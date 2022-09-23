
#pragma once

#include "Error.h"
#include "NonNullOwnPointer.h"
#include "RefCounted.h"

#define OWNPTR_SCRUB_BYTE 0xf0

template<typename T>
class [[nodiscard]] OwnPointer {

public:

    OwnPointer() = default;

    ///

    // NonnullOwnPtr<T> release_nonnull()
    // {
    //     VERIFY(m_ptr);
    //     return NonnullOwnPtr<T>(NonnullOwnPtr<T>::Adopt, *leak_ptr());
    // }

    // template<typename U>
    // NonnullOwnPtr<U> release_nonnull() {

    //     VERIFY(m_ptr);
        
    //     return NonnullOwnPtr<U>(NonnullOwnPtr<U>::Adopt, static_cast<U&>(*leak_ptr()));
    // }

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
            requires { requires typename T::AllowOwnPointer()(); } || !requires { requires !typename T::AllowOwnPointer()(); declval<T>().ref(); declval<T>().unref(); }, "Use RefPtr<> for RefCounted types");
    }

private:

    T* m_ptr = nullptr;
};