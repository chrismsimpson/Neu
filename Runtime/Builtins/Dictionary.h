/*
 * Copyright (c) 2022, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/HashMap.h>
#include <Core/NonNullRefPointer.h>
#include <Core/RefCounted.h>

template<typename K, typename V>
struct DictionaryStorage : public RefCounted<DictionaryStorage<K, V>> {

    HashMap<K, V> map;
};

template<typename K, typename V>
class Dictionary {

    using Storage = DictionaryStorage<K, V>;

public:

    bool isEmpty() const { return m_storage->map.isEmpty(); }

    size_t size() const { return m_storage->map.size(); }

    void clear() { m_storage->map.clear(); }

    ErrorOr<void> set(K const& key, V value) {

        TRY(m_storage->map.set(key, move(value)));
        
        return { };
    }

    bool remove(K const& key) { return m_storage->map.remove(key); }

    bool contains(K const& key) const { return m_storage->map.contains(key); }

    Optional<V> get(K const& key) const { return m_storage->map.get(key); }

    Vector<K> keys() const { return m_storage->map.keys(); }

    ErrorOr<void> ensureCapacity(size_t capacity) {

        TRY(m_storage->map.ensureCapacity(capacity));
        
        return { };
    }

    // FIXME: Remove this constructor once neu knows how to call Dictionary::createEmpty()

    Dictionary()
        : m_storage(MUST(adoptNonNullRefOrErrorNomem(new (nothrow) Storage))) { }

    struct Entry {
        K key;
        V value;
    };

    static ErrorOr<Dictionary> createEmpty() {

        auto storage = TRY(adoptNonNullRefOrErrorNomem(new (nothrow) Storage));
        
        return Dictionary { move(storage) };
    }

    static ErrorOr<Dictionary> createWithEntries(std::initializer_list<Entry> list) {

        auto dictionary = TRY(createEmpty());

        TRY(dictionary.ensureCapacity(list.size()));
        
        for (auto& item : list) {

            TRY(dictionary.set(item.key, item.value));
        }
        
        return dictionary;
    }

private:

    explicit Dictionary(NonNullRefPointer<Storage> storage)
        : m_storage(move(storage)) { }

    NonNullRefPointer<Storage> m_storage;
};
