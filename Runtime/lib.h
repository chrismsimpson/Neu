#pragma once

template<typename Block>
class Defer {

public:

    Defer(
        Block block)
        : m_block(block) { }

    ~Defer() { m_block(); }

private:

    Block m_block;
};

void foo();