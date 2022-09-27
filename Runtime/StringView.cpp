
#include "AnyOf.h"
#include "ByteBuffer.h"
#include "Find.h"
#include "Function.h"
#include "StringView.h"
#include "Vector.h"

#ifndef OS
#    include "FlyString.h"
#    include "String.h"
#endif

#ifndef OS

StringView::StringView(String const& string)
    : m_characters(string.characters()), 
      m_length(string.length()) { }

StringView::StringView(FlyString const& string)
    : m_characters(string.characters()), 
      m_length(string.length()) { }

#endif

StringView::StringView(ByteBuffer const& buffer)
    : m_characters((char const*) buffer.data()), 
      m_length(buffer.size()) { }


Vector<StringView> StringView::splitView(char const separator, bool keepEmpty) const {

    StringView seperatorView { &separator, 1 };

    return splitView(seperatorView, keepEmpty);
}

Vector<StringView> StringView::splitView(StringView separator, bool keepEmpty) const {

    Vector<StringView> parts;

    forEachSplitView(separator, keepEmpty, [&](StringView view) {

        parts.append(view);
    });

    return parts;
}