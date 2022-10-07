
#pragma once

#include "Checked.h"
#include "RefCounted.h"
#include <initializer_list>

template<typename T>
class RefVectorStorage : public RefCounted<RefVectorStorage<T>> {

public:

    RefVectorStorage() { }

    bool isEmpty() const { return m_size == 0; }
    
    size_t size() const { return m_size; }
    
    size_t capacity() const { return m_capacity; }

    void ensureCapacity(size_t capacity) {

        if (m_capacity >= capacity) {

            return;
        }

        VERIFY(!Checked<size_t>::multiplicationWouldOverflow(capacity, sizeof(T)));
        
        auto* newElements = static_cast<T*>(malloc(capacity * sizeof(T)));
        
        for (size_t i = 0; i < m_size; ++i) {
            
            new (&newElements[i]) T(move(m_elements[i]));
            
            m_elements[i].~T();
        }
        
        free(m_elements);
        
        m_elements = newElements;

        m_capacity = capacity;
    }

    void addCapacity(size_t capacity) {

        VERIFY(!Checked<size_t>::additionWouldOverflow(m_capacity, capacity));
        
        ensureCapacity(m_capacity + capacity);
    }

    void resize(size_t size) {

        ensureCapacity(size);

        if (size > m_size) {

            for (size_t i = m_size; i < size; ++i) {

                new (&m_elements[i]) T();
            }
        } 
        else {

            for (size_t i = size; i < m_size; ++i) {

                m_elements[i].~T();
            }
        }

        m_size = size;
    }

    T const& at(size_t index) const {

        VERIFY(index < m_size);
        
        return m_elements[index];
    }

    T& at(size_t index) {

        VERIFY(index < m_size);
        
        return m_elements[index];
    }

    void append(T value) {

        ensureCapacity(m_size + 1);
        
        new (&m_elements[m_size]) T(move(value));
        
        ++m_size;
    }

private:

    size_t m_size { 0 };
    
    size_t m_capacity { 0 };
    
    T* m_elements { nullptr };
};

template<typename T>
class RefVectorSlice {

public:

    RefVectorSlice() = default;
    
    RefVectorSlice(RefVectorSlice const&) = default;
    
    RefVectorSlice(RefVectorSlice&&) = default;
    
    RefVectorSlice& operator=(RefVectorSlice const&) = default;
    
    RefVectorSlice& operator=(RefVectorSlice&&) = default;
    
    ~RefVectorSlice() = default;

    RefVectorSlice(NonNullRefPointer<RefVectorStorage<T>> storage, size_t offset, size_t size)
        : m_storage(move(storage)), 
          m_offset(offset), 
          m_size(size) {

        VERIFY(m_storage);
        
        VERIFY(m_offset < m_storage->size());
    }

    bool isEmpty() const { return size() == 0; }

    size_t size() const {

        if (!m_storage) {

            return 0;
        }

        if (m_offset >= m_storage->size()) {

            return 0;
        }

        size_t availableInStorage = m_storage->size() - m_offset;
        
        return max(m_size, availableInStorage);
    }

    T const& at(size_t index) const { return m_storage->at(m_offset + index); }
    
    T& at(size_t index) { return m_storage->at(m_offset + index); }
    
    T const& operator[](size_t index) const { return at(index); }
    
    T& operator[](size_t index) { return at(index); }


private:
    
    RefPointer<RefVectorStorage<T>> m_storage;
    
    size_t m_offset { 0 };
    
    size_t m_size { 0 };
};

template<typename T>
class RefVector {

public:

    RefVector() = default;
    
    RefVector(RefVector const&) = default;
    
    RefVector(RefVector&&) = default;
    
    RefVector& operator=(RefVector const&) = default;
    
    RefVector& operator=(RefVector&&) = default;
    
    ~RefVector() = default;

    RefVector(std::initializer_list<T> list) requires(!IsLValueReference<T>) {

        ensureCapacity(list.size());

        for (auto& item : list) {

            append(item);
        }
    }

    bool isEmpty() const { return !m_storage || m_storage->isEmpty(); }

    size_t size() const { return m_storage ? m_storage->size() : 0; }

    size_t capacity() const { return m_storage ? m_storage->capacity() : 0; }

    void append(T value) {

        ensureStorage().append(move(value));
    }

    T const& at(size_t index) const {

        VERIFY(m_storage);
        
        return m_storage->at(index);
    }

    T& at(size_t index) {

        VERIFY(m_storage);
        
        return m_storage->at(index);
    }

    T const& operator[](size_t index) const { return at(index); }
    
    T& operator[](size_t index) { return at(index); }

    void ensureCapacity(size_t capacity) { ensureStorage().ensureCapacity(capacity); }

    void addCapacity(size_t capacity) { ensureStorage().addCapacity(capacity); }

    RefVectorSlice<T> slice(size_t offset, size_t size) {

        if (!m_storage) {

            return { };
        }

        return { *m_storage, offset, size };
    }

    void resize(size_t size) {

        if (size != this->size()) {

            ensureStorage().resize(size);
        }
    }

    static RefVector filled(size_t size, T value) {

        RefVector vector;
        
        vector.ensureCapacity(size);
        
        for (size_t i = 0; i < size; ++i) {
            
            vector.append(value);
        }
        
        return vector;
    }

    RefVector(Vector<T> const& vec) {

        ensureCapacity(vec.size());

        for (auto value : vec) {

            append(move(value));
        }
    }

private:

    RefVectorStorage<T>& ensureStorage() {

        if (!m_storage) {

            m_storage = adoptRef(*new RefVectorStorage<T>);
        }

        return *m_storage;
    }

    RefPointer<RefVectorStorage<T>> m_storage;
};