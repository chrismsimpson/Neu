/*
 * Copyright (c) 2022, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#include "File.h"
#include <errno.h>

File::File() { }

File::~File() {

    fclose(m_stdioFile);
}

ErrorOr<NonNullRefPointer<File>> File::openForReading(String path) {

    auto* stdioFile = fopen(path.characters(), "rb");
    
    if (!stdioFile) {
    
        return Error::fromError(errno);
    }
    
    auto file = TRY(adoptNonNullRefOrErrorNomem(new (nothrow) File));
    
    file->m_stdioFile = stdioFile;
    
    return file;
}

ErrorOr<NonNullRefPointer<File>> File::openForWriting(String path) {
    
    auto* stdioFile = fopen(path.characters(), "wb");

    if (!stdioFile) {

        return Error::fromError(errno);
    }

    auto file = TRY(adoptNonNullRefOrErrorNomem(new (nothrow) File));
    
    file->m_stdioFile = stdioFile;
    
    return file;
}

ErrorOr<Array<UInt8>> File::readAll() {

    Array<UInt8> entireFile;

    while (true) {

        UInt8 buffer[4096];
        
        auto nread = fread(buffer, 1, sizeof(buffer), m_stdioFile);
        
        if (nread == 0) {
            
            if (feof(m_stdioFile)) {
            
                return entireFile;
            }
            
            auto error = ferror(m_stdioFile);
            
            return Error::fromError(error);
        }

        size_t oldSize = entireFile.size();
        
        TRY(entireFile.addSize(nread));
        
        memcpy(entireFile.unsafeData() + oldSize, buffer, nread);
    }
}

ErrorOr<size_t> File::read(Array<UInt8> buffer) {

    auto nread = fread(buffer.unsafeData(), 1, buffer.size(), m_stdioFile);

    if (nread == 0) {

        if (feof(m_stdioFile)) {

            return 0;
        }

        auto error = ferror(m_stdioFile);
        
        return Error::fromError(error);
    }

    return nread;
}

ErrorOr<size_t> File::write(Array<UInt8> data) {

    auto nwritten = fwrite(data.unsafeData(), 1, data.size(), m_stdioFile);
    
    if (nwritten == 0) {
        
        auto error = ferror(m_stdioFile);
        
        return Error::fromError(error);
    }

    return nwritten;
}
