/*
 * Copyright (c) 2021, Max Wipfli <mail@maxwipfli.ch>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include "Forward.h"

namespace UnicodeUtils {

    template<typename Callback>
    [[nodiscard]] constexpr int codePointToUtf8(UInt32 codePoint, Callback callback) {

        if (codePoint <= 0x7f) {
            
            callback((char)codePoint);
            
            return 1;
        } 
        else if (codePoint <= 0x07ff) {
            
            callback((char)(((codePoint >> 6) & 0x1f) | 0xc0));
            
            callback((char)(((codePoint >> 0) & 0x3f) | 0x80));
            
            return 2;
        }
        else if (codePoint <= 0xffff) {
            
            callback((char)(((codePoint >> 12) & 0x0f) | 0xe0));
            
            callback((char)(((codePoint >> 6) & 0x3f) | 0x80));
            
            callback((char)(((codePoint >> 0) & 0x3f) | 0x80));
            
            return 3;
        } 
        else if (codePoint <= 0x10ffff) {

            
            callback((char)(((codePoint >> 18) & 0x07) | 0xf0));
            
            callback((char)(((codePoint >> 12) & 0x3f) | 0x80));
            
            callback((char)(((codePoint >> 6) & 0x3f) | 0x80));
            
            callback((char)(((codePoint >> 0) & 0x3f) | 0x80));
            
            return 4;
        }
        
        return -1;
    }
}