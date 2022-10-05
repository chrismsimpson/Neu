
08/05/22: da17cefbba88f80fdb363ca325e6124667cb3c40 add ak 

08/05/22: 34b40b37663b6119ed6a64f7bbac55009911fd63 indexing 

09/05/22: 94680f7ddf0ab571d33026d9ff55d3ecf311b345 remove flystring 

12/05/22: e723cd66ea65496debecc9a6c679f12a1efcd6db hex literals




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