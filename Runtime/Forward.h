
#pragma once

#include "Types.h"


namespace Detail {

    template<size_t inline_capacity>
    class ByteBuffer;
}

using ByteBuffer = Detail::ByteBuffer<32>;
class Error;
class String;
class StringView;

template<typename T>
class Span;

template<typename T, size_t Size>
struct Array;

template<typename Container, typename ValueType>
class SimpleIterator;

using ReadOnlyBytes = Span<const UInt8>;

using Bytes = Span<UInt8>;


template<typename T>
class OwnPointer;

template<typename T>
class WeakPointer;

template<typename T, size_t inlineCapacity = 0>
requires(!IsRValueReference<T>) class Vector;

template<typename T, typename ErrorType = Error>
class [[nodiscard]] ErrorOr;