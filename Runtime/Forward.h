
#pragma once

#include "Types.h"

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

template<typename T, typename ErrorType = Error>
class [[nodiscard]] ErrorOr;