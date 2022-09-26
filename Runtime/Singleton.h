
#pragma once

#include "Assertions.h"
#include "Atomic.h"
#include "NonCopyable.h"

#ifndef __os__
#    include <new>
#endif

template<typename T>
struct SingletonInstanceCreator {

    static T* create() {

        return new T();
    }
};

template<typename T, T* (*InitFunction)() = SingletonInstanceCreator<T>::create>
class Singleton {

    MAKE_NONCOPYABLE(Singleton);
    
    MAKE_NONMOVABLE(Singleton);

public:

    Singleton() = default;


    template<bool allowCreate = true>
    static T* get(Atomic<T*>& objVar) {

        T* obj = objVar.load(::memory_order_acquire);

        
        if (FlatPointer(obj) <= 0x1) {

            // If this is the first time, see if we get to initialize it

            // if OS

            if constexpr (allowCreate) {

                if (obj == nullptr && objVar.compareExchangeStrong(obj, (T*) 0x1, ::memory_order_acq_rel)) {
                    
                    // We're the first one
                    
                    obj = InitFunction();
                    
                    objVar.store(obj, ::memory_order_release);
                    
                    return obj;
                }
            }
            
            // Someone else was faster, wait until they're done
            
            while (obj == (T*)0x1) {
            
                // if OS
            
                obj = objVar.load(::memory_order_acquire);
            }

            if constexpr (allowCreate) {

                // We should always return an instance if we allow creating one

                VERIFY(obj != nullptr);
            }

            VERIFY(obj != (T*) 0x1);
        }

        return obj;
    }

    T* pointer() const {

        return get(m_obj);
    }

    T* operator->() const {

        return pointer();
    }

    T& operator*() const {

        return *pointer();
    }

    operator T*() const {

        return pointer();
    }

    operator T&() const {

        return *pointer();
    }

    bool isInitialized() const {

        T* obj = m_obj.load(MemoryOrder::memory_order_consume);

        return FlatPointer(obj) > 0x1;
    }

    void ensureInstance() {

        pointer();
    }

private:
    
    mutable Atomic<T*> m_obj { nullptr };
};