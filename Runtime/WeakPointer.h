/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#ifdef OS

#else

#    include "Weakable.h"

template<typename T>
class [[nodiscard]] WeakPointer {
    
    template<typename U>
    friend class Weakable;

public:

    WeakPointer() = default;

    template<typename U>
    WeakPointer(WeakPointer<U> const& other) requires(IsBaseOf<T, U>)
        : m_link(other.m_link) { }

    template<typename U>
    WeakPointer(WeakPointer<U>&& other) requires(IsBaseOf<T, U>)
        : m_link(other.takeLink()) { }

    template<typename U>
    WeakPointer& operator=(WeakPointer<U>&& other) requires(IsBaseOf<T, U>) {

        m_link = other.takeLink();
        
        return *this;
    }

    template<typename U>
    WeakPointer& operator=(WeakPointer<U> const& other) requires(IsBaseOf<T, U>) {

        if ((void const*)this != (void const*)&other) {

            m_link = other.m_link;
        }

        return *this;
    }

    WeakPointer& operator=(std::nullptr_t) {

        clear();
        
        return *this;
    }

    template<typename U>
    WeakPointer(const U& object) requires(IsBaseOf<T, U>)
        : m_link(object.template makeWeakPointer<U>().takeLink()) { }

    template<typename U>
    WeakPointer(const U* object) requires(IsBaseOf<T, U>) {

        if (object) {

            m_link = object->template makeWeakPointer<U>().takeLink();
        }
    }

    template<typename U>
    WeakPointer(RefPointer<U> const& object) requires(IsBaseOf<T, U>) {

        if (object) {

            m_link = object->template makeWeakPointer<U>().takeLink();
        }
    }

    template<typename U>
    WeakPointer(NonNullRefPointer<U> const& object) requires(IsBaseOf<T, U>) {

        m_link = object->template makeWeakPointer<U>().takeLink();
    }

    template<typename U>
    WeakPointer& operator=(const U& object) requires(IsBaseOf<T, U>) {

        m_link = object.template makeWeakPointer<U>().takeLink();

        return *this;
    }

    template<typename U>
    WeakPointer& operator=(const U* object) requires(IsBaseOf<T, U>) {

        if (object) {

            m_link = object->template makeWeakPointer<U>().takeLink();
        }
        else {

            m_link = nullptr;
        }
        
        return *this;
    }

    template<typename U>
    WeakPointer& operator=(RefPointer<U> const& object) requires(IsBaseOf<T, U>) {

        if (object) {

            m_link = object->template makeWeakPointer<U>().takeLink();
        }
        else {

            m_link = nullptr;
        }

        return *this;
    }

    template<typename U>
    WeakPointer& operator=(NonNullRefPointer<U> const& object) requires(IsBaseOf<T, U>) {

        m_link = object->template makeWeakPointer<U>().takeLink();
        
        return *this;
    }

    [[nodiscard]] RefPointer<T> strongRef() const {

        return RefPointer<T> { pointer() };
    }

    T* pointer() const { return unsafePointer(); }
    
    T* operator->() { return unsafePointer(); }
    
    const T* operator->() const { return unsafePointer(); }
    
    operator const T*() const { return unsafePointer(); }
    
    operator T*() { return unsafePointer(); }

    [[nodiscard]] T* unsafePointer() const {

        if (m_link) {

            return m_link->template unsafePointer<T>();
        }

        return nullptr;
    }

    operator bool() const { return m_link ? !m_link->isNull() : false; }

    [[nodiscard]] bool isNull() const { return !m_link || m_link->isNull(); }
    
    [[nodiscard]] bool hasValue() const { return !isNull(); }

    T* value() { return pointer(); }

    T const* value() const { return pointer(); }

    void clear() { m_link = nullptr; }

    [[nodiscard]] RefPointer<WeakLink> takeLink() { return move(m_link); }

private:

    WeakPointer(RefPointer<WeakLink> const& link)
        : m_link(link) { }

    RefPointer<WeakLink> m_link;
};






























template<typename T>
template<typename U>
inline ErrorOr<WeakPointer<U>> Weakable<T>::tryMakeWeakPointer() const {

    if (!m_link) {

        m_link = TRY(adoptNonNullRefOrErrorNomem(new (nothrow) WeakLink(const_cast<T&>(static_cast<T const&>(*this)))));
    }

    return WeakPointer<U>(m_link);
}

template<typename T>
struct Formatter<WeakPointer<T>> : Formatter<const T*> {

    ErrorOr<void> format(FormatBuilder& builder, WeakPointer<T> const& value) {

        return Formatter<const T*>::format(builder, value.pointer());
    }
};

template<typename T>
ErrorOr<WeakPointer<T>> tryMakeWeakPointerIfNonNull(T const* ptr) {

    if (ptr) {

        return ptr->template tryMakeWeakPointer<T>();
    }

    return WeakPointer<T> { };
}

template<typename T>
WeakPointer<T> makeWeakPointerIfNonNull(T const* ptr) {

    return MUST(tryMakeWeakPointerIfNonNull(ptr));
}















#endif