/*
 * Copyright (c) 2022, Andreas Kling <kling@serenityos.org>
 *
 * SPDX-License-Identifier: BSD-2-Clause
 */

#pragma once

#include <Core/Error.h>
#include <Core/RefCounted.h>
#include <Core/String.h>
#include <Builtins/Array.h>
#include <stdio.h>

class File final : public RefCounted<File> {

public:

    static ErrorOr<NonNullRefPointer<File>> openForReading(String path);
    
    static ErrorOr<NonNullRefPointer<File>> openForWriting(String path);

    ErrorOr<size_t> read(Array<UInt8>);
    
    ErrorOr<size_t> write(Array<UInt8>);

    ErrorOr<Array<UInt8>> readAll();

    ~File();

private:

    File();

    FILE* m_stdioFile { nullptr };
};
