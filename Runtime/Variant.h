
#pragma once

#include "Array.h"
#include "BitCast.h"
#include "std.h"
#include "TypeList.h"

namespace Detail {

    template<typename T, typename IndexType, IndexType InitialIndex, typename... InTypes>
    struct VariantIndexOf {

        static_assert(DependentFalse<T, IndexType, InTypes...>, "Invalid VariantIndex instantiated");
    };

    ///

    template<typename T, typename IndexType, IndexType InitialIndex, typename InType, typename... RestOfInTypes>
    struct VariantIndexOf<T, IndexType, InitialIndex, InType, RestOfInTypes...> {

        consteval IndexType operator()() {

            if constexpr (IsSame<T, InType>) {

                return InitialIndex;
            }
            else {

                return VariantIndexOf<T, IndexType, InitialIndex + 1, RestOfInTypes...> {}();
            }
        }
    };

    ///

    template<typename T, typename IndexType, IndexType InitialIndex>
    struct VariantIndexOf<T, IndexType, InitialIndex> {

        consteval IndexType operator()() { return InitialIndex; }
    };

    ///

    template<typename T, typename IndexType, typename... Ts>
    consteval IndexType indexOf() {

        return VariantIndexOf<T, IndexType, 0, Ts...> {}();
    }

    ///

    template<typename IndexType, IndexType InitialIndex, typename... Ts>
    struct Variant;

    ///

    template<typename IndexType, IndexType InitialIndex, typename F, typename... Ts>
    struct Variant<IndexType, InitialIndex, F, Ts...> {

        static constexpr auto currentIndex = VariantIndexOf<F, IndexType, InitialIndex, F, Ts...> { }();

        ///
        
        ALWAYS_INLINE static void delete_(IndexType id, void* data) {

            if (id == currentIndex) {

                bitCast<F*>(data)->~F();
            }
            else {

                Variant<IndexType, InitialIndex + 1, Ts...>::delete_(id, data);
            }
        }

        ///

        ALWAYS_INLINE static void move_(IndexType oldId, void* oldData, void* newData) {

            if (oldId == currentIndex) {

                new (newData) F(move(*bitCast<F*>(oldData)));
            }
            else {

                Variant<IndexType, InitialIndex + 1, Ts...>::move_(oldId, oldData, newData);
            }
        }

        ///

        ALWAYS_INLINE static void copy_(IndexType oldId, void const* oldData, void* newData) {

            if (oldId == currentIndex) {

                new (newData) F(*bitCast<F const*>(oldData));
            }
            else {

                Variant<IndexType, InitialIndex + 1, Ts...>::copy_(oldId, oldData, newData);
            }
        }
    };

    ///

    template<typename IndexType, IndexType InitialIndex>
    struct Variant<IndexType, InitialIndex> {

        ALWAYS_INLINE static void delete_(IndexType, void*) { }
        
        ALWAYS_INLINE static void move_(IndexType, void*, void*) { }
        
        ALWAYS_INLINE static void copy_(IndexType, void const*, void*) { }
    };

    ///

    template<typename IndexType, typename... Ts>
    struct VisitImpl {

        template<typename RT, typename T, size_t I, typename Fn>
        static constexpr bool hasExplicitlyNamedOverload() {

            // If we're not allowed to make a member function pointer and call it directly (without explicitly resolving it),
            // we have a templated function on our hands (or a function overload set).
            // in such cases, we don't have an explicitly named overload, and we would have to select it.
            
            return requires { (declval<Fn>().*(&Fn::operator()))(declval<T>()); };
        }

        ///

        template<typename ReturnType, typename T, typename Visitor, auto... Is>
        static constexpr bool shouldInvokeConstOverload(IndexSequence<Is...>) {

            // Scan over all the different visitor functions, if none of them are suitable for calling with `T const&`, avoid calling that first.
            
            return ((hasExplicitlyNamedOverload<ReturnType, T, Is, typename Visitor::Types::template Type<Is>>()) || ...);
        }

        ///

        template<typename Self, typename Visitor, IndexType CurrentIndex = 0>
        ALWAYS_INLINE static constexpr decltype(auto) visit(Self& self, IndexType id, void const* data, Visitor&& visitor) requires(CurrentIndex < sizeof...(Ts)) {

            using T = typename TypeList<Ts...>::template Type<CurrentIndex>;

            if (id == CurrentIndex) {

                // Check if Visitor::operator() is an explicitly typed function (as opposed to a templated function)
                // if so, try to call that with `T const&` first before copying the Variant's const-ness.
                // This emulates normal C++ call semantics where templated functions are considered last, after all non-templated overloads
                // are checked and found to be unusable.

                using ReturnType = decltype(visitor(*bitCast<T*>(data)));

                if constexpr (shouldInvokeConstOverload<ReturnType, T, Visitor>(MakeIndexSequence<Visitor::Types::size>())) {

                    return visitor(*bitCast<AddConst<T>*>(data));
                }

                return visitor(*bitCast<CopyConst<Self, T>*>(data));
            }

            if constexpr ((CurrentIndex + 1) < sizeof...(Ts)) {

                return visit<Self, Visitor, CurrentIndex + 1>(self, id, data, forward<Visitor>(visitor));
            }
            else {

                VERIFY_NOT_REACHED();
            }
        }
    };

    ///

    struct VariantNoClearTag {

        explicit VariantNoClearTag() = default;
    };

    ///

    struct VariantConstructTag {
        
        explicit VariantConstructTag() = default;
    };

    ///

    template<typename T, typename Base>
    struct VariantConstructors {

        ALWAYS_INLINE VariantConstructors(T&& t) requires(requires { T(move(t)); }) {

            internalCast().clearWithoutDestruction();
            
            internalCast().set(move(t), VariantNoClearTag {});
        }

        ALWAYS_INLINE VariantConstructors(const T& t) requires(requires { T(t); }) {

            internalCast().clearWithoutDestruction();
            
            internalCast().set(t, VariantNoClearTag {});
        }

        ALWAYS_INLINE VariantConstructors() = default;

    private:

        [[nodiscard]] ALWAYS_INLINE Base& internalCast() {

            // Warning: Internal type shenanigans - VariantsConstrutors<T, Base> <- Base
            //          Not the other way around, so be _really_ careful not to cause issues.

            return *reinterpret_cast<Base*>(this);
        }
    };

    ///

    // Type list deduplication
    // Since this is a big template mess, each template is commented with how and why it works.

    struct ParameterPackTag { };

    ///

    // Pack<Ts...> is just a way to pass around the type parameter pack Ts

    template<typename... Ts>
    struct ParameterPack : ParameterPackTag { };

    ///

    // Blank<T> is a unique replacement for T, if T is a duplicate type.
    template<typename T>
    struct Blank { };

    ///

    template<typename A, typename P>
    inline constexpr bool IsTypeInPack = false;

    ///

    // IsTypeInPack<T, Pack<Ts...>> will just return whether 'T' exists in 'Ts'.

    template<typename T, typename... Ts>
    inline constexpr bool IsTypeInPack<T, ParameterPack<Ts...>> = (IsSame<T, Ts> || ...);

    ///

    // Replaces T with Blank<T> if it exists in Qs.

    template<typename T, typename... Qs>
    using BlankIfDuplicate = Conditional<(IsTypeInPack<T, Qs> || ...), Blank<T>, T>;

    ///

    template<unsigned I, typename...>
    struct InheritFromUniqueEntries;

    ///

    // InheritFromUniqueEntries will inherit from both Qs and Ts, but only scan entries going *forwards*
    // that is to say, if it's scanning from index I in Qs, it won't scan for duplicates for entries before I
    // as that has already been checked before.
    // This makes sure that the search is linear in time (like the 'merge' step of merge sort).

    template<unsigned I, typename... Ts, unsigned... Js, typename... Qs>
    struct InheritFromUniqueEntries<I, ParameterPack<Ts...>, IndexSequence<Js...>, Qs...>
        : public BlankIfDuplicate<Ts, Conditional<Js <= I, ParameterPack<>, Qs>...>... {

        using BlankIfDuplicate<Ts, Conditional<Js <= I, ParameterPack<>, Qs>...>::BlankIfDuplicate...;
    };

    ///

    template<typename...>
    struct InheritFromPacks;

    ///

    // InheritFromPacks will attempt to 'merge' the pack 'Ps' with *itself*, but skip the duplicate entries
    // (via InheritFromUniqueEntries).

    template<unsigned... Is, typename... Ps>
    struct InheritFromPacks<IndexSequence<Is...>, Ps...>
        : public InheritFromUniqueEntries<Is, Ps, IndexSequence<Is...>, Ps...>... {

        using InheritFromUniqueEntries<Is, Ps, IndexSequence<Is...>, Ps...>::InheritFromUniqueEntries...;
    };

    // Just a nice wrapper around InheritFromPacks, which will wrap any parameter packs in ParameterPack (unless it already is one).

    template<typename... Ps>
    using MergeAndDeduplicatePacks = InheritFromPacks<MakeIndexSequence<sizeof...(Ps)>, Conditional<IsBaseOf<ParameterPackTag, Ps>, Ps, ParameterPack<Ps>>...>;
}

///

struct Empty { };

///

