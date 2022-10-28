/*
 * Copyright (c) 2018-2021, Andreas Kling <kling@serenityos.org>
 * Copyright (c) 2021, the SerenityOS developers.
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Assertions.h>
#include <Core/Error.h>
#include <Core/Find.h>
#include <Core/Forward.h>
#include <Core/Iterator.h>
#include <Core/Optional.h>
#include <Core/ReverseIterator.h>
#include <Core/Span.h>
#include <Core/std.h>
#include <Core/Traits.h>
#include <Core/TypedTransfer.h>
#include <Core/kmalloc.h>
#include <initializer_list>

namespace Detail {

    template<typename StorageType, bool>
    struct CanBePlacedInsideVectorHelper;

    template<typename StorageType>
    struct CanBePlacedInsideVectorHelper<StorageType, true> {

        template<typename U>
        static constexpr bool value = requires(U&& u) { StorageType { &u }; };
    };


    template<typename StorageType>
    struct CanBePlacedInsideVectorHelper<StorageType, false> {

        template<typename U>
        static constexpr bool value = requires(U&& u) { StorageType(forward<U>(u)); };
    };
}

///

template<typename T, size_t inlineCapacity>
requires(!IsRValueReference<T>) class Vector {

private:

    static constexpr bool containsReference = IsLValueReference<T>;

    using StorageType = Conditional<containsReference, RawPointer<RemoveReference<T>>, T>;

    using VisibleType = RemoveReference<T>;

    template<typename U>
    static constexpr bool CanBePlacedInsideVector = Detail::CanBePlacedInsideVectorHelper<StorageType, containsReference>::template value<U>;

public:

    using ValueType = T;

    Vector()
        : m_capacity(inlineCapacity) { }

    Vector(std::initializer_list<T> list) requires(!IsLValueReference<T>) {

        ensureCapacity(list.size());

        for (auto& item : list) {

            uncheckedAppend(item);
        }
    }

    Vector(Vector&& other)
        : m_size(other.m_size), 
            m_capacity(other.m_capacity), 
            m_outlineBuffer(other.m_outlineBuffer) {

        if constexpr (inlineCapacity > 0) {

            if (!m_outlineBuffer) {

                for (size_t i = 0; i < m_size; ++i) {

                    new (&inlineBuffer()[i]) StorageType(move(other.inlineBuffer()[i]));

                    other.inlineBuffer()[i].~StorageType();
                }
            }
        }

        other.m_outlineBuffer = nullptr;
        
        other.m_size = 0;
        
        other.resetCapacity();
    }

    Vector(Vector const& other) {

        ensureCapacity(other.size());
        
        TypedTransfer<StorageType>::copy(data(), other.data(), other.size());
        
        m_size = other.size();
    }

    explicit Vector(Span<T const> other) requires(!IsLValueReference<T>) {

        ensureCapacity(other.size());
        
        TypedTransfer<StorageType>::copy(data(), other.data(), other.size());
        
        m_size = other.size();
    }

    template<size_t otherInlineCapacity>
    Vector(Vector<T, otherInlineCapacity> const& other) {

        ensureCapacity(other.size());
        
        TypedTransfer<StorageType>::copy(data(), other.data(), other.size());
        
        m_size = other.size();
    }

    ~Vector() {
        
        clear();
    }

    ///

    Span<StorageType> span() { return { data(), size() }; }

    Span<StorageType const> span() const { return { data(), size() }; }

    ///

    operator Span<StorageType>() { return span(); }
    
    operator Span<StorageType const>() const { return span(); }

    ///

    bool isEmpty() const { return size() == 0; }

    ALWAYS_INLINE size_t size() const { return m_size; }

    size_t capacity() const { return m_capacity; }

    ALWAYS_INLINE StorageType* data() {

        if constexpr (inlineCapacity > 0) {

            return m_outlineBuffer ? m_outlineBuffer : inlineBuffer();
        }

        return m_outlineBuffer;
    }

    ALWAYS_INLINE StorageType const* data() const {

        if constexpr (inlineCapacity > 0) {

            return m_outlineBuffer ? m_outlineBuffer : inlineBuffer();
        }

        return m_outlineBuffer;
    }

    ALWAYS_INLINE VisibleType const& at(size_t i) const {

        VERIFY(i < m_size);

        if constexpr (containsReference) {

            return *data()[i];
        }
        else {

            return data()[i];
        }
    }

    ALWAYS_INLINE VisibleType& at(size_t i) {

        VERIFY(i < m_size);

        if constexpr (containsReference) {

            return *data()[i];
        }
        else {

            return data()[i];
        }
    }

    ///

    ALWAYS_INLINE VisibleType const& operator[](size_t i) const { return at(i); }

    ALWAYS_INLINE VisibleType& operator[](size_t i) { return at(i); }

    ///

    VisibleType const& first() const { return at(0); }

    VisibleType& first() { return at(0); }

    ///

    VisibleType const& last() const { return at(size() - 1); }

    VisibleType& last() { return at(size() - 1); }

    ///

    template<typename TUnaryPredicate>
    Optional<VisibleType&> firstMatching(TUnaryPredicate predicate) requires(!containsReference) {

        for (size_t i = 0; i < size(); ++i) {

            if (predicate(at(i))) {
                
                return at(i);
            }
        }

        return { };
    }

    template<typename TUnaryPredicate>
    Optional<VisibleType const&> firstMatching(TUnaryPredicate predicate) const requires(!containsReference) {

        for (size_t i = 0; i < size(); ++i) {

            if (predicate(at(i))) {

                return Optional<VisibleType const&>(at(i));
            }
        }

        return { };
    }

    ///

    template<typename TUnaryPredicate>
    Optional<VisibleType&> lastMatching(TUnaryPredicate predicate) requires(!containsReference) {

        for (ssize_t i = size() - 1; i >= 0; --i) {

            if (predicate(at(i))) {
                
                return at(i);
            }
        }

        return { };
    }

    ///

    template<typename V>
    bool operator==(V const& other) const {

        if (m_size != other.size()) {

            return false;
        }

        return TypedTransfer<StorageType>::compare(data(), other.data(), size());
    }

    template<typename V>
    bool containsSlow(V const& value) const {

        for (size_t i = 0; i < size(); ++i) {

            if (Traits<VisibleType>::equals(at(i), value)) {

                return true;
            }
        }

        return false;
    }

    bool containsInRange(VisibleType const& value, size_t const start, size_t const end) const {

        VERIFY(start <= end);
        
        VERIFY(end < size());
        
        for (size_t i = start; i <= end; ++i) {

            if (Traits<VisibleType>::equals(at(i), value)) {

                return true;
            }
        }
        
        return false;
    }

#ifndef KERNEL

    template<typename U = T>
    void insert(size_t index, U&& value) requires(CanBePlacedInsideVector<U>) {

        MUST(tryInsert<U>(index, forward<U>(value)));
    }

    template<typename TUnaryPredicate, typename U = T>
    void insertBeforeMatching(U&& value, TUnaryPredicate predicate, size_t firstIndex = 0, size_t* insertedIndex = nullptr) requires(CanBePlacedInsideVector<U>) {

        MUST(tryInsertBeforeMatching(forward<U>(value), predicate, firstIndex, insertedIndex));
    }

    void extend(Vector&& other) {

        MUST(tryExtend(move(other)));
    }

    void extend(Vector const& other) {

        MUST(tryExtend(other));
    }

#endif

    ALWAYS_INLINE void append(T&& value) {

        if constexpr (containsReference) {

            MUST(tryAppend(value));
        }
        else {

            MUST(tryAppend(move(value)));
        }
    }

    ALWAYS_INLINE void append(T const& value) requires(!containsReference) {

        MUST(tryAppend(T(value)));
    }

#ifndef KERNEL

    void append(StorageType const* values, size_t count) {
        
        MUST(tryAppend(values, count));
    }

#endif

    template<typename U = T>
    ALWAYS_INLINE void uncheckedAppend(U&& value) requires(CanBePlacedInsideVector<U>) {

        VERIFY((size() + 1) <= capacity());

        if constexpr (containsReference) {

            new (slot(m_size)) StorageType(&value);
        }
        else {

            new (slot(m_size)) StorageType(forward<U>(value));
        }

        ++m_size;
    }

    ALWAYS_INLINE void uncheckedAppend(StorageType const* values, size_t count) {

        if (count == 0) {

            return;
        }
        
        VERIFY((size() + count) <= capacity());
        
        TypedTransfer<StorageType>::copy(slot(m_size), values, count);
        
        m_size += count;
    }

#ifndef KERNEL

    template<class... Args>
    void empend(Args&&... args) requires(!containsReference) {

        MUST(tryEmpend(forward<Args>(args)...));
    }

    template<typename U = T>
    void prepend(U&& value) requires(CanBePlacedInsideVector<U>) {

        MUST(tryInsert(0, forward<U>(value)));
    }

    void prepend(Vector&& other) {

        MUST(tryPrepend(move(other)));
    }

    void prepend(StorageType const* values, size_t count) {

        MUST(tryPrepend(values, count));
    }

#endif

    // FIXME: What about assigning from a vector with lower inline capacity?

    Vector& operator=(Vector&& other) {

        if (this != &other) {

            clear();
            
            m_size = other.m_size;
            
            m_capacity = other.m_capacity;
            
            m_outlineBuffer = other.m_outlineBuffer;
            
            if constexpr (inlineCapacity > 0) {

                if (!m_outlineBuffer) {

                    for (size_t i = 0; i < m_size; ++i) {

                        new (&inlineBuffer()[i]) StorageType(move(other.inlineBuffer()[i]));

                        other.inlineBuffer()[i].~StorageType();
                    }
                }
            }

            other.m_outlineBuffer = nullptr;
            
            other.m_size = 0;
            
            other.resetCapacity();
        }

        return *this;
    }

    Vector& operator=(Vector const& other) {

        if (this != &other) {

            clear();
            
            ensureCapacity(other.size());
            
            TypedTransfer<StorageType>::copy(data(), other.data(), other.size());
            
            m_size = other.size();
        }

        return *this;
    }

    template<size_t otherInlineCapacity>
    Vector& operator=(Vector<T, otherInlineCapacity> const& other) {

        clear();
        
        ensureCapacity(other.size());
        
        TypedTransfer<StorageType>::copy(data(), other.data(), other.size());
        
        m_size = other.size();
        
        return *this;
    }

    ///

    void clear() {

        clearWithCapacity();

        if (m_outlineBuffer) {

            kfreeSized(m_outlineBuffer, m_capacity * sizeof(StorageType));

            m_outlineBuffer = nullptr;
        }

        resetCapacity();
    }

    void clearWithCapacity() {

        for (size_t i = 0; i < m_size; ++i) {

            data()[i].~StorageType();
        }

        m_size = 0;
    }

    void remove(size_t index) {

        VERIFY(index < m_size);

        if constexpr (Traits<StorageType>::isTrivial()) {
            
            TypedTransfer<StorageType>::copy(slot(index), slot(index + 1), m_size - index - 1);
        } 
        else {
            
            at(index).~StorageType();

            for (size_t i = index + 1; i < m_size; ++i) {

                new (slot(i - 1)) StorageType(move(at(i)));
                
                at(i).~StorageType();
            }
        }

        --m_size;
    }

    void remove(size_t index, size_t count) {

        if (count == 0) {

            return;
        }

        VERIFY(index + count > index);
        
        VERIFY(index + count <= m_size);

        if constexpr (Traits<StorageType>::isTrivial()) {

            TypedTransfer<StorageType>::copy(slot(index), slot(index + count), m_size - index - count);
        } 
        else {

            for (size_t i = index; i < index + count; i++) {

                at(i).~StorageType();
            }

            for (size_t i = index + count; i < m_size; ++i) {

                new (slot(i - count)) StorageType(move(at(i)));

                at(i).~StorageType();
            }
        }

        m_size -= count;
    }

    template<typename TUnaryPredicate>
    bool removeFirstMatching(TUnaryPredicate predicate) {

        for (size_t i = 0; i < size(); ++i) {

            if (predicate(at(i))) {

                remove(i);

                return true;
            }
        }

        return false;
    }

    template<typename TUnaryPredicate>
    bool removeAllMatching(TUnaryPredicate predicate) {

        bool somethingWasRemoved = false;

        for (size_t i = 0; i < size();) {

            if (predicate(at(i))) {

                remove(i);
                
                somethingWasRemoved = true;
            } 
            else {
                
                ++i;
            }
        }

        return somethingWasRemoved;
    }

    ALWAYS_INLINE T takeLast() {

        VERIFY(!isEmpty());

        auto value = move(rawLast());
        
        if constexpr (!containsReference) {

            last().~T();
        }

        --m_size;

        if constexpr (containsReference) {

            return *value;
        }
        else {

            return value;
        }
    }

    T takeFirst() {

        VERIFY(!isEmpty());
        
        auto value = move(rawFirst());

        remove(0);

        if constexpr (containsReference) {

            return *value;
        }
        else {

            return value;
        }
    }

    T take(size_t index) {

        auto value = move(rawAt(index));

        remove(index);
        
        if constexpr (containsReference) {

            return *value;
        }
        else {

            return value;
        }
    }

    T unstableTake(size_t index) {

        VERIFY(index < m_size);
        
        swap(rawAt(index), rawAt(m_size - 1));
        
        return takeLast();
    }

    template<typename U = T>
    ErrorOr<void> tryInsert(size_t index, U&& value) requires(CanBePlacedInsideVector<U>) {

        if (index > size()) {

            return Error::fromError(EINVAL);
        }
        
        if (index == size()) {

            return tryAppend(forward<U>(value));
        }

        TRY(tryGrowCapacity(size() + 1));
        
        ++m_size;

        if constexpr (Traits<StorageType>::isTrivial()) {
            TypedTransfer<StorageType>::move(slot(index + 1), slot(index), m_size - index - 1);
        } 
        else {
            for (size_t i = size() - 1; i > index; --i) {
                
                new (slot(i)) StorageType(move(at(i - 1)));
                
                at(i - 1).~StorageType();
            }
        }
        if constexpr (containsReference) {

            new (slot(index)) StorageType(&value);
        }
        else {

            new (slot(index)) StorageType(forward<U>(value));
        }

        return { };
    }

    template<typename TUnaryPredicate, typename U = T>
    ErrorOr<void> tryInsertBeforeMatching(U&& value, TUnaryPredicate predicate, size_t firstIndex = 0, size_t* insertedIndex = nullptr) requires(CanBePlacedInsideVector<U>) {

        for (size_t i = firstIndex; i < size(); ++i) {

            if (predicate(at(i))) {

                TRY(tryInsert(i, forward<U>(value)));

                if (insertedIndex) {

                    *insertedIndex = i;
                }
                
                return { };
            }
        }

        TRY(tryAppend(forward<U>(value)));

        if (insertedIndex) {

            *insertedIndex = size() - 1;
        }

        return { };
    }

    ErrorOr<void> tryExtend(Vector&& other) {

        if (isEmpty() && capacity() <= other.capacity()) {

            *this = move(other);
            
            return { };
        }

        auto otherSize = other.size();
        
        Vector tmp = move(other);
        
        TRY(tryGrowCapacity(size() + otherSize));
        
        TypedTransfer<StorageType>::move(data() + m_size, tmp.data(), otherSize);
        
        m_size += otherSize;
        
        return { };
    }

    ErrorOr<void> tryExtend(Vector const& other) {

        TRY(tryGrowCapacity(size() + other.size()));
        
        TypedTransfer<StorageType>::copy(data() + m_size, other.data(), other.size());
        
        m_size += other.m_size;
        
        return { };
    }

    ErrorOr<void> tryAppend(T&& value) {

        TRY(tryGrowCapacity(size() + 1));

        if constexpr (containsReference) {

            new (slot(m_size)) StorageType(&value);
        }
        else {

            new (slot(m_size)) StorageType(move(value));
        }
        
        ++m_size;

        return { };
    }

    ErrorOr<void> tryAppend(T const& value) requires(!containsReference) {

        return tryAppend(T(value));
    }

    ErrorOr<void> tryAppend(StorageType const* values, size_t count) {

        if (count == 0) {

            return { };
        }

        TRY(tryGrowCapacity(size() + count));
        
        TypedTransfer<StorageType>::copy(slot(m_size), values, count);
        
        m_size += count;
        
        return { };
    }

    template<class... Args>
    ErrorOr<void> tryEmpend(Args&&... args) requires(!containsReference) {

        TRY(tryGrowCapacity(m_size + 1));
        
        new (slot(m_size)) StorageType { forward<Args>(args)... };
        
        ++m_size;
        
        return { };
    }

    template<typename U = T>
    ErrorOr<void> tryPrepend(U&& value) requires(CanBePlacedInsideVector<U>) {
        
        return tryInsert(0, forward<U>(value));
    }

    ErrorOr<void> tryPrepend(Vector&& other) {

        if (other.isEmpty()) {

            return { };
        }

        if (isEmpty()) {

            *this = move(other);
            
            return { };
        }

        auto otherSize = other.size();

        TRY(tryGrowCapacity(size() + otherSize));

        for (size_t i = size() + otherSize - 1; i >= other.size(); --i) {
            
            new (slot(i)) StorageType(move(at(i - otherSize)));
            
            at(i - otherSize).~StorageType();
        }

        Vector tmp = move(other);
        
        TypedTransfer<StorageType>::move(slot(0), tmp.data(), tmp.size());
        
        m_size += otherSize;
        
        return { };
    }

    ErrorOr<void> tryPrepend(StorageType const* values, size_t count) {

        if (count == 0) {

            return { };
        }

        TRY(tryGrowCapacity(size() + count));
        
        TypedTransfer<StorageType>::move(slot(count), slot(0), m_size);
        
        TypedTransfer<StorageType>::copy(slot(0), values, count);
        
        m_size += count;
        
        return { };
    }

    ErrorOr<void> tryGrowCapacity(size_t neededCapacity) {

        if (m_capacity >= neededCapacity) {

            return { };
        }

        return tryEnsureCapacity(paddedCapacity(neededCapacity));
    }

    ErrorOr<void> tryEnsureCapacity(size_t neededCapacity) {

        if (m_capacity >= neededCapacity) {

            return { };
        }

        size_t newCapacity = kmallocGoodSize(neededCapacity * sizeof(StorageType)) / sizeof(StorageType);
        
        auto* newBuffer = static_cast<StorageType*>(kmallocArray(newCapacity, sizeof(StorageType)));

        if (newBuffer == nullptr) {

            return Error::fromError(ENOMEM);
        }

        if constexpr (Traits<StorageType>::isTrivial()) {
            
            TypedTransfer<StorageType>::copy(newBuffer, data(), m_size);
        } 
        else {

            for (size_t i = 0; i < m_size; ++i) {

                new (&newBuffer[i]) StorageType(move(at(i)));

                at(i).~StorageType();
            }
        }

        if (m_outlineBuffer) {

            kfreeSized(m_outlineBuffer, m_capacity * sizeof(StorageType));
        }
        
        m_outlineBuffer = newBuffer;

        m_capacity = newCapacity;

        return { };
    }

    ErrorOr<void> tryResize(size_t newSize, bool keepCapacity = false) requires(!containsReference) {

        if (newSize <= size()) {
            
            shrink(newSize, keepCapacity);
            
            return { };
        }

        TRY(tryEnsureCapacity(newSize));

        for (size_t i = size(); i < newSize; ++i) {

            new (slot(i)) StorageType { };
        }

        m_size = newSize;

        return { };
    }

    ErrorOr<void> tryResizeAndKeepCapacity(size_t newSize) requires(!containsReference) {

        return tryResize(newSize, true);
    }

    void growCapacity(size_t neededCapacity) {

        MUST(tryGrowCapacity(neededCapacity));
    }

    void ensureCapacity(size_t neededCapacity) {
        
        MUST(tryEnsureCapacity(neededCapacity));
    }

    void shrink(size_t newSize, bool keepCapacity = false) {

        VERIFY(newSize <= size());
        
        if (newSize == size()) {

            return;
        }

        if (newSize == 0) {

            if (keepCapacity) {

                clearWithCapacity();
            }
            else {

                clear();
            }

            return;
        }

        for (size_t i = newSize; i < size(); ++i) {

            at(i).~StorageType();
        }

        m_size = newSize;
    }

    void resize(size_t newSize, bool keepCapacity = false) requires(!containsReference) {

        MUST(tryResize(newSize, keepCapacity));
    }

    void resizeAndKeepCapacity(size_t newSize) requires(!containsReference) {

        MUST(tryResizeAndKeepCapacity(newSize));
    }

    ///

    using ConstIterator = SimpleIterator<Vector const, VisibleType const>;
    
    using Iterator = SimpleIterator<Vector, VisibleType>;
    
    using ReverseIterator = SimpleReverseIterator<Vector, VisibleType>;
    
    using ReverseConstIterator = SimpleReverseIterator<Vector const, VisibleType const>;

    ///

    ConstIterator begin() const { return ConstIterator::begin(*this); }
    
    Iterator begin() { return Iterator::begin(*this); }
    
    ///

    ReverseIterator rbegin() { return ReverseIterator::rbegin(*this); }

    ReverseConstIterator rbegin() const { return ReverseConstIterator::rbegin(*this); }

    ///

    ConstIterator end() const { return ConstIterator::end(*this); }

    Iterator end() { return Iterator::end(*this); }

    ///

    ReverseIterator rend() { return ReverseIterator::rend(*this); }
    
    ReverseConstIterator rend() const { return ReverseConstIterator::rend(*this); }

    ///

    ALWAYS_INLINE constexpr auto inReverse() {

        return ReverseWrapper::inReverse(*this);
    }

    ALWAYS_INLINE constexpr auto inReverse() const {

        return ReverseWrapper::inReverse(*this);
    }

    template<typename TUnaryPredicate>
    ConstIterator findIf(TUnaryPredicate&& finder) const {

        return ::findIf(begin(), end(), forward<TUnaryPredicate>(finder));
    }

    template<typename TUnaryPredicate>
    Iterator findIf(TUnaryPredicate&& finder) {

        return ::findIf(begin(), end(), forward<TUnaryPredicate>(finder));
    }

    ConstIterator find(VisibleType const& value) const {

        return ::find(begin(), end(), value);
    }

    Iterator find(VisibleType const& value) {

        return ::find(begin(), end(), value);
    }
    
    ///

    Optional<size_t> findFirstIndex(VisibleType const& value) const {

        if (auto const index = ::findIndex(begin(), end(), value);
            index < size()) {

            return index;
        }

        return { };
    }

    void reverse() {

        for (size_t i = 0; i < size() / 2; ++i) {

            ::swap(at(i), at(size() - i - 1));
        }
    }

private:

    void resetCapacity() {

        m_capacity = inlineCapacity;
    }

    static size_t paddedCapacity(size_t capacity) {

        return max(static_cast<size_t>(4), capacity + (capacity / 4) + 4);
    }

    StorageType* slot(size_t i) { return &data()[i]; }
    
    StorageType const* slot(size_t i) const { return &data()[i]; }

    StorageType* inlineBuffer() {

        static_assert(inlineCapacity > 0);

        return reinterpret_cast<StorageType*>(m_inlineBufferStorage);
    }

    StorageType const* inlineBuffer() const {

        static_assert(inlineCapacity > 0);

        return reinterpret_cast<StorageType const*>(m_inlineBufferStorage);
    }

    StorageType& rawLast() { return rawAat(size() - 1); }

    StorageType& rawFirst() { return rawAt(0); }
    
    StorageType& rawAt(size_t index) { return *slot(index); }

    size_t m_size { 0 };

    size_t m_capacity { 0 };

    static constexpr size_t storageSize() {

        if constexpr (inlineCapacity == 0) {

            return 0;
        }
        else {

            return sizeof(StorageType) * inlineCapacity;
        }
    }

    static constexpr size_t storageAlignment() {

        if constexpr (inlineCapacity == 0) {

            return 1;
        }
        else {

            return alignof(StorageType);
        }
    }

    alignas(storageAlignment()) unsigned char m_inlineBufferStorage[storageSize()];

    StorageType* m_outlineBuffer { nullptr };
};

template<class... Args>
Vector(Args... args) -> Vector<CommonType<Args...>>;
