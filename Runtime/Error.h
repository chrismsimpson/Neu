
#pragma once

#include "Optional.h"
#include "StringView.h"
#include "Try.h"
#include "Variant.h"

#include <errno.h>
#include <string.h>

class Error {

public:

    static Error fromErrno(int code) { return Error(code); }

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
class [[nodiscard]] ErrorOr final: public Variant<T, ErrorType> {

public:

    using Variant<T, ErrorType>::Variant;



};

///

template<typename ErrorType>
class [[nodiscard]] ErrorOr<void, ErrorType> {

// private:
//     Optional<ErrorType> m_error;
};