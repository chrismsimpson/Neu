
#pragma once

#include <stdio.h>
#include <string.h>

#include "CheckedFormatString.h"

#include "Array.h"
#include "Forward.h"
#include "StringView.h"

// void outLine(const char * format, ...) {

//     va_list arglist;

//     va_start(arglist, format);

//     vprintf(format, arglist);

//     fputc('\n', stdout);

//     va_end(arglist);
// }


template<typename... Parameters>
void out(
    FILE* file, 
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) {

}

template<typename... Parameters>
void outLine(
    FILE* file,
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) { 
    
}

// inline 
void outLine(FILE* file) { 
    
    fputc('\n', file);
}

template<typename... Parameters>
void out(
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) { 
    
    out(stdout, move(fmtstr), parameters...);
}

template<typename... Parameters>
void outLine(
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) { 
    
    outLine(stdout, move(fmtstr), parameters...);
}

// inline 
void outLine() { 
    
    outLine(stdout);
}