using PixiEditor.Extensions.Wasm;
using PixiEditor.WasmRuntime;

WasmRuntime runtime = new WasmRuntime();
runtime.LoadModule(@"C:\Git\PixiEditor\src\WasmSampleExtension\bin\Debug\net8.0\WasmSampleExtension.wasm");
