
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