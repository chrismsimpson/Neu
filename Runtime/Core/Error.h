/*
 * Copyright (c) 2021, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Optional.h>
#include <Core/Try.h>
#include <Core/Variant.h>

#include <errno.h>
#include <string.h>

class Error {

public:

    static Error fromError(int code) { return Error(code); }

    bool isErrno() const { return m_code != 0; }

    int code() const { return m_code; }

protected:

    Error(int code)
        : m_code(code) { }

private: 

    int m_code { 0 };
    
    bool m_syscall { false };
};

///

template<typename T, typename ErrorType>
class [[nodiscard]] ErrorOr final : public Variant<T, ErrorType> {

public:

    using Variant<T, ErrorType>::Variant;

    template<typename U>
    ALWAYS_INLINE ErrorOr(U&& value) requires(!IsSame<RemoveConstVolatileReference<U>, ErrorOr<T>>)
        : Variant<T, ErrorType>(forward<U>(value)) {

    }

    T& value() {

        return this->template get<T>();
    }

    T const& value() const { return this->template get<T>(); }

    ///

    ErrorType& error() { return this->template get<ErrorType>(); }

    ErrorType const& error() const { return this->template get<ErrorType>(); }

    ///

    bool isError() const { return this->template has<ErrorType>(); }

    T releaseValue() { return move(value()); }

    ErrorType releaseError() { return move(error()); }

    T releaseValueButFixmeShouldPropagateErrors() {

        VERIFY(!isError());

        return releaseValue();
    }

private: 

    // 'downcast' is fishy in this context. Let's hide it by making it private.

    using Variant<T, ErrorType>::downcast;
};

///

// Partial specialization for void value type
template<typename ErrorType>
class [[nodiscard]] ErrorOr<void, ErrorType> {

public:

    ErrorOr(ErrorType error)
        : m_error(move(error)) { }

    ErrorOr() = default;
    
    ErrorOr(ErrorOr&& other) = default;
    
    ErrorOr(ErrorOr const& other) = default;
    
    ~ErrorOr() = default;

    ///

    ErrorOr& operator=(ErrorOr&& other) = default;

    ErrorOr& operator=(ErrorOr const& other) = default;

    ///

    ErrorType& error() { return m_error.value(); }
    
    bool isError() const { return m_error.hasValue(); }

    ErrorType releaseError() { return m_error.releaseValue(); }

    void releaseValue() { }

private:
    
    Optional<ErrorType> m_error;
};