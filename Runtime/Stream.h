
#pragma once

#include "Concepts.h"
#include "Endian.h"
#include "Forward.h"
#include "Optional.h"
#include "Span.h"
#include "std.h"

namespace Detail {

    class Stream {

    public:

        virtual ~Stream() { VERIFY(!hasAnyError()); }

        virtual bool hasRecoverableError() const { return m_recoverableError; }
    
        virtual bool hasFatalError() const { return m_fatalError; }
    
        virtual bool hasAnyError() const { return hasRecoverableError() || hasFatalError(); }

        virtual bool handleRecoverableError() {

            VERIFY(!hasFatalError());
            
            return exchange(m_recoverableError, false);
        }

        virtual bool handleFatalError() { return exchange(m_fatalError, false); }

        virtual bool handleAnyError() {

            if (hasAnyError()) {
                
                m_recoverableError = false;
                
                m_fatalError = false;

                return true;
            }

            return false;
        }

        ErrorOr<void> tryHandleAnyError() {

            if (!handleAnyError()) {

                return { };
            }

            return Error::fromStringLiteral("Stream error"sv);
        }

        virtual void setRecoverableError() const { m_recoverableError = true; }
    
        virtual void setFatalError() const { m_fatalError = true; }

    private:

        mutable bool m_recoverableError { false };
    
        mutable bool m_fatalError { false };
    };
}

///

class InputStream : public virtual Detail::Stream {

public:

    // Reads at least one byte unless none are requested or none are available. Does nothing
    // and returns zero if there is already an error.
    
    virtual size_t read(Bytes) = 0;

    // If this function returns true, then no more data can be read. If read(Bytes) previously
    // returned zero even though bytes were requested, then the inverse is true as well.

    virtual bool unreliableEof() const = 0;

    // Some streams additionally define a method with the signature:
    //
    //     bool eof() const;
    //
    // This method has the same semantics as unreliableEof() but returns true if and only if no
    // more data can be read. (A failed read is not necessary.)

    virtual bool readOrError(Bytes) = 0;
    
    virtual bool discardOrError(size_t count) = 0;
};

class OutputStream : public virtual Detail::Stream {

public:

    virtual size_t write(ReadOnlyBytes) = 0;

    virtual bool writeOrError(ReadOnlyBytes) = 0;
};

class DuplexStream : 
    public InputStream, public OutputStream {
};

inline InputStream& operator>>(InputStream& stream, Bytes bytes) {

    stream.readOrError(bytes);
    
    return stream;
}

inline OutputStream& operator<<(OutputStream& stream, ReadOnlyBytes bytes) {

    stream.writeOrError(bytes);

    return stream;
}

template<typename T>
InputStream& operator>>(InputStream& stream, LittleEndian<T>& value) {

    return stream >> Bytes { &value.m_value, sizeof(value.m_value) };
}