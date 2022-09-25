
#pragma once

#include "Assertions.h"
#include "Error.h"
#include "Find.h"
#include "Forward.h"
#include "Iterator.h"
#include "Optional.h"
#include "ReverseIterator.h"
#include "Span.h"
#include "std.h"
#include "Traits.h"
#include "TypedTransfer.h"
#include "kmalloc.h"
#include <initializer_list>

namespace Detail {

    template<typename StorageType, bool>
    struct CanBePlacedInsideVectorHelper;


    template<typename StorageType>
    struct CanBePlacedInsideVectorHelper<StorageType, true> {

        template<typename U>
        static constexpr bool value = requires(U&& u) { StorageType { &u }; };
    };


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

            // ensure_capacity(list.size());

            // for (auto& item : list) {

            //     unchecked_append(item);
            // }
        }



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

    #ifndef OS

        template<typename U = T>
        void insert(size_t index, U&& value) requires(CanBePlacedInsideVector<U>) {

            MUST(tryInsert<U>(index, forward<U>(value)));
        }

        // template<typename TUnaryPredicate, typename U = T>
        // void insert_before_matching(U&& value, TUnaryPredicate predicate, size_t first_index = 0, size_t* inserted_index = nullptr) requires(CanBePlacedInsideVector<U>) {

        //     MUST(try_insert_before_matching(forward<U>(value), predicate, first_index, inserted_index));
        // }

        // void extend(Vector&& other) {

        //     MUST(try_extend(move(other)));
        // }

        // void extend(Vector const& other) {

        //     MUST(try_extend(other));
        // }

    #endif

        using ConstIterator = SimpleIterator<Vector const, VisibleType const>;
        
        using Iterator = SimpleIterator<Vector, VisibleType>;
        
        // using ReverseIterator = SimpleReverseIterator<Vector, VisibleType>;
        
        // using ReverseConstIterator = SimpleReverseIterator<Vector const, VisibleType const>;

        // ConstIterator begin() const { return ConstIterator::begin(*this); }
        // Iterator begin() { return Iterator::begin(*this); }
        // ReverseIterator rbegin() { return ReverseIterator::rbegin(*this); }
        // ReverseConstIterator rbegin() const { return ReverseConstIterator::rbegin(*this); }

        // ConstIterator end() const { return ConstIterator::end(*this); }
        // Iterator end() { return Iterator::end(*this); }
        // ReverseIterator rend() { return ReverseIterator::rend(*this); }
        // ReverseConstIterator rend() const { return ReverseConstIterator::rend(*this); }

        // ALWAYS_INLINE constexpr auto inReverse() {

        //     return ReverseWrapper::inReverse(*this);
        // }

        // ALWAYS_INLINE constexpr auto inReverse() const {

        //     return ReverseWrapper::inReverse(*this);
        // }

        // template<typename TUnaryPredicate>
        // ConstIterator findIf(TUnaryPredicate&& finder) const {

        //     return ::findIf(begin(), end(), forward<TUnaryPredicate>(finder));
        // }

        // template<typename TUnaryPredicate>
        // Iterator findIf(TUnaryPredicate&& finder) {

        //     return ::findIf(begin(), end(), forward<TUnaryPredicate>(finder));
        // }


        // ConstIterator find(VisibleType const& value) const {

        //     return ::find(begin(), end(), value);
        // }

        // Iterator find(VisibleType const& value) {

        //     return ::find(begin(), end(), value);
        // }
        
        ///

        // Optional<size_t> findFirstIndex(VisibleType const& value) const {

        //     if (auto const index = ::findIndex(begin(), end(), value);
        //         index < size()) {

        //         return index;
        //     }

        //     return { };
        // }

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
}