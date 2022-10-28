/*
 * Copyright (c) 2020, the SerenityOS developers.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/std.h>

template<typename... Types>
struct TypeList;

template<unsigned Index, typename List>
struct TypeListElement;

template<unsigned Index, typename Head, typename... Tail>
struct TypeListElement<Index, TypeList<Head, Tail...>>
    : TypeListElement<Index - 1, TypeList<Tail...>> { };


template<typename Head, typename... Tail>
struct TypeListElement<0, TypeList<Head, Tail...>> {

    using Type = Head;
};

template<typename... Types>
struct TypeList {

    static constexpr unsigned size = sizeof...(Types);

    template<unsigned N>
    using Type = typename TypeListElement<N, TypeList<Types...>>::Type;
};

template<typename T>
struct TypeWrapper {

    using Type = T;
};

template<typename List, typename F, unsigned... Indices>
constexpr void forEachTypeImpl(F&& f, IndexSequence<Indices...>) {

    (forward<F>(f)(TypeWrapper<typename List::template Type<Indices>> { }), ...);
}

template<typename List, typename F>
constexpr void forEachType(F&& f) {

    forEachTypeImpl<List>(forward<F>(f), MakeIndexSequence<List::size> { });
}

template<typename ListA, typename ListB, typename F, unsigned... Indices>
constexpr void forEachTypeZippedImpl(F&& f, IndexSequence<Indices...>) {

    (forward<F>(f)(TypeWrapper<typename ListA::template Type<Indices>> { }, TypeWrapper<typename ListB::template Type<Indices>> {}), ...);
}

template<typename ListA, typename ListB, typename F>
constexpr void forEachTypeZipped(F&& f) {

    static_assert(ListA::size == ListB::size, "Can't zip TypeLists that aren't the same size!");

    forEachTypeZippedImpl<ListA, ListB>(forward<F>(f), MakeIndexSequence<ListA::size> { });
}