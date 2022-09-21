
#pragma once

#include <stdio.h>

#include "CheckedFormattedString.h"

template<typename... Parameters>
void out(
    FILE* file, 
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) {

    // TODO
}

template<typename... Parameters>
void outLine(
    FILE* file, 
    CheckedFormatString<Parameters...>&& fmtstr, 
    Parameters const&... parameters) {

    // TODO
}

void outLine(
    FILE* file) { fputc('\n', file); }

void outLine() { outLine(stdout); }