/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include "HashTable.h"
#include "Optional.h"
#include "Vector.h"

#include <initializer_list>

template<typename K, typename V, typename KeyTraits, bool IsOrdered>
class HashMap {

private:

    struct Entry {
        K key;
        V value;
    };

    struct EntryTraits {

        static unsigned hash(Entry const& entry) { return KeyTraits::hash(entry.key); }
        
        static bool equals(Entry const& a, Entry const& b) { return KeyTraits::equals(a.key, b.key); }
    };

public:

    using KeyType = K;
    
    using ValueType = V;

    HashMap() = default;

    HashMap(std::initializer_list<Entry> list) {

        ensureCapacity(list.size());
            
        for (auto& item : list) {

            set(item.key, item.value);
        }
    }

    [[nodiscard]] bool isEmpty() const {

        return m_table.isEmpty();
    }

    [[nodiscard]] size_t size() const { return m_table.size(); }

    [[nodiscard]] size_t capacity() const { return m_table.capacity(); }

    void clear() { m_table.clear(); }

    void clearWithCapacity() { m_table.clearWithCapacity(); }

    HashSetResult set(const K& key, const V& value) { return m_table.set({ key, value }); }
    
    HashSetResult set(const K& key, V&& value) { return m_table.set({ key, move(value) }); }
    
    HashSetResult set(K&& key, V&& value) { return m_table.set({ move(key), move(value) }); }

    ErrorOr<HashSetResult> trySet(const K& key, const V& value) { return m_table.trySet({ key, value }); }
    
    ErrorOr<HashSetResult> trySet(const K& key, V&& value) { return m_table.trySet({ key, move(value) }); }
    
    ErrorOr<HashSetResult> trySet(K&& key, V&& value) { return m_table.trySet({ move(key), move(value) }); }

    bool remove(const K& key) {

        auto it = find(key);

        if (it != end()) {

            m_table.remove(it);
            
            return true;
        }

        return false;
    }

    template<Concepts::HashCompatible<K> Key>
    requires(IsSame<KeyTraits, Traits<K>>) bool remove(Key const& key) {

        auto it = find(key);

        if (it != end()) {

            m_table.remove(it);
            
            return true;
        }

        return false;
    }

    template<typename TUnaryPredicate>
    bool removeAllMatching(TUnaryPredicate predicate) {

        return m_table.template removeAllMatching([&](auto& entry) {

            return predicate(entry.key, entry.value);
        });
    }

    using HashTableType = HashTable<Entry, EntryTraits, IsOrdered>;

    using IteratorType = typename HashTableType::Iterator;
    
    using ConstIteratorType = typename HashTableType::ConstIterator;

    ///

    [[nodiscard]] IteratorType begin() { return m_table.begin(); }
    
    [[nodiscard]] IteratorType end() { return m_table.end(); }

    [[nodiscard]] IteratorType find(const K& key) {

        return m_table.find(KeyTraits::hash(key), [&](auto& entry) { return KeyTraits::equals(key, entry.key); });
    }

    template<typename TUnaryPredicate>
    [[nodiscard]] IteratorType find(unsigned hash, TUnaryPredicate predicate) {

        return m_table.find(hash, predicate);
    }

    ///

    [[nodiscard]] ConstIteratorType begin() const { return m_table.begin(); }
    
    [[nodiscard]] ConstIteratorType end() const { return m_table.end(); }
    
    [[nodiscard]] ConstIteratorType find(const K& key) const {

        return m_table.find(KeyTraits::hash(key), [&](auto& entry) { return KeyTraits::equals(key, entry.key); });
    }
    
    template<typename TUnaryPredicate>
    [[nodiscard]] ConstIteratorType find(unsigned hash, TUnaryPredicate predicate) const {

        return m_table.find(hash, predicate);
    }

    ///

    template<Concepts::HashCompatible<K> Key>
    requires(IsSame<KeyTraits, Traits<K>>) [[nodiscard]] IteratorType find(Key const& key) {

        return m_table.find(Traits<Key>::hash(key), [&](auto& entry) { return Traits<K>::equals(key, entry.key); });
    }

