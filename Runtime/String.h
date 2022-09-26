
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




















    [[nodiscard]] StringImpl const* impl() const { return m_impl.pointer(); }





private:

    RefPointer<StringImpl> m_impl;
};
