
#pragma once

template<typename>
class Optional;

template<typename T>
requires(!IsLValueReference<T>) class [[nodiscard]] Optional<T> {

    template<typename U>
    friend class Optional;

    static_assert(!IsLValueReference<T> && !IsRValueReference<T>);

public:

    using ValueType = T;

    ALWAYS_INLINE Optional() = default;

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


};