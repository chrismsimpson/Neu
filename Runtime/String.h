
#pragma once

#include "ByteBuffer.h"
#include "Format.h"
#include "Forward.h"
#include "RefPointer.h"
// #include "Stream.h"
#include "StringBuilder.h"
#include "StringImpl.h"
#include "StringUtils.h"
#include "Traits.h"

class String {

public:

    ~String() = default;

    String() = default;

    String(StringView view) {

        // TODO
    }








    [[nodiscard]] bool isNull() const { return !m_impl; }

    [[nodiscard]] ALWAYS_INLINE bool isEmpty() const { return length() == 0; }
    
    [[nodiscard]] ALWAYS_INLINE size_t length() const { return m_impl ? m_impl->length() : 0; }
    
    // Includes NUL-terminator, if non-nullptr.

    [[nodiscard]] ALWAYS_INLINE char const* characters() const { return m_impl ? m_impl->characters() : nullptr; }

    [[nodiscard]] bool copyCharactersToBuffer(char* buffer, size_t buffer_size) const;

    [[nodiscard]] ALWAYS_INLINE ReadOnlyBytes bytes() const {

        if (m_impl) {

            return m_impl->bytes();
        }

        return { };
    }

    [[nodiscard]] ALWAYS_INLINE char const& operator[](size_t i) const {

        VERIFY(!isNull());
        
        return (*m_impl)[i];
    }

    using ConstIterator = SimpleIterator<const String, char const>;












    [[nodiscard]] StringImpl const* impl() const { return m_impl.pointer(); }






















    [[nodiscard]] StringView view() const {

        return { characters(), length() };
    }














    // template<typename... Ts>
    // [[nodiscard]] ALWAYS_INLINE constexpr bool is_one_of_ignoring_case(Ts&&... strings) const {

    //     return (... ||
    //             [this, &strings]() -> bool {

    //         if constexpr (requires(Ts a) { a.view()->StringView; }) {

    //             return this->equalsIgnoringCase(forward<Ts>(strings.view()));
    //         }
    //         else {

    //             return this->equalsIgnoringCase(forward<Ts>(strings));
    //         }
    //     }());
    // }

private:

    RefPointer<StringImpl> m_impl;
};
