project (Runtime)

cmake_minimum_required(VERSION 3.21)

set(CMAKE_CXX_STANDARD 20)

add_compile_options(-Wno-user-defined-literals)
add_compile_options(-DNEU_CONTINUE_ON_PANIC)

add_library(
    runtime 
    lib.cpp)

set_target_properties(runtime PROPERTIES LINKER_LANGUAGE CXX)