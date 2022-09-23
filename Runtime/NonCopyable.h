
#pragma once

#define MAKE_NONCOPYABLE(c)     \
private:                        \
    c(c const&) = delete;       \
    c& operator=(c const&) = delete

#define MAKE_NONMOVABLE(c)      \
private:                        \
    c(c&&) = delete;            \
    c& operator=(c&&) = delete