    template<Concepts::HashCompatible<K> Key>
    requires(IsSame<KeyTraits, Traits<K>>) [[nodiscard]] ConstIteratorType find(Key const& key) const {

        return m_table.find(Traits<Key>::hash(key), [&](auto& entry) { return Traits<K>::equals(key, entry.key); });
    }

    ///

    void ensureCapacity(size_t capacity) { m_table.ensureCapacity(capacity); }

    ErrorOr<void> tryEnsureCapacity(size_t capacity) { return m_table.tryEnsureCapacity(capacity); }

    Optional<typename Traits<V>::ConstPeekType> get(const K& key) const requires(!IsPointer<typename Traits<V>::PeekType>) {

        auto it = find(key);

        if (it == end()) {

            return { };
        }

        return (*it).value;
    }

    Optional<typename Traits<V>::ConstPeekType> get(const K& key) const requires(IsPointer<typename Traits<V>::PeekType>) {

        auto it = find(key);

        if (it == end()) {

            return {};
        }

        return (*it).value;
    }

    Optional<typename Traits<V>::PeekType> get(const K& key) requires(!IsConst<typename Traits<V>::PeekType>) {

        auto it = find(key);

        if (it == end()) {
            
            return { };
        }

        return (*it).value;
    }

    template<Concepts::HashCompatible<K> Key>
    requires(IsSame<KeyTraits, Traits<K>>) Optional<typename Traits<V>::PeekType> get(Key const& key)
    const requires(!IsPointer<typename Traits<V>::PeekType>) {

        auto it = find(key);

        if (it == end()) {

            return { };
        }

        return (*it).value;
    }

    template<Concepts::HashCompatible<K> Key>
    requires(IsSame<KeyTraits, Traits<K>>) Optional<typename Traits<V>::ConstPeekType> get(Key const& key)
    const requires(IsPointer<typename Traits<V>::PeekType>) {

        auto it = find(key);

        if (it == end()) {

            return { };
        }

        return (*it).value;
    }

    template<Concepts::HashCompatible<K> Key>
    requires(IsSame<KeyTraits, Traits<K>>) Optional<typename Traits<V>::PeekType> get(Key const& key)
    requires(!IsConst<typename Traits<V>::PeekType>) {

        auto it = find(key);

        if (it == end()) {

            return { };
        }

        return (*it).value;
    }

    ///

    [[nodiscard]] bool contains(const K& key) const {

        return find(key) != end();
    }

    template<Concepts::HashCompatible<K> Key>
    requires(IsSame<KeyTraits, Traits<K>>) [[nodiscard]] bool contains(Key const& value) {

        return find(value) != end();
    }

    ///

    void remove(IteratorType it) {

        m_table.remove(it);
    }

    ///

    V& ensure(const K& key) {

        auto it = find(key);
        
        if (it != end()) {

            return it->value;
        }
        
        auto result = set(key, V());
        
        VERIFY(result == HashSetResult::InsertedNewEntry);
        
        return find(key)->value;
    }

    template<typename Callback>
    V& ensure(K const& key, Callback initializationCallback) {

        auto it = find(key);

        if (it != end()) {

            return it->value;
        }

        auto result = set(key, initializationCallback());
        
        VERIFY(result == HashSetResult::InsertedNewEntry);
        
        return find(key)->value;
    }

    ///

    [[nodiscard]] Vector<K> keys() const {

        Vector<K> list;

        list.ensureCapacity(size());

        for (auto& it : *this) {

            list.uncheckedAppend(it.key);
        }

        return list;
    }

    [[nodiscard]] UInt32 hash() const {

        UInt32 hash = 0;

        for (auto& it : *this) {

            auto entryHash = hashPairUInt32(it.key.hash(), it.value.hash());
            
            hash = hashPairUInt32(hash, entryHash);
        }

        return hash;
    }

private:

    HashTableType m_table;
};