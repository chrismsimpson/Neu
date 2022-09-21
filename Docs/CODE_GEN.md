## Setting up CMake with Ninja

```
cmake Generated -B Build -G Ninja
```

## Building

```
cmake --build Build
```

## Running

```
./Build/Output/output
```

# Info

```
file ./Build/Output/output
```


# Building the runtime in isolation

```
cmake Runtime -B Build/Runtime -G Ninja
```

```
cmake --build Build/Runtime
```

Effective build command:

```
rm -Rf Build/Runtime && mkdir -p Build/Runtime && cmake Runtime -B Build/Runtime -G Ninja && cmake --build Build/Runtime
```
