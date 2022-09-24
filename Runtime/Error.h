
#pragma once

#include "Optional.h"
#include "StringView.h"
#include "Try.h"
#include "Variant.h"

#include <errno.h>
#include <string.h>

class Error {

public:

    static Error fromError(int code) { return Error(code); }

    static Error fromSyscall(StringView syscallName, int rc) { return Error(syscallName, rc); }

    static Error fromStringLiteral(StringView stringLiteral) { return Error(stringLiteral); }

    bool isErrno() const { return m_code != 0; }

    bool isSyscall() const { return m_syscall; }

    int code() const { return m_code; }

    StringView stringLiteral() const { return m_stringLiteral; }

protected:

    Error(int code)
        : m_code(code) { }

private: 

    Error(StringView stringLiteral)
        : m_stringLiteral(stringLiteral) { }

    Error(StringView syscallName, int rc)
        : m_code(-rc), 
          m_stringLiteral(syscallName), 
          m_syscall(true) { }

    ///

    int m_code { 0 };
    
    StringView m_stringLiteral;
    
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