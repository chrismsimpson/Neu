#pragma once

template<typename Block>
class Defer {

public: 

    Defer(Block block)
        : m_block(block) { }

private:
    
    Block m_block;
};