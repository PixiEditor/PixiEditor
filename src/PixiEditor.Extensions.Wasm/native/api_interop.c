#include <mono-wasi/driver.h>
#include <string.h>
#include <assert.h>
#include "api.h"

MonoMethod* method_init;

__attribute((export_name("initialize")))
void initialize()
{
    if (!method_init) {
        method_init = lookup_dotnet_method("PixiEditor.Extensions.Wasm.dll", "PixiEditor.Extensions.Wasm", "Interop", "Initialize", -1);
        assert(method_init);
    }

    void* method_params[] = {  };
    MonoObject* exception;
    mono_wasm_invoke_method(method_init, NULL, method_params, &exception);
    assert(!exception);
}

void attach_internal_calls()
{
    attach_logger_calls();
    attach_window_calls();
}