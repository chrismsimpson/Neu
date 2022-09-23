
#pragma once

#include "Assertions.h"

#include "Span.h"
#include "Types.h"

namespace Detail {

    template<size_t inlineCapacity>
    class ByteBuffer {

    public:

        ByteBuffer() = default;

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

        // NEVER_INLINE ErrorOr<void> try_ensure_capacity_slowpath(size_t new_capacity)
        // {
        //     new_capacity = kmalloc_good_size(new_capacity);
        //     auto* new_buffer = (u8*)kmalloc(new_capacity);
        //     if (!new_buffer)
        //         return Error::from_errno(ENOMEM);

        //     if (m_inline) {
        //         __builtin_memcpy(new_buffer, data(), m_size);
        //     } else if (m_outline_buffer) {
        //         __builtin_memcpy(new_buffer, m_outline_buffer, min(new_capacity, m_outline_capacity));
        //         kfree_sized(m_outline_buffer, m_outline_capacity);
        //     }

        //     m_outline_buffer = new_buffer;
        //     m_outline_capacity = new_capacity;
        //     m_inline = false;
        //     return {};
        // }

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