
#pragma once

#include "StringUtils.h"
#include "String.h"

class FlyString {

public:
    
    FlyString() = default;

    FlyString(FlyString const& other)
        : m_impl(other.impl()) { }

    FlyString(FlyString&& other)
        : m_impl(move(other.m_impl)) { }

    FlyString(String const&);
    
    FlyString(StringView);
    
    FlyString(char const* string)
        : FlyString(static_cast<String>(string)) { }

    static FlyString fromFlyImpl(NonNullRefPointer<StringImpl> impl) {

        VERIFY(impl->isFly());

        FlyString string;
        
        string.m_impl = move(impl);
        
        return string;
    }

    FlyString& operator=(FlyString const& other) {

        m_impl = other.m_impl;
        
        return *this;
    }

    FlyString& operator=(FlyString&& other) {

        m_impl = move(other.m_impl);
        
        return *this;
    }

    bool isEmpty() const { return !m_impl || !m_impl->length(); }
    
    bool isNull() const { return !m_impl; }

    bool operator==(FlyString const& other) const { return m_impl == other.m_impl; }

    bool operator!=(FlyString const& other) const { return m_impl != other.m_impl; }

    bool operator==(String const&) const;
    
    bool operator!=(String const& string) const { return !(*this == string); }

    bool operator==(StringView) const;
    
    bool operator!=(StringView string) const { return !(*this == string); }

    bool operator==(char const*) const;
    
    bool operator!=(char const* string) const { return !(*this == string); }

    StringImpl const* impl() const { return m_impl; }

    char const* characters() const { return m_impl ? m_impl->characters() : nullptr; }
    
    size_t length() const { return m_impl ? m_impl->length() : 0; }

    ALWAYS_INLINE UInt32 hash() const { return m_impl ? m_impl->existingHash() : 0; }

    ALWAYS_INLINE StringView view() const { return m_impl ? m_impl->view() : StringView { }; }

    FlyString toLowercase() const;

    template<typename T = int>
    Optional<T> toInt(TrimWhitespace = TrimWhitespace::Yes) const;
    
    template<typename T = unsigned>
    Optional<T> toUint(TrimWhitespace = TrimWhitespace::Yes) const;

    bool equalsIgnoringCase(StringView) const;

    bool startsWith(StringView, CaseSensitivity = CaseSensitivity::CaseSensitive) const;
    
    bool endsWith(StringView, CaseSensitivity = CaseSensitivity::CaseSensitive) const;

    static void didDestroyImpl(Badge<StringImpl>, StringImpl&);

    template<typename... Ts>
    [[nodiscard]] ALWAYS_INLINE constexpr bool isOneOf(Ts... strings) const {

        return (... || this->operator==(forward<Ts>(strings)));
    }

private:

    RefPointer<StringImpl> m_impl;
};

template<>
struct Traits<FlyString> : public GenericTraits<FlyString> {

    static unsigned hash(FlyString const& s) { return s.hash(); }
};
