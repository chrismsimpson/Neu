
#pragma once

#include <assert.h>

#define VERIFY assert

#define VERIFY_NOT_REACHED() assert(false)

static constexpr bool TODO = false;

#define TODO() VERIFY(TODO)