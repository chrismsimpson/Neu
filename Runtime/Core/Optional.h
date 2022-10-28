/*
 * Copyright (c) 2018-2021, Andreas Kling <kling@serenityos.org>
 * Copyright (c) 2021, Daniel Bertalan <dani@danielbertalan.dev>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Assertions.h>
#include <Core/std.h>
#include <Core/Types.h>
#include <Core/kmalloc.h>

// NOTE: If you're here because of an internal compiler error in GCC 10.3.0+,
//       it's because of the following bug:
//
//       https://gcc.gnu.org/bugzilla/show_bug.cgi?id=96745
//
//       Make sure you didn't accidentally make your destructor private before
//       you start bug hunting. :^)

template<typename>
class Optional;

struct NullOptional { };

template<typename T>
requires(!IsLValueReference<T>) class [[nodiscard]] Optional<T> {

    template<typename U>
    friend class Optional;

    static_assert(!IsLValueReference<T> && !IsRValueReference<T>);

public:

    using ValueType = T;

    ALWAYS_INLINE Optional() = default;

    ALWAYS_INLINE Optional(NullOptional) { }

#ifdef HAS_CONDITIONALLY_TRIVIAL

    Optional(Optional const& other) requires(!IsCopyConstructible<T>) = delete;

    Optional(Optional const& other) = default;

    Optional(Optional&& other) requires(!IsMoveConstructible<T>) = delete;

    Optional& operator=(Optional const&) requires(!IsCopyConstructible<T> || !IsDestructible<T>) = delete;
    
    Optional& operator=(Optional const&) = default;

    Optional& operator=(Optional&& other) requires(!IsMoveConstructible<T> || !IsDestructible<T>) = delete;

    ~Optional() requires(!IsDestructible<T>) = delete;

    ~Optional() = default;

#endif

    ALWAYS_INLINE Optional(Optional const& other)
#ifdef HAS_CONDITIONALLY_TRIVIAL
        requires(!IsTriviallyCopyConstructible<T>)
#endif
        : m_hasValue(other.m_hasValue) {

        if (other.hasValue()) {

            new (&m_storage) T(other.value());
        }
    }

    ALWAYS_INLINE Optional(Optional&& other)
        : m_hasValue(other.m_hasValue) {

        if (other.hasValue()) {

            new (&m_storage) T(other.releaseValue());
        }
    }

    template<typename U>
    requires(IsConstructible<T, U const&> && !IsSpecializationOf<T, Optional> && !IsSpecializationOf<U, Optional>) ALWAYS_INLINE explicit Optional(Optional<U> const& other)
        : m_hasValue(other.m_hasValue) {

        if (other.hasValue()) {

            new (&m_storage) T(other.value());
        }
    }

    template<typename U>
    requires(IsConstructible<T, U&&> && !IsSpecializationOf<T, Optional> && !IsSpecializationOf<U, Optional>) ALWAYS_INLINE explicit Optional(Optional<U>&& other)
        : m_hasValue(other.m_hasValue) {

        if (other.hasValue()) {

            new (&m_storage) T(other.releaseValue());
        }
    }

    template<typename U = T>
    ALWAYS_INLINE explicit(!IsConvertible<U&&, T>) Optional(U&& value) requires(!IsSame<RemoveConstVolatileReference<U>, Optional<T>> && IsConstructible<T, U&&>)
        : m_hasValue(true) {

        new (&m_storage) T(forward<U>(value));
    }

    ALWAYS_INLINE Optional& operator=(Optional const& other)
#ifdef HAS_CONDITIONALLY_TRIVIAL
        requires(!IsTriviallyCopyConstructible<T> || !IsTriviallyDestructible<T>)
#endif
    {
        if (this != &other) {
            
            clear();
            
            m_hasValue = other.m_hasValue;
            
            if (other.hasValue()) {

                new (&m_storage) T(other.value());
            }
        }
        return *this;
    }

    ALWAYS_INLINE Optional& operator=(Optional&& other) {

        if (this != &other) {

            clear();
            
            m_hasValue = other.m_hasValue;
            
            if (other.hasValue()) {

                new (&m_storage) T(other.releaseValue());
            }
        }
        
        return *this;
    }

    template<typename O>
    ALWAYS_INLINE bool operator==(Optional<O> const& other) const {

        return hasValue() == other.hasValue() && (!hasValue() || value() == other.value());
    }

    template<typename O>
    ALWAYS_INLINE bool operator==(O const& other) const {

        return hasValue() && value() == other;
    }

    ALWAYS_INLINE ~Optional()
#ifdef HAS_CONDITIONALLY_TRIVIAL
        requires(!IsTriviallyDestructible<T>)
#endif
    {
        clear();
    }

    ALWAYS_INLINE void clear() {

        if (m_hasValue) {

            value().~T();

            m_hasValue = false;
        }
    }

    template<typename... Parameters>
    ALWAYS_INLINE void emplace(Parameters&&... parameters) {

        clear();
        
        m_hasValue = true;
        
        new (&m_storage) T(forward<Parameters>(parameters)...);
    }

    [[nodiscard]] ALWAYS_INLINE bool hasValue() const { return m_hasValue; }

    [[nodiscard]] ALWAYS_INLINE T& value() & {

        VERIFY(m_hasValue);

        return *__builtin_launder(reinterpret_cast<T*>(&m_storage));
    }

    [[nodiscard]] ALWAYS_INLINE T const& value() const& {

        VERIFY(m_hasValue);

        return *__builtin_launder(reinterpret_cast<T const*>(&m_storage));
    }

    [[nodiscard]] ALWAYS_INLINE T value() && {

        return releaseValue();
    }

    [[nodiscard]] ALWAYS_INLINE T releaseValue() {

        VERIFY(m_hasValue);

        T releasedValue = move(value());
        
        value().~T();
        
        m_hasValue = false;
        
        return releasedValue;
    }

    [[nodiscard]] ALWAYS_INLINE T valueOr(T const& fallback) const& {

        if (m_hasValue) {

            return value();
        }

        return fallback;
    }

    [[nodiscard]] ALWAYS_INLINE T valueOr(T&& fallback) && {

        if (m_hasValue) {

            return move(value());
        }

        return move(fallback);
    }

    template<typename Callback>
    [[nodiscard]] ALWAYS_INLINE T valueOrLazyEvaluated(Callback callback) const {

        if (m_hasValue) {

            return value();
        }

        return callback();
    }

    ///

    ALWAYS_INLINE T const& operator*() const { return value(); }
    ALWAYS_INLINE T& operator*() { return value(); }

    ALWAYS_INLINE T const* operator->() const { return &value(); }
    ALWAYS_INLINE T* operator->() { return &value(); }

private:

    alignas(T) UInt8 m_storage[sizeof(T)];

    bool m_hasValue { false };
};

template<typename T>
requires(IsLValueReference<T>) class [[nodiscard]] Optional<T> {

    template<typename>
    friend class Optional;

    template<typename U>
    constexpr static bool CanBePlacedInOptional = IsSame<RemoveReference<T>, RemoveReference<AddConstToReferencedType<U>>> && (IsBaseOf<RemoveConstVolatileReference<T>, RemoveConstVolatileReference<U>> || IsSame<RemoveConstVolatileReference<T>, RemoveConstVolatileReference<U>>);

public:

    using ValueType = T;

    ALWAYS_INLINE Optional() = default;

    template<typename U = T>
    ALWAYS_INLINE Optional(U& value) requires(CanBePlacedInOptional<U&>)
        : m_pointer(&value) { }

    ALWAYS_INLINE Optional(RemoveReference<T>& value)
        : m_pointer(&value) { }

    ALWAYS_INLINE Optional(Optional const& other)
        : m_pointer(other.m_pointer) { }

    ALWAYS_INLINE Optional(Optional&& other)
        : m_pointer(other.m_pointer) {
            
        other.m_pointer = nullptr;
    }

    template<typename U>
    ALWAYS_INLINE Optional(Optional<U> const& other) requires(CanBePlacedInOptional<U>)
        : m_pointer(other.m_pointer) { }

    template<typename U>
    ALWAYS_INLINE Optional(Optional<U>&& other) requires(CanBePlacedInOptional<U>)
        : m_pointer(other.m_pointer) {

        other.m_pointer = nullptr;
    }

    ALWAYS_INLINE Optional& operator=(Optional const& other) {

        m_pointer = other.m_pointer;
        
        return *this;
    }

    ALWAYS_INLINE Optional& operator=(Optional&& other) {

        m_pointer = other.m_pointer;
        
        other.m_pointer = nullptr;
        
        return *this;
    }

    template<typename U>
    ALWAYS_INLINE Optional& operator=(Optional<U> const& other) requires(CanBePlacedInOptional<U>) {

        m_pointer = other.m_pointer;
        
        return *this;
    }

    template<typename U>
    ALWAYS_INLINE Optional& operator=(Optional<U>&& other) requires(CanBePlacedInOptional<U>) {

        m_pointer = other.m_pointer;
        
        other.m_pointer = nullptr;
        
        return *this;
    }

    // Note: Disallows assignment from a temporary as this does not do any lifetime extension.
    template<typename U>
    ALWAYS_INLINE Optional& operator=(U&& value) requires(CanBePlacedInOptional<U>&& IsLValueReference<U>) {

        m_pointer = &value;
        
        return *this;
    }

    ALWAYS_INLINE void clear() {

        m_pointer = nullptr;
    }

    [[nodiscard]] ALWAYS_INLINE bool hasValue() const { return m_pointer != nullptr; }

    [[nodiscard]] ALWAYS_INLINE T value() {

        VERIFY(m_pointer);

        return *m_pointer;
    }

    [[nodiscard]] ALWAYS_INLINE AddConstToReferencedType<T> value() const {

        VERIFY(m_pointer);
        
        return *m_pointer;
    }

    template<typename U>
    requires(IsBaseOf<RemoveConstVolatileReference<T>, U>) [[nodiscard]] ALWAYS_INLINE AddConstToReferencedType<T> valueOr(U& fallback) const {

        if (m_pointer) {

            return value();
        }

        return fallback;
    }

    // Note that this ends up copying the value.
    [[nodiscard]] ALWAYS_INLINE RemoveConstVolatileReference<T> valueOr(RemoveConstVolatileReference<T> fallback) const {

        if (m_pointer) {

            return value();
        }

        return fallback;
    }

    [[nodiscard]] ALWAYS_INLINE T releaseValue() {

        return *exchange(m_pointer, nullptr);
    }

    template<typename U>
    ALWAYS_INLINE bool operator==(Optional<U> const& other) const {

        return hasValue() == other.hasValue() && (!hasValue() || value() == other.value());
    }

    template<typename U>
    ALWAYS_INLINE bool operator==(U const& other) const {

        return hasValue() && value() == other;
    }

    ALWAYS_INLINE AddConstToReferencedType<T> operator*() const { return value(); }

    ALWAYS_INLINE T operator*() { return value(); }

    ALWAYS_INLINE RawPointer<AddConst<RemoveReference<T>>> operator->() const { return &value(); }

    ALWAYS_INLINE RawPointer<RemoveReference<T>> operator->() { return &value(); }

    // Conversion operators from Optional<T&> -> Optional<T>
    ALWAYS_INLINE operator Optional<RemoveConstVolatileReference<T>>() const {

        if (hasValue()) {

            return Optional<RemoveConstVolatileReference<T>>(value());
        }

        return { };
    }

private:

    RemoveReference<T>* m_pointer { nullptr };
};