template<typename... Ts>
struct Variant
    : public Detail::MergeAndDeduplicatePacks<Detail::VariantConstructors<Ts, Variant<Ts...>>...> {

private:

    using IndexType = Conditional<sizeof...(Ts) < 255, UInt8, size_t>; // Note: size+1 reserved for internal value checks
    
    static constexpr IndexType invalidIndex = sizeof...(Ts);

    template<typename T>
    static constexpr IndexType indexOf() { return Detail::indexOf<T, IndexType, Ts...>(); }

public:

    template<typename T>
    static constexpr bool canContain() {

        return indexOf<T>() != invalidIndex;
    }

    template<typename... NewTs>
    Variant(Variant<NewTs...>&& old) requires((canContain<NewTs>() && ...))
        : Variant(move(old).template downcast<Ts...>()) { }

    template<typename... NewTs>
    Variant(Variant<NewTs...> const& old) requires((canContain<NewTs>() && ...))
        : Variant(old.template downcast<Ts...>()) { }

    template<typename... NewTs>
    friend struct Variant;

    Variant() requires(!canContain<Empty>()) = delete;

    Variant() requires(canContain<Empty>())
        : Variant(Empty()) { }


#ifdef HAS_CONDITIONALLY_TRIVIAL

    Variant(Variant const&) requires(!(IsCopyConstructible<Ts> && ...)) = delete;

    Variant(Variant const&) = default;

    Variant(Variant&&) requires(!(IsMoveConstructible<Ts> && ...)) = delete;

    Variant(Variant&&) = default;

    ~Variant() requires(!(IsDestructible<Ts> && ...)) = delete;

    ~Variant() = default;

    Variant& operator=(Variant const&) requires(!(IsCopyConstructible<Ts> && ...) || !(IsDestructible<Ts> && ...)) = delete;

    Variant& operator=(Variant const&) = default;

    Variant& operator=(Variant&&) requires(!(IsMoveConstructible<Ts> && ...) || !(IsDestructible<Ts> && ...)) = delete;

    Variant& operator=(Variant&&) = default;

#endif

    ALWAYS_INLINE Variant(Variant const& old)
#ifdef HAS_CONDITIONALLY_TRIVIAL
        requires(!(IsTriviallyCopyConstructible<Ts> && ...))
#endif
        : Detail::MergeAndDeduplicatePacks<Detail::VariantConstructors<Ts, Variant<Ts...>>...>(), 
          m_data { }, 
          m_index(old.m_index) {

        Helper::copy_(old.m_index, old.m_data, m_data);
    }

    // Note: A moved-from variant emulates the state of the object it contains
    //       so if a variant containing an int is moved from, it will still contain that int
    //       and if a variant with a nontrivial move ctor is moved from, it may or may not be valid
    //       but it will still contain the "moved-from" state of the object it previously contained.
    ALWAYS_INLINE Variant(Variant&& old)
#ifdef HAS_CONDITIONALLY_TRIVIAL
        requires(!(IsTriviallyMoveConstructible<Ts> && ...))
#endif
        : Detail::MergeAndDeduplicatePacks<Detail::VariantConstructors<Ts, Variant<Ts...>>...>(), 
          m_index(old.m_index)
    {
        Helper::move_(old.m_index, old.m_data, m_data);
    }

    ALWAYS_INLINE ~Variant()
#ifdef HAS_CONDITIONALLY_TRIVIAL
        requires(!(IsTriviallyDestructible<Ts> && ...))
#endif
    {
        Helper::delete_(m_index, m_data);
    }

    ALWAYS_INLINE Variant& operator=(Variant const& other)
#ifdef HAS_CONDITIONALLY_TRIVIAL
        requires(!(IsTriviallyCopyConstructible<Ts> && ...) || !(IsTriviallyDestructible<Ts> && ...))
#endif
    {
        if (this != &other) {
            
            if constexpr (!(IsTriviallyDestructible<Ts> && ...)) {

                Helper::delete_(m_index, m_data);
            }
            
            m_index = other.m_index;

            Helper::copy_(other.m_index, other.m_data, m_data);
        }

        return *this;
    }

    ALWAYS_INLINE Variant& operator=(Variant&& other)
#ifdef HAS_CONDITIONALLY_TRIVIAL
        requires(!(IsTriviallyMoveConstructible<Ts> && ...) || !(IsTriviallyDestructible<Ts> && ...))
#endif
    {
        if (this != &other) {

            if constexpr (!(IsTriviallyDestructible<Ts> && ...)) {

                Helper::delete_(m_index, m_data);
            }

            m_index = other.m_index;

            Helper::move_(other.m_index, other.m_data, m_data);
        }

        return *this;
    }

    using Detail::MergeAndDeduplicatePacks<Detail::VariantConstructors<Ts, Variant<Ts...>>...>::MergeAndDeduplicatePacks;

    template<typename T, typename StrippedT = RemoveConstVolatileReference<T>>
    void set(T&& t) requires(canContain<StrippedT>() && requires { StrippedT(forward<T>(t)); }) {

        constexpr auto newIndex = index_of<StrippedT>();

        Helper::delete_(m_index, m_data);

        new (m_data) StrippedT(forward<T>(t));

        m_index = newIndex;
    }

    template<typename T, typename StrippedT = RemoveConstVolatileReference<T>>
    void set(T&& t, Detail::VariantNoClearTag) requires(canContain<StrippedT>() && requires { StrippedT(forward<T>(t)); }) {

        constexpr auto newIndex = index_of<StrippedT>();

        new (m_data) StrippedT(forward<T>(t));
        
        m_index = newIndex;
    }

    template<typename T>
    T* getPointer() requires(canContain<T>()) {

        if (index_of<T>() == m_index) {

            return bitCast<T*>(&m_data);
        }

        return nullptr;
    }

    template<typename T>
    T& get() requires(canContain<T>()) {

        VERIFY(has<T>());

        return *bitCast<T*>(&m_data);
    }

    template<typename T>
    const T* getPointer() const requires(canContain<T>()) {

        if (indexOf<T>() == m_index) {

            return bitCast<const T*>(&m_data);
        }

        return nullptr;
    }

    template<typename T>
    const T& get() const requires(canContain<T>()) {

        VERIFY(has<T>());

        return *bitCast<const T*>(&m_data);
    }

    template<typename T>
    [[nodiscard]] bool has() const requires(canContain<T>()) {

        return indexOf<T>() == m_index;
    }

    template<typename... Fs>
    ALWAYS_INLINE decltype(auto) visit(Fs&&... functions) {

        Visitor<Fs...> visitor { forward<Fs>(functions)... };
        
        return VisitHelper::visit(*this, m_index, m_data, move(visitor));
    }

    template<typename... Fs>
    ALWAYS_INLINE decltype(auto) visit(Fs&&... functions) const {

        Visitor<Fs...> visitor { forward<Fs>(functions)... };

        return VisitHelper::visit(*this, m_index, m_data, move(visitor));
    }

    template<typename... NewTs>
    Variant<NewTs...> downcast() && {

        Variant<NewTs...> instance { Variant<NewTs...>::invalidIndex, Detail::VariantConstructTag { } };

        visit([&](auto& value) {

            if constexpr (Variant<NewTs...>::template canContain<RemoveConstVolatileReference<decltype(value)>>()) {

                instance.set(move(value), Detail::VariantNoClearTag {});
            }
        });

        VERIFY(instance.m_index != instance.invalidIndex);
        
        return instance;
    }

    template<typename... NewTs>
    Variant<NewTs...> downcast() const& {

        Variant<NewTs...> instance { Variant<NewTs...>::invalidIndex, Detail::VariantConstructTag {} };

        visit([&](auto const& value) {

            if constexpr (Variant<NewTs...>::template canContain<RemoveConstVolatileReference<decltype(value)>>()) {

                instance.set(value, Detail::VariantNoClearTag {});
            }
        });

        VERIFY(instance.m_index != instance.invalidIndex);
        
        return instance;
    }

private:

    static constexpr auto dataSize = Detail::integerSequenceGenerateArray<size_t>(0, IntegerSequence<size_t, sizeof(Ts)...>()).max();
    
    static constexpr auto dataAlignment = Detail::integerSequenceGenerateArray<size_t>(0, IntegerSequence<size_t, alignof(Ts)...>()).max();

    using Helper = Detail::Variant<IndexType, 0, Ts...>;

    using VisitHelper = Detail::VisitImpl<IndexType, Ts...>;

    template<typename T_, typename U_>
    friend struct Detail::VariantConstructors;

    explicit Variant(IndexType index, Detail::VariantConstructTag)
        : Detail::MergeAndDeduplicatePacks<Detail::VariantConstructors<Ts, Variant<Ts...>>...>(), 
          m_index(index) { }

    ALWAYS_INLINE void clearWithoutDestruction() {

        __builtin_memset(m_data, 0, dataSize);

        m_index = invalidIndex;
    }

    template<typename... Fs>
    struct Visitor : Fs... {

        using Types = TypeList<Fs...>;

        Visitor(Fs&&... args)
            : Fs(forward<Fs>(args))... { }

        using Fs::operator()...;
    };

    // Note: Make sure not to default-initialize!
    //       VariantConstructors::VariantConstructors(T) will set this to the correct value
    //       So default-constructing to anything will leave the first initialization with that value instead of the correct one.

    alignas(dataAlignment) UInt8 m_data[dataSize];

    IndexType m_index;
};