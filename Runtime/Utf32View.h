
#pragma once

#include "Assertions.h"
#include "Checked.h"
#include "Types.h"

class Utf32View;

class Utf32CodePointIterator {
    
    friend class Utf32View;

public:

    Utf32CodePointIterator() = default;
    
    ~Utf32CodePointIterator() = default;

    bool operator==(Utf32CodePointIterator const& other) const {

        return m_ptr == other.m_ptr && m_length == other.m_length;
    }
    
    bool operator!=(Utf32CodePointIterator const& other) const {

        return !(*this == other);
    }
    
    Utf32CodePointIterator& operator++() {

        VERIFY(m_length > 0);
        
        m_ptr++;
        
        m_length--;
        
        return *this;
    }

    ssize_t operator-(Utf32CodePointIterator const& other) const {

        return m_ptr - other.m_ptr;
    }

    UInt32 operator*() const {

        VERIFY(m_length > 0);
        
        return *m_ptr;
    }

    constexpr int codePointLengthInBytes() const { return sizeof(UInt32); }

    bool done() const { return !m_length; }

private:

    Utf32CodePointIterator(UInt32 const* ptr, size_t length)
        : m_ptr(ptr), 
          m_length((ssize_t)length) { }

    UInt32 const* m_ptr { nullptr };
    
    ssize_t m_length { -1 };
};

///

class Utf32View {

public:

    using Iterator = Utf32CodePointIterator;

    Utf32View() = default;
    
    Utf32View(UInt32 const* codePoints, size_t length)
        : m_codePoints(codePoints), 
          m_length(length) {

        VERIFY(codePoints || length == 0);
    }

    Utf32CodePointIterator begin() const {

        return { beginPointer(), m_length };
    }

    Utf32CodePointIterator end() const {

        return { endPointer(), 0 };
    }

    UInt32 at(size_t index) const
    {
        VERIFY(index < m_length);
        return m_codePoints[index];
    }

    UInt32 operator[](size_t index) const { return at(index); }

    UInt32 const* codePoints() const { return m_codePoints; }
    
    bool isEmpty() const { return m_length == 0; }
    
    bool isNull() const { return !m_codePoints; }
    
    size_t length() const { return m_length; }

    size_t iteratorOffset(Utf32CodePointIterator const& it) const {
        
        VERIFY(it.m_ptr >= m_codePoints);
        
        VERIFY(it.m_ptr < m_codePoints + m_length);
        
        return ((ptrdiff_t)it.m_ptr - (ptrdiff_t) m_codePoints) / sizeof(UInt32);
    }

    Utf32View substringView(size_t offset, size_t length) const {

        VERIFY(offset <= m_length);
        
        VERIFY(!Checked<size_t>::additionWouldOverflow(offset, length));
        
        VERIFY((offset + length) <= m_length);
        
        return Utf32View(m_codePoints + offset, length);
    }

private:
    
    UInt32 const* beginPointer() const {

        return m_codePoints;
    }
    
    UInt32 const* endPointer() const {

        return m_codePoints + m_length;
    }

    UInt32 const* m_codePoints { nullptr };
    
    size_t m_length { 0 };
};