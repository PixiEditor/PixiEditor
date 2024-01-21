using PixiEditor.Extensions.Wasm;
using PixiEditor.WasmRuntime;

WasmRuntime runtime = new WasmRuntime();
runtime.LoadModule(@"C:\Git\PixiEditor\src\WasmSampleExtension\bin\Release\net8.0\wasi-wasm\publish\WasmSampleExtension.wasm");
