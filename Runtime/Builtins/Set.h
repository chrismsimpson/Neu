/*
 * Copyright (c) 2022, Mustafa Quraish <mustafa@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/HashTable.h>
#include <initializer_list>

template<typename T, typename TraitsForT, bool IsOrdered>
class Set : public HashTable<T, TraitsForT, IsOrdered> {

private:

    using HashTableType = HashTable<T, TraitsForT, IsOrdered>;

public:

    using HashTableType::HashTable;

    Set(std::initializer_list<T> list) {

        MUST(ensureCapacity(list.size()));

        for (auto& item : list) {

            MUST(add(item));
        }
    }

    using HashTableType::contains;

    ErrorOr<HashSetResult> add(T const& value) { return HashTableType::trySet(value); }
    
    ErrorOr<HashSetResult> add(T&& value) { return HashTableType::trySet(move(value)); }
    
    ErrorOr<void> ensureCapacity(size_t capacity) { return HashTableType::tryEnsureCapacity(capacity); }

    using HashTableType::capacity;
    
    using HashTableType::clear;
    
    using HashTableType::remove;
    
    using HashTableType::size;

    [[nodiscard]] UInt32 hash() const {

        UInt32 hash = 0;

        for (auto& value : *this) {

            hash = hashPairUInt32(hash, value.hash());
        }

        return hash;
    }
};