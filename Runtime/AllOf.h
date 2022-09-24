
#pragma once

#include "Concepts.h"
#include "Find.h"
#include "Iterator.h"

template<typename TEndIterator, IteratorPairWith<TEndIterator> TIterator>
constexpr bool allOf(
    TIterator const& begin,
    TEndIterator const& end,
    auto const& predicate) {

    constexpr auto negatedPredicate = [](auto const& pred) {

        return [&](auto const& elem) { return !pred(elem); };
    };

    return !(findIf(begin, end, negatedPredicate(predicate)) != end);
}

template<IterableContainer Container>
constexpr bool allOf(Container&& container, auto const& predicate) {

    return allOf(container.begin(), container.end(), predicate);
}