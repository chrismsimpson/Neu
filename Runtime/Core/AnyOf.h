/*
 * Copyright (c) 2021, the SerenityOS developers.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Concepts.h>
#include <Core/Find.h>
#include <Core/Iterator.h>

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
