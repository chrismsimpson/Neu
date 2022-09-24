
#pragma once

#include "Concepts.h"
#include "Find.h"
#include "Iterator.h"

template<typename TEndIterator, IteratorPairWith<TEndIterator> TIterator>
constexpr bool anyOf(
    TIterator const& begin,
    TEndIterator const& end,
    auto const& predicate) {

    return findIf(begin, end, predicate) != end;
}

template<IterableContainer Container>
constexpr bool anyOf(Container&& container, auto const& predicate) {

    return anyOf(container.begin(), container.end(), predicate);
}
