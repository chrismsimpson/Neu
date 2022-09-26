
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

// StringView::StringView(String const& string)
//     : m_characters(string.characters()), 
//       m_length(string.length()) { }


// StringView::StringView(FlyString const& string)
//     : m_characters(string.characters()), 
//       m_length(string.length()) { }

#endif