
#pragma once

#include "std.h"
#include "TypeList.h"

namespace Detail {

    template<typename... Ts>
    struct Tuple { };

    ///

    template<typename T>
    struct Tuple<T> {

        Tuple(T&& value) requires(!IsSame<T&&, const T&>)
            : value(forward<T>(value)) { }

        Tuple(const T& value)
            : value(value) { }

        template<typename U>
        U& get() {

            static_assert(IsSame<T, U>, "Invalid tuple access");
            
            return value;
        }

        template<typename U>
        const U& get() const {

            return const_cast<Tuple<T>&>(*this).get<U>();
        }

        template<typename U, unsigned index>
        U& getWithIndex() {

            static_assert(IsSame<T, U> && index == 0, "Invalid tuple access");
            
            return value;
        }

        template<typename U, unsigned index>
        const U& getWithIndex() const {

            return const_cast<Tuple<T>&>(*this).getWithIndex<U, index>();
        }

    private:

        T value;
    };

    ///

    template<typename T, typename... TRest>
    struct Tuple<T, TRest...> : Tuple<TRest...> {

        template<typename FirstT, typename... RestT>
        Tuple(FirstT&& first, RestT&&... rest)
            : Tuple<TRest...>(forward<RestT>(rest)...), 
              value(forward<FirstT>(first)) { }

        Tuple(T&& first, TRest&&... rest)
            : Tuple<TRest...>(move(rest)...), 
              value(move(first)) { }

        template<typename U>
        U& get() {

            if constexpr (IsSame<T, U>) {

                return value;
            }
            else {

                return Tuple<TRest...>::template get<U>();
            }
        }

        template<typename U>
        const U& get() const {

            return const_cast<Tuple<T, TRest...>&>(*this).get<U>();
        }

        template<typename U, unsigned index>
        U& getWithIndex() {

            if constexpr (IsSame<T, U> && index == 0) {

                return value;
            }
            else {

                return Tuple<TRest...>::template getWithIndex<U, index - 1>();
            }
        }

        template<typename U, unsigned index>
        const U& getWithIndex() const {

            return const_cast<Tuple<T, TRest...>&>(*this).getWithIndex<U, index>();
        }

    private:

        T value;
    };
}

///

template<typename... Ts>
struct Tuple : Detail::Tuple<Ts...> {

    using Types = TypeList<Ts...>;
    
    using Detail::Tuple<Ts...>::Tuple;
    
    using Indices = MakeIndexSequence<sizeof...(Ts)>;

    Tuple(Tuple&& other)
        : Tuple(move(other), Indices()) { }

    Tuple(Tuple const& other)
        : Tuple(other, Indices()) { }

    Tuple& operator=(Tuple&& other) {
        
        set(move(other), Indices());

        return *this;
    }

    Tuple& operator=(Tuple const& other) {

        set(other, Indices());
        
        return *this;
    }

    template<typename T>
    auto& get() {

        return Detail::Tuple<Ts...>::template get<T>();
    }

    template<unsigned index>
    auto& get() {

        return Detail::Tuple<Ts...>::template getWithIndex<typename Types::template Type<index>, index>();
    }

    template<typename T>
    auto& get() const {

        return Detail::Tuple<Ts...>::template get<T>();
    }

    template<unsigned index>
    auto& get() const {

        return Detail::Tuple<Ts...>::template getWithIndex<typename Types::template Type<index>, index>();
    }

    template<typename F>
    auto applyAsArgs(F&& f) {

        return applyAsArgs(forward<F>(f), Indices());
    }

    template<typename F>
    auto applyAsArgs(F&& f) const {

        return applyAsArgs(forward<F>(f), Indices());
    }

    static constexpr auto size() { return sizeof...(Ts); }

private:

    template<unsigned... Is>
    Tuple(Tuple&& other, IndexSequence<Is...>)
        : Detail::Tuple<Ts...>(move(other.get<Is>())...) { }

    template<unsigned... Is>
    Tuple(Tuple const& other, IndexSequence<Is...>)
        : Detail::Tuple<Ts...>(other.get<Is>()...) { }

    template<unsigned... Is>
    void set(Tuple&& other, IndexSequence<Is...>) {

        ((get<Is>() = move(other.get<Is>())), ...);
    }

    template<unsigned... Is>
    void set(Tuple const& other, IndexSequence<Is...>) {

        ((get<Is>() = other.get<Is>()), ...);
    }

    template<typename F, unsigned... Is>
    auto applyAsArgs(F&& f, IndexSequence<Is...>) {

        return forward<F>(f)(get<Is>()...);
    }

    template<typename F, unsigned... Is>
    auto applyAsArgs(F&& f, IndexSequence<Is...>) const {

        return forward<F>(f)(get<Is>()...);
    }
};

///

template<class... Args>
Tuple(Args... args) -> Tuple<Args...>;