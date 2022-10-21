
08/05/22: da17cefbba88f80fdb363ca325e6124667cb3c40 add ak 

08/05/22: 34b40b37663b6119ed6a64f7bbac55009911fd63 indexing 

09/05/22: 94680f7ddf0ab571d33026d9ff55d3ecf311b345 remove flystring 

12/05/22: e723cd66ea65496debecc9a6c679f12a1efcd6db hex literals

14/05/22: 34d9663d036209aaa0cbf6d44e8f3a924f24e699 ns part 1

16/05/22: 415eb56f5dac2a80f7426ea7112de4cfdbb9a929 more generics

20/05/22: abac592fd95152bd560fa2d4d08fb6685a87bf9b enums

20/05/22: fc9211e774cbf5594262dc02a162632d3111f63a inline c++

21/05/22: ba12627ffc9e2cc765fe400b0117ea18a6cbf1d0 parser test

21/05/22: acb36a069a452e01c8c725dda8ee04fcb89b473b vs launch

21/05/22: b89afba419a64423b068eca919c65fa920af6490 Add overflow checks to arithmetic

22/05/22: 1cf4f7065b73a8e23266f0d53c86afc4e08b3d45 IsMutating()

22/05/22: 19e43debb35545e56487cc3cd9e49767d877a385 stderr



launch.json:
```
{
    "version": "0.2.0",
    "configurations": [
      {
        "type": "lldb",
        "request": "launch",
        "name": "Launch",
        "args": [
            // "./samples/basics/cat.jakt"
            "./samples/classes/static_method.jakt"
        ],
        "program": "${workspaceFolder}/target/debug/jakt",
        "cwd": "${workspaceFolder}",
        "stopOnEntry": false,
        "sourceLanguages": ["rust"],
        "sourceMap": {
          "/rustc/*": "${env:HOME}/.rustup/toolchains/stable-x86_64-apple-darwin/lib/rustlib/src/rust"
        }
      }
    ]
  }
```