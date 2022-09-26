
#include "FlyString.h"
#include "HashTable.h"
#include "Optional.h"
#include "Singleton.h"
#include "String.h"
#include "StringUtils.h"
#include "StringView.h"


struct FlyStringImplTraits : public Traits<StringImpl*> {

    static unsigned hash(StringImpl const* s) { return s ? s->hash() : 0; }

    static bool equals(StringImpl const* a, StringImpl const* b) {

        VERIFY(a);
        
        VERIFY(b);
        
        return *a == *b;
    }
};

static Singleton<HashTable<StringImpl*, FlyStringImplTraits>> s_table;

static HashTable<StringImpl*, FlyStringImplTraits>& flyImpls() {

    return *s_table;
}

void FlyString::didDestroyImpl(Badge<StringImpl>, StringImpl& impl) {

    flyImpls().remove(&impl);
}

FlyString::FlyString(String const& string) {

    if (string.isNull()) {

        return;
    }

    if (string.impl()->isFly()) {

        m_impl = string.impl();

        return;
    }

    auto it = flyImpls().find(const_cast<StringImpl*>(string.impl()));

    if (it == flyImpls().end()) {

        flyImpls().set(const_cast<StringImpl*>(string.impl()));
        
        string.impl()->setFly({ }, true);
        
        m_impl = string.impl();
    } 
    else {
        
        VERIFY((*it)->isFly());
        
        m_impl = *it;
    }
}


// FlyString::FlyString(StringView string) {

//     if (string.isNull()) {

//         return;
//     }

//     auto it = flyImpls().find(string.hash(), [&](auto& candidate) {
        
//         return string == candidate;
//     });

//     if (it == flyImpls().end()) {

//         auto new_string = string.toString();
        
//         flyImpls().set(new_string.impl());
        
//         new_string.impl()->setFly({ }, true);
        
//         m_impl = new_string.impl();
//     } 
//     else {
        
//         VERIFY((*it)->isFly());
        
//         m_impl = *it;
//     }
// }
