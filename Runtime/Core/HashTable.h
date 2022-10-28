/*
 * Copyright (c) 2018-2020, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Concepts.h>
#include <Core/Error.h>
#include <Core/Forward.h>
#include <Core/HashFunctions.h>
#include <Core/std.h>
#include <Core/Traits.h>
#include <Core/Types.h>
#include <Core/kmalloc.h>

enum class HashSetResult {

    InsertedNewEntry,
    ReplacedExistingEntry,
    KeptExistingEntry
};

enum class HashSetExistingEntryBehavior {

    Keep,
    Replace
};

///

// Upper nibble determines state class:
// - 0: unused bucket
// - 1: used bucket
// - F: end bucket
// Lower nibble determines state within a class.

enum class BucketState : UInt8 {

    Free = 0x00,
    Used = 0x10,
    Deleted = 0x01,
    Rehashed = 0x12,
    End = 0xFF,
};

///

// Note that because there's the end state, used and free are not 100% opposites!

constexpr bool isUsedBucket(BucketState state) {

    return (static_cast<UInt8>(state) & 0xf0) == 0x10;
}

constexpr bool isFreeBucket(BucketState state) {

    return (static_cast<UInt8>(state) & 0xf0) == 0x00;
}

template<typename HashTableType, typename T, typename BucketType>
class HashTableIterator {

    friend HashTableType;

public:

    bool operator==(HashTableIterator const& other) const { return m_bucket == other.m_bucket; }

    bool operator!=(HashTableIterator const& other) const { return m_bucket != other.m_bucket; }

    T& operator*() { return *m_bucket->slot(); }

    T* operator->() { return m_bucket->slot(); }

    void operator++() { skipToNext(); }


private:

    void skipToNext() {

        if (!m_bucket) {

            return;
        }

        do {

            ++m_bucket;

            if (m_bucket->state == BucketState::Used) {

                return;
            }
        }
        while (m_bucket->state != BucketState::End);

        if (m_bucket->state == BucketState::End) {

            m_bucket = nullptr;
        }
    }

    explicit HashTableIterator(BucketType* bucket)
        : m_bucket(bucket) { }

    BucketType* m_bucket { nullptr };
};

///

template<typename OrderedHashTableType, typename T, typename BucketType>
class OrderedHashTableIterator {
    
    friend OrderedHashTableType;

public:

    bool operator==(OrderedHashTableIterator const& other) const { return m_bucket == other.m_bucket; }
    
    bool operator!=(OrderedHashTableIterator const& other) const { return m_bucket != other.m_bucket; }
    
    T& operator*() { return *m_bucket->slot(); }
    
    T* operator->() { return m_bucket->slot(); }
    
    void operator++() { m_bucket = m_bucket->next; }
    
    void operator--() { m_bucket = m_bucket->previous; }

private:

    explicit OrderedHashTableIterator(BucketType* bucket)
        : m_bucket(bucket) { }

    BucketType* m_bucket { nullptr };
};

///


template<typename T, typename TraitsForT, bool IsOrdered>
class HashTable {
    
    static constexpr size_t loadFactorInPercent = 60;

    ///

    struct Bucket {
        
        BucketState state;
        
        alignas(T) UInt8 storage[sizeof(T)];

        T* slot() { return reinterpret_cast<T*>(storage); }
        
        const T* slot() const { return reinterpret_cast<const T*>(storage); }
    };

    ///

    struct OrderedBucket {

        OrderedBucket* previous;
        
        OrderedBucket* next;
        
        BucketState state;
        
        alignas(T) UInt8 storage[sizeof(T)];
        
        T* slot() { return reinterpret_cast<T*>(storage); }
        
        const T* slot() const { return reinterpret_cast<const T*>(storage); }
    };

    ///

    using BucketType = Conditional<IsOrdered, OrderedBucket, Bucket>;

    struct CollectionData { };

    struct OrderedCollectionData {

        BucketType* head { nullptr };
        
        BucketType* tail { nullptr };
    };

    using CollectionDataType = Conditional<IsOrdered, OrderedCollectionData, CollectionData>;

    ///

public:

    HashTable() = default;
    
    explicit HashTable(size_t capacity) { rehash(capacity); }

    ~HashTable() {

        if (!m_buckets) {

            return;
        }

        for (size_t i = 0; i < m_capacity; ++i) {

            if (isUsedBucket(m_buckets[i].state)) {

                m_buckets[i].slot()->~T();
            }
        }

        kfreeSized(m_buckets, sizeInBytes(m_capacity));
    }

    HashTable(HashTable const& other) {

        rehash(other.capacity());

        for (auto& it : other) {

            set(it);
        }
    }

    HashTable& operator=(HashTable const& other) {

        HashTable temporary(other);
        
        swap(*this, temporary);
        
        return *this;
    }

    HashTable(HashTable&& other) noexcept
        : m_buckets(other.m_buckets), 
          m_collectionData(other.m_collectionData), 
          m_size(other.m_size), 
          m_capacity(other.m_capacity), 
          m_deletedCount(other.m_deletedCount) {

        other.m_size = 0;
        
        other.m_capacity = 0;
        
        other.m_deletedCount = 0;
        
        other.m_buckets = nullptr;

        if constexpr (IsOrdered) {

            other.m_collectionData = { nullptr, nullptr };
        }
    }

    HashTable& operator=(HashTable&& other) noexcept {

        HashTable temporary { move(other) };
        
        swap(*this, temporary);
        
        return *this;
    }

    friend void swap(HashTable& a, HashTable& b) noexcept {

        swap(a.m_buckets, b.m_buckets);
        
        swap(a.m_size, b.m_size);
        
        swap(a.m_capacity, b.m_capacity);
        
        swap(a.m_deletedCount, b.m_deletedCount);

        if constexpr (IsOrdered) {

            swap(a.m_collectionData, b.m_collectionData);
        }
    }

    [[nodiscard]] bool isEmpty() const { return m_size == 0; }
    
    [[nodiscard]] size_t size() const { return m_size; }
    
    [[nodiscard]] size_t capacity() const { return m_capacity; }

    template<typename U, size_t N>
    ErrorOr<void> trySetFrom(U (&fromArray)[N]) {

        for (size_t i = 0; i < N; ++i) {

            TRY(trySet(fromArray[i]));
        }

        return { };
    }

    template<typename U, size_t N>
    void setFrom(U (&fromArray)[N]) {

        MUST(trySetFrom(fromArray));
    }

    void ensureCapacity(size_t capacity) {

        VERIFY(capacity >= size());
        
        rehash(capacity * 2);
    }

    ErrorOr<void> tryEnsureCapacity(size_t capacity) {

        VERIFY(capacity >= size());
        
        return tryRehash(capacity * 2);
    }

    [[nodiscard]] bool contains(T const& value) const {

        return find(value) != end();
    }

    template<Concepts::HashCompatible<T> K>
    requires(IsSame<TraitsForT, Traits<T>>) [[nodiscard]] bool contains(K const& value) const {

        return find(value) != end();
    }

    using Iterator = Conditional<IsOrdered,
        OrderedHashTableIterator<HashTable, T, BucketType>,
        HashTableIterator<HashTable, T, BucketType>>;

    [[nodiscard]] Iterator begin() {

        if constexpr (IsOrdered) {
            
            return Iterator(m_collectionData.head);
        }

        for (size_t i = 0; i < m_capacity; ++i) {

            if (isUsedBucket(m_buckets[i].state)) {

                return Iterator(&m_buckets[i]);
            }
        }

        return end();
    }

    [[nodiscard]] Iterator end() {

        return Iterator(nullptr);
    }

    using ConstIterator = Conditional<IsOrdered,
        OrderedHashTableIterator<const HashTable, const T, const BucketType>,
        HashTableIterator<const HashTable, const T, const BucketType>>;

    [[nodiscard]] ConstIterator begin() const {

        if constexpr (IsOrdered) {

            return ConstIterator(m_collectionData.head);
        }

        for (size_t i = 0; i < m_capacity; ++i) {

            if (isUsedBucket(m_buckets[i].state)) {

                return ConstIterator(&m_buckets[i]);
            }
        }

        return end();
    }

    [[nodiscard]] ConstIterator end() const {

        return ConstIterator(nullptr);
    }

    void clear() {

        *this = HashTable();
    }

    void clearWithCapacity() {

        if constexpr (!Detail::IsTriviallyDestructible<T>) {

            for (auto* bucket : *this) {

                bucket->~T();
            }
        }

        __builtin_memset(m_buckets, 0, sizeInBytes(capacity()));

        m_size = 0;
        
        m_deletedCount = 0;

        if constexpr (IsOrdered) {

            m_collectionData = { nullptr, nullptr };
        }
        else {

            m_buckets[m_capacity].state = BucketState::End;
        }
    }

    template<typename U = T>
    ErrorOr<HashSetResult> trySet(U&& value, HashSetExistingEntryBehavior existingEntryBehaviour = HashSetExistingEntryBehavior::Replace) {

        auto* bucket = TRY(tryLookupForWriting(value));

        if (isUsedBucket(bucket->state)) {

            if (existingEntryBehaviour == HashSetExistingEntryBehavior::Keep) {

                return HashSetResult::KeptExistingEntry;
            }

            (*bucket->slot()) = forward<U>(value);
           
            return HashSetResult::ReplacedExistingEntry;
        }

        new (bucket->slot()) T(forward<U>(value));

        if (bucket->state == BucketState::Deleted) {

            --m_deletedCount;
        }

        bucket->state = BucketState::Used;

        if constexpr (IsOrdered) {

            if (!m_collectionData.head) {
                
                m_collectionData.head = bucket;
            } 
            else {

                bucket->previous = m_collectionData.tail;
                
                m_collectionData.tail->next = bucket;
            }

            m_collectionData.tail = bucket;
        }

        ++m_size;

        return HashSetResult::InsertedNewEntry;
    }

    template<typename U = T>
    HashSetResult set(U&& value, HashSetExistingEntryBehavior existingEntryBehaviour = HashSetExistingEntryBehavior::Replace) {

        return MUST(trySet(forward<U>(value), existingEntryBehaviour));
    }

    template<typename TUnaryPredicate>
    [[nodiscard]] Iterator find(unsigned hash, TUnaryPredicate predicate) {

        return Iterator(lookupWithHash(hash, move(predicate)));
    }

    [[nodiscard]] Iterator find(T const& value) {

        return find(TraitsForT::hash(value), [&](auto& other) { return TraitsForT::equals(value, other); });
    }

    template<typename TUnaryPredicate>
    [[nodiscard]] ConstIterator find(unsigned hash, TUnaryPredicate predicate) const {

        return ConstIterator(lookupWithHash(hash, move(predicate)));
    }

    [[nodiscard]] ConstIterator find(T const& value) const {

        return find(TraitsForT::hash(value), [&](auto& other) { return TraitsForT::equals(value, other); });
    }

    ///

    // FIXME: Support for predicates, while guaranteeing that the predicate call
    //        does not call a non trivial constructor each time invoked

    template<Concepts::HashCompatible<T> K>
    requires(IsSame<TraitsForT, Traits<T>>) [[nodiscard]] Iterator find(K const& value) {

        return find(Traits<K>::hash(value), [&](auto& other) { return Traits<T>::equals(other, value); });
    }

    template<Concepts::HashCompatible<T> K, typename TUnaryPredicate>
    requires(IsSame<TraitsForT, Traits<T>>) [[nodiscard]] Iterator find(K const& value, TUnaryPredicate predicate) {

        return find(Traits<K>::hash(value), move(predicate));
    }

    template<Concepts::HashCompatible<T> K>
    requires(IsSame<TraitsForT, Traits<T>>) [[nodiscard]] ConstIterator find(K const& value) const {

        return find(Traits<K>::hash(value), [&](auto& other) { return Traits<T>::equals(other, value); });
    }

    template<Concepts::HashCompatible<T> K, typename TUnaryPredicate>
    requires(IsSame<TraitsForT, Traits<T>>) [[nodiscard]] ConstIterator find(K const& value, TUnaryPredicate predicate) const {

        return find(Traits<K>::hash(value), move(predicate));
    }

    bool remove(const T& value) {

        auto it = find(value);

        if (it != end()) {
            
            remove(it);
            
            return true;
        }

        return false;
    }

    template<Concepts::HashCompatible<T> K>
    requires(IsSame<TraitsForT, Traits<T>>) bool remove(K const& value) {

        auto it = find(value);
        
        if (it != end()) {
            
            remove(it);
            
            return true;
        }

        return false;
    }

    void remove(Iterator iterator) {

        VERIFY(iterator.m_bucket);
        
        auto& bucket = *iterator.m_bucket;
        
        VERIFY(isUsedBucket(bucket.state));

        deleteBucket(bucket);
        
        --m_size;
        
        ++m_deletedCount;

        rehashInPlaceIfNeeded();
    }

    template<typename TUnaryPredicate>
    bool removeAllMatching(TUnaryPredicate predicate) {

        size_t removedCount = 0;
        
        for (size_t i = 0; i < m_capacity; ++i) {
            
            auto& bucket = m_buckets[i];
            
            if (isUsedBucket(bucket.state) && predicate(*bucket.slot())) {
            
                deleteBucket(bucket);
            
                ++removedCount;
            }
        }

        if (removedCount) {
        
            m_deletedCount += removedCount;
        
            m_size -= removedCount;
        }

        rehashInPlaceIfNeeded();

        return removedCount;
    }

private:

    void insertDuringRehash(T&& value) {

        auto& bucket = lookupForWriting(value);
        
        new (bucket.slot()) T(move(value));
        
        bucket.state = BucketState::Used;

        if constexpr (IsOrdered) {

            if (!m_collectionData.head) {
                
                m_collectionData.head = &bucket;
            } 
            else {
                
                bucket.previous = m_collectionData.tail;
                
                m_collectionData.tail->next = &bucket;
            }
            
            m_collectionData.tail = &bucket;
        }
    }

    [[nodiscard]] static constexpr size_t sizeInBytes(size_t capacity) {

        if constexpr (IsOrdered) {
            
            return sizeof(BucketType) * capacity;
        } 
        else {

            return sizeof(BucketType) * (capacity + 1);
        }
    }

    ErrorOr<void> tryRehash(size_t newCapacity) {

        if (newCapacity == m_capacity && newCapacity >= 4) {

            rehashInPlace();

            return { };
        }

        newCapacity = max(newCapacity, static_cast<size_t>(4));

        newCapacity = kmallocGoodSize(newCapacity * sizeof(BucketType)) / sizeof(BucketType);

        auto* oldBuckets = m_buckets;
        
        auto oldCapacity = m_capacity;
        
        Iterator oldIter = begin();

        auto* newBuckets = kcalloc(1, sizeInBytes(newCapacity));

        if (!newBuckets) {

            return Error::fromError(ENOMEM);
        }

        m_buckets = (BucketType*) newBuckets;

        m_capacity = newCapacity;
        
        m_deletedCount = 0;

        if constexpr (IsOrdered) {

            m_collectionData = { nullptr, nullptr };
        }
        else {

            m_buckets[m_capacity].state = BucketState::End;
        }

        if (!oldBuckets) {

            return { };
        }

        for (auto it = move(oldIter); it != end(); ++it) {

            insertDuringRehash(move(*it));
            
            it->~T();
        }

        kfreeSized(oldBuckets, sizeInBytes(oldCapacity));

        return { };
    }

    void rehash(size_t newCapacity) {

        MUST(tryRehash(newCapacity));
    }

    void rehashInPlace() {

        // FIXME: This implementation takes two loops over the entire bucket array, but avoids re-allocation.
        //        Please benchmark your new implementation before you replace this.
        //        The reason is that because of collisions, we use the special "rehashed" bucket state to mark already-rehashed used buckets.
        //        Because we of course want to write into old used buckets, but already rehashed data shall not be touched.

        // FIXME: Find a way to reduce the cognitive complexity of this function.

        for (size_t i = 0; i < m_capacity; ++i) {

            auto& bucket = m_buckets[i];

            // FIXME: Bail out when we have handled every filled bucket.

            if (bucket.state == BucketState::Rehashed || bucket.state == BucketState::End || bucket.state == BucketState::Free) {

                continue;
            }

            if (bucket.state == BucketState::Deleted) {

                bucket.state = BucketState::Free;
                
                continue;
            }

            auto const newHash = TraitsForT::hash(*bucket.slot());

            if (newHash % m_capacity == i) {
                
                bucket.state = BucketState::Rehashed;
                
                continue;
            }

            auto targetHash = newHash;

            auto const toMoveHash = i;

            BucketType* targetBucket = &m_buckets[targetHash % m_capacity];

            BucketType* bucketToMove = &m_buckets[i];

            // Try to move the bucket to move into its correct spot.
            // During the procedure, we might re-hash or actually change the bucket to move.
            while (!isFreeBucket(bucketToMove->state)) {

                // If we're targeting ourselves, there's nothing to do.

                if (toMoveHash == targetHash % m_capacity) {

                    bucketToMove->state = BucketState::Rehashed;
                    
                    break;
                }

                if (isFreeBucket(targetBucket->state)) {
                    
                    // We can just overwrite the target bucket and bail out.
                    
                    new (targetBucket->slot()) T(move(*bucketToMove->slot()));
                    
                    targetBucket->state = BucketState::Rehashed;
                    
                    bucketToMove->state = BucketState::Free;

                    if constexpr (IsOrdered) {

                        swap(bucketToMove->previous, targetBucket->previous);
                        
                        swap(bucketToMove->next, targetBucket->next);

                        if (targetBucket->previous) {

                            targetBucket->previous->next = targetBucket;
                        }
                        else {

                            m_collectionData.head = targetBucket;
                        }

                        if (targetBucket->next) {

                            targetBucket->next->previous = targetBucket;
                        }
                        else {

                            m_collectionData.tail = targetBucket;
                        }
                    }
                } 
                else if (targetBucket->state == BucketState::Rehashed) {

                    // If the target bucket is already re-hashed, we do normal probing.

                    targetHash = doubleHashUInt32(targetHash);

                    targetBucket = &m_buckets[targetHash % m_capacity];
                } 
                else {
                    
                    VERIFY(targetBucket->state != BucketState::End);
                    
                    // The target bucket is a used bucket that hasn't been re-hashed.
                    // Swap the data into the target; now the target's data resides in the bucket to move again.
                    // (That's of course what we want, how neat!)
                    
                    swap(*bucketToMove->slot(), *targetBucket->slot());
                    
                    bucketToMove->state = targetBucket->state;
                    
                    targetBucket->state = BucketState::Rehashed;

                    if constexpr (IsOrdered) {

                        // Update state for the target bucket, we'll do the bucket to move later.
                        
                        swap(bucketToMove->previous, targetBucket->previous);
                        
                        swap(bucketToMove->next, targetBucket->next);

                        if (targetBucket->previous) {

                            targetBucket->previous->next = targetBucket;
                        }
                        else {

                            m_collectionData.head = targetBucket;
                        }

                        if (targetBucket->next) {

                            targetBucket->next->previous = targetBucket;
                        }
                        else {

                            m_collectionData.tail = targetBucket;
                        }
                    }

                    targetHash = TraitsForT::hash(*bucketToMove->slot());
                    targetBucket = &m_buckets[targetHash % m_capacity];

                    // The data is already in the correct location: Adjust the pointers

                    if (targetHash % m_capacity == toMoveHash) {

                        bucketToMove->state = BucketState::Rehashed;

                        if constexpr (IsOrdered) {

                            // Update state for the bucket to move as it's not actually moved anymore.
                            
                            if (bucketToMove->previous) {

                                bucketToMove->previous->next = bucketToMove;
                            }
                            else {

                                m_collectionData.head = bucketToMove;
                            }
                            
                            if (bucketToMove->next) {

                                bucketToMove->next->previous = bucketToMove;
                            }
                            else {

                                m_collectionData.tail = bucketToMove;
                            }
                        }

                        break;
                    }
                }
            }

            ///
            
            // After this, the bucketToMove either contains data that rehashes to itself, or it contains nothing as we were able to move the last thing.
            
            if (bucketToMove->state == BucketState::Deleted) {

                bucketToMove->state = BucketState::Free;
            }
        }

        for (size_t i = 0; i < m_capacity; ++i) {

            if (m_buckets[i].state == BucketState::Rehashed) {

                m_buckets[i].state = BucketState::Used;
            }
        }

        m_deletedCount = 0;
    }

    void rehashInPlaceIfNeeded() {

        // This signals a "thrashed" hash table with many deleted slots.
        
        if (m_deletedCount >= m_size && shouldGrow()) {

            rehashInPlace();
        }
    }

    template<typename TUnaryPredicate>
    [[nodiscard]] BucketType* lookupWithHash(unsigned hash, TUnaryPredicate predicate) const {

        if (isEmpty()) {

            return nullptr;
        }

        for (;;) {

            auto& bucket = m_buckets[hash % m_capacity];

            if (isUsedBucket(bucket.state) && predicate(*bucket.slot())) {

                return &bucket;
            }

            if (bucket.state != BucketState::Used && bucket.state != BucketState::Deleted) {

                return nullptr;
            }

            hash = doubleHashUInt32(hash);
        }
    }

    ErrorOr<BucketType*> tryLookupForWriting(T const& value) {

        // FIXME: Maybe overrun the "allowed" load factor to avoid OOM
        //        If we are allowed to do that, separate that logic from
        //        the normal lookup_for_writing
        
        if (shouldGrow()) {

            TRY(tryRehash(capacity() * 2));
        }

        auto hash = TraitsForT::hash(value);
        
        BucketType* firstEmptyBucket = nullptr;
        
        for (;;) {

            auto& bucket = m_buckets[hash % m_capacity];

            if (isUsedBucket(bucket.state) && TraitsForT::equals(*bucket.slot(), value)) {

                return &bucket;
            }

            if (!isUsedBucket(bucket.state)) {

                if (!firstEmptyBucket) {

                    firstEmptyBucket = &bucket;
                }

                if (bucket.state != BucketState::Deleted) {

                    return const_cast<BucketType*>(firstEmptyBucket);
                }
            }

            hash = doubleHashUInt32(hash);
        }
    }

    [[nodiscard]] BucketType& lookupForWriting(T const& value) {

        return *MUST(tryLookupForWriting(value));
    }

    [[nodiscard]] size_t usedBucketCount() const { return m_size + m_deletedCount; }
    
    [[nodiscard]] bool shouldGrow() const { return ((usedBucketCount() + 1) * 100) >= (m_capacity * loadFactorInPercent); }

    void deleteBucket(auto& bucket) {

        bucket.slot()->~T();
        
        bucket.state = BucketState::Deleted;

        if constexpr (IsOrdered) {

            if (bucket.previous) {

                bucket.previous->next = bucket.next;
            }
            else {

                m_collectionData.head = bucket.next;
            }

            if (bucket.next) {

                bucket.next->previous = bucket.previous;
            }
            else {

                m_collectionData.tail = bucket.previous;
            }
        }
    }

    BucketType* m_buckets { nullptr };

    [[no_unique_address]] CollectionDataType m_collectionData;
    
    size_t m_size { 0 };
    
    size_t m_capacity { 0 };
    
    size_t m_deletedCount { 0 };
};