
#pragma once

#include "Assertions.h"
#include "Error.h"
#include "Span.h"
#include "Types.h"
#include "kmalloc.h"

namespace Detail {

    template<size_t inlineCapacity>
    class ByteBuffer {

    public:

        ByteBuffer() = default;


















        [[nodiscard]] bool isEmpty() const { return m_size == 0; }

        [[nodiscard]] size_t size() const { return m_size; }

        [[nodiscard]] UInt8* data() { return m_inline ? m_inlineBuffer : m_outlineBuffer; }

        [[nodiscard]] UInt8 const* data() const { return m_inline ? m_inlineBuffer : m_outlineBuffer; }

        [[nodiscard]] Bytes bytes() { return { data(), size() }; }

        [[nodiscard]] ReadOnlyBytes bytes() const { return { data(), size() }; }


















        void clear() {

            if (!m_inline) {
                
                kfreeSized(m_outlineBuffer, m_outlineCapacity);

                m_inline = true;
            }

            m_size = 0;
        }

        ALWAYS_INLINE void resize(size_t newSize) {

            MUST(tryResize(newSize));
        }

        ALWAYS_INLINE void ensure_capacity(size_t newCapacity) {

            MUST(tryEnsureCapacity(newCapacity));
        }

        ErrorOr<void> tryResize(size_t newSize) {

            if (newSize <= m_size) {
                
                trim(newSize, false);
                
                return { };
            }
            
            TRY(tryEnsureCapacity(newSize));
            
            m_size = newSize;
            
            return { };
        }

        ErrorOr<void> tryEnsureCapacity(size_t newCapacity) {

            if (newCapacity <= capacity()) {

                return { };
            }

            return tryEnsureCapacitySlowpath(newCapacity);
        }

        /// Return a span of bytes past the end of this ByteBuffer for writing.
        /// Ensures that the required space is available.
        ErrorOr<Bytes> getBytesForWriting(size_t length) {

            TRY(tryEnsureCapacity(size() + length));

            return Bytes { data() + size(), length };
        }

        /// Like getBytesForWriting, but crashes if allocation fails.
        Bytes mustGetBytesForWriting(size_t length) {

            return MUST(getBytesForWriting(length));
        }

        void append(UInt8 byte) {

            MUST(tryAppend(byte));
        }

        void append(ReadOnlyBytes bytes) {

            MUST(tryAppend(bytes));
        }

        void append(void const* data, size_t dataSize) { append({ data, dataSize }); }

        ErrorOr<void> tryAppend(UInt8 byte) {

            auto oldSize = size();
            
            auto newSize = oldSize + 1;
            
            VERIFY(newSize > oldSize);
            
            TRY(try_resize(newSize));
            
            data()[oldSize] = byte;
            
            return { };
        }

        ErrorOr<void> tryAppend(ReadOnlyBytes bytes) {

            return tryAppend(bytes.data(), bytes.size());
        }

        ErrorOr<void> tryAppend(void const* data, size_t dataSize) {

            if (dataSize == 0) {

                return { };
            }
            
            VERIFY(data != nullptr);
            
            int oldSize = size();
            
            TRY(try_resize(size() + dataSize));
            
            __builtin_memcpy(this->data() + oldSize, data, dataSize);
            
            return { };
        }















































        void operator+=(ByteBuffer const& other) {

            MUST(tryAppend(other.data(), other.size()));
        }

        void overwrite(size_t offset, void const* data, size_t dataSize) {

            // make sure we're not told to write past the end
            
            VERIFY(offset + dataSize <= size());
            
            __builtin_memmove(this->data() + offset, data, dataSize);
        }

        void zeroFill() {

            __builtin_memset(data(), 0, m_size);
        }

        operator Bytes() { return bytes(); }

        operator ReadOnlyBytes() const { return bytes(); }

        ///

        ALWAYS_INLINE size_t capacity() const { return m_inline ? inlineCapacity : m_outlineCapacity; }

    private:

        void moveFrom(ByteBuffer&& other) {

            m_size = other.m_size;

            m_inline = other.m_inline;
            
            if (!other.m_inline) {

                m_outlineBuffer = other.m_outlineBuffer;
                
                m_outlineCapacity = other.m_outlineCapacity;
            } 
            else {
                
                VERIFY(other.m_size <= inlineCapacity);
                
                __builtin_memcpy(m_inlineBuffer, other.m_inlineBuffer, other.m_size);
            }
            
            other.m_size = 0;
            
            other.m_inline = true;
        }

        ///

        void trim(size_t size, bool mayDiscardExistingData) {

            VERIFY(size <= m_size);
            
            if (!m_inline && size <= inlineCapacity) {

                shrinkIntoInlineBuffer(size, mayDiscardExistingData);
            }

            m_size = size;
        }

        ///

        NEVER_INLINE void shrinkIntoInlineBuffer(size_t size, bool mayDiscardExistingData) {

            // m_inlineBuffer and m_outlineBuffer are part of a union, so save the pointer
            
            auto* outlineBuffer = m_outlineBuffer;

            auto outlineCapacity = m_outlineCapacity;

            if (!mayDiscardExistingData) {

                __builtin_memcpy(m_inlineBuffer, outlineCapacity, size);
            }

            kfreeSized(outlineBuffer, outlineCapacity);

            m_inline = true;
        }

        ///

        NEVER_INLINE ErrorOr<void> tryEnsureCapacitySlowpath(size_t newCapacity) {

            newCapacity = kmallocGoodSize(newCapacity);
            
            auto* newBuffer = (UInt8*) kmalloc(newCapacity);

            if (!newBuffer) {

                return Error::fromError(ENOMEM);
            }

            if (m_inline) {
                
                __builtin_memcpy(newBuffer, data(), m_size);
            }
            else if (m_outlineBuffer) {
                
                __builtin_memcpy(newBuffer, m_outlineBuffer, min(newCapacity, m_outlineCapacity));
                
                kfreeSized(m_outlineBuffer, m_outlineCapacity);
            }

            m_outlineBuffer = newBuffer;
            
            m_outlineCapacity = newCapacity;
            
            m_inline = false;
            
            return { };
        }

        ///

        union {

            UInt8 m_inlineBuffer[inlineCapacity];

            struct {

                UInt8* m_outlineBuffer;

                size_t m_outlineCapacity;
            };
        };

        size_t m_size { 0 };

        bool m_inline { true };
    };
}