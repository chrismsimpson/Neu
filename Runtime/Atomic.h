
#pragma once

#include "Concepts.h"
#include "Platform.h"
#include "Types.h"

static inline void atomicSignalFence(MemoryOrder order) noexcept {

    return __atomic_signal_fence(order);
}

static inline void atomicThreadFence(MemoryOrder order) noexcept {

    return __atomic_thread_fence(order);
}

static inline void fullMemoryBarrier() noexcept {

    atomicSignalFence(MemoryOrder::memory_order_acq_rel);

    atomicThreadFence(MemoryOrder::memory_order_acq_rel);
}

template<typename T>
static inline T atomicExchange(volatile T* var, T desired, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_exchange_n(var, desired, order);
}

template<typename T, typename V = RemoveVolatile<T>>
static inline V* atomicExchange(volatile T** var, V* desired, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_exchange_n(var, desired, order);
}

template<typename T, typename V = RemoveVolatile<T>>
static inline V* atomicExchange(volatile T** var, std::nullptr_t, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_exchange_n(const_cast<V**>(var), nullptr, order);
}

template<typename T>
[[nodiscard]] static inline bool atomicCompareExchangeStrong(volatile T* var, T& expected, T desired, MemoryOrder order = memory_order_seq_cst) noexcept {

    if (order == memory_order_acq_rel || order == memory_order_release) {

        return __atomic_compare_exchange_n(var, &expected, desired, false, memory_order_release, memory_order_acquire);
    }

    return __atomic_compare_exchange_n(var, &expected, desired, false, order, order);
}


template<typename T, typename V = RemoveVolatile<T>>
[[nodiscard]] static inline bool atomicCompareExchangeStrong(volatile T** var, V*& expected, V* desired, MemoryOrder order = memory_order_seq_cst) noexcept {

    if (order == memory_order_acq_rel || order == memory_order_release) {

        return __atomic_compare_exchange_n(var, &expected, desired, false, memory_order_release, memory_order_acquire);
    }

    return __atomic_compare_exchange_n(var, &expected, desired, false, order, order);
}

template<typename T, typename V = RemoveVolatile<T>>
[[nodiscard]] static inline bool atomicCompareExchangeStrong(volatile T** var, V*& expected, std::nullptr_t, MemoryOrder order = memory_order_seq_cst) noexcept {

    if (order == memory_order_acq_rel || order == memory_order_release) {

        return __atomic_compare_exchange_n(const_cast<V**>(var), &expected, nullptr, false, memory_order_release, memory_order_acquire);
    }

    return __atomic_compare_exchange_n(const_cast<V**>(var), &expected, nullptr, false, order, order);
}

///

template<typename T>
static inline T atomicFetchAdd(volatile T* var, T val, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_fetch_add(var, val, order);
}

template<typename T>
static inline T atomicFetchSub(volatile T* var, T val, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_fetch_sub(var, val, order);
}

template<typename T>
static inline T atomicFetchAnd(volatile T* var, T val, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_fetch_and(var, val, order);
}

template<typename T>
static inline T atomicFetchOr(volatile T* var, T val, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_fetch_or(var, val, order);
}

template<typename T>
static inline T atomicFetchXor(volatile T* var, T val, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_fetch_xor(var, val, order);
}

///

template<typename T>
static inline T atomicLoad(volatile T* var, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_load_n(var, order);
}

template<typename T, typename V = RemoveVolatile<T>>
static inline V* atomicLoad(volatile T** var, MemoryOrder order = memory_order_seq_cst) noexcept {

    return __atomic_load_n(const_cast<V**>(var), order);
}

///

template<typename T>
static inline void atomicStore(volatile T* var, T desired, MemoryOrder order = memory_order_seq_cst) noexcept {

    __atomic_store_n(var, desired, order);
}

template<typename T, typename V = RemoveVolatile<T>>
static inline void atomicStore(volatile T** var, V* desired, MemoryOrder order = memory_order_seq_cst) noexcept {

    __atomic_store_n(var, desired, order);
}

template<typename T, typename V = RemoveVolatile<T>>
static inline void atomicStore(volatile T** var, std::nullptr_t, MemoryOrder order = memory_order_seq_cst) noexcept {

    __atomic_store_n(const_cast<V**>(var), nullptr, order);
}

template<typename T>
static inline bool atomicIsLockFree(volatile T* ptr = nullptr) noexcept {

    return __atomic_is_lock_free(sizeof(T), ptr);
}

///

template<typename T, MemoryOrder DefaultMemoryOrder = MemoryOrder::memory_order_seq_cst>
class Atomic {

    // FIXME: This should work through concepts/requires clauses, but according to the compiler,
    //        "IsIntegral is not more specialized than IsFundamental".
    //        Additionally, Enums are not fundamental types except that they behave like them in every observable way.
    
    static_assert(IsFundamental<T> | IsEnum<T>, "Atomic doesn't support non-primitive types, because it relies on compiler intrinsics. If you put non-primitives into it, you'll get linker errors like \"undefined reference to __atomic_store\".");
    
    T m_value { 0 };

public:

    Atomic() noexcept = default;

    Atomic& operator=(Atomic const&) volatile = delete;
    
    Atomic& operator=(Atomic&&) volatile = delete;
    
    Atomic(Atomic const&) = delete;
    
    Atomic(Atomic&&) = delete;

    constexpr Atomic(T val) noexcept
        : m_value(val) { }

    ///

    volatile T* pointer() noexcept {

        return &m_value;
    }

    ///

    T exchange(T desired, MemoryOrder order = DefaultMemoryOrder) volatile noexcept {

        // We use this hack to prevent unnecessary initialization, even if T has a default constructor.
        // NOTE: Will need to investigate if it pessimizes the generated assembly.
        
        alignas(T) UInt8 buffer[sizeof(T)];
        
        T* ret = reinterpret_cast<T*>(buffer);
        
        __atomic_exchange(&m_value, &desired, ret, order);
        
        return *ret;
    }

    [[nodiscard]] bool compareExchangeStrong(T& expected, T desired, MemoryOrder order = DefaultMemoryOrder) volatile noexcept {

        if (order == memory_order_acq_rel || order == memory_order_release) {

            return __atomic_compare_exchange(&m_value, &expected, &desired, false, memory_order_release, memory_order_acquire);
        }

        return __atomic_compare_exchange(&m_value, &expected, &desired, false, order, order);
    }

    ALWAYS_INLINE operator T() const volatile noexcept {

        return load();
    }

    ALWAYS_INLINE T load(MemoryOrder order = DefaultMemoryOrder) const volatile noexcept {

        alignas(T) UInt8 buffer[sizeof(T)];
        
        T* ret = reinterpret_cast<T*>(buffer);
        
        __atomic_load(&m_value, ret, order);
        
        return *ret;
    }

};
