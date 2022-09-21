
#pragma once

#include "Forward.h"

template<typename Container, typename ValueType>
class SimpleIterator {

public: 

    friend Container;

    ///
        
    // constexpr bool isEnd() const { return m_index == SimpleIterator::end(m_container).m_index; }

    ///



private:

    static constexpr SimpleIterator begin(Container& container) { return { container, 0 }; }

    ///

    // static constexpr SimpleIterator end(Container& container) {

    //     using RawContainerType = RemoveConstVolatile<Container>;

    //     if constexpr (IsSame<StringView, RawContainerType> || IsSame<String, RawContainerType>) {

    //         return { container, container.length() };
    //     }
    //     else {

    //         return { container, container.size() };
    //     }
    // }


    ///
    
    Container& m_container;

    size_t m_index;
};