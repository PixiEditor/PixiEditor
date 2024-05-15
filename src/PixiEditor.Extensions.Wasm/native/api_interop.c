#include <assert.h>
#include <driver.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>
#include "api.h"

extern void _start(void);

void attach_internal_calls()
{
    attach_logger_calls();
    attach_window_calls();
    attach_layout_builder_calls();
}

void initialize_runtime(void)
{
    static int runtime_initialized = 0;

    if (runtime_initialized == 0) {
        _start();
        attach_internal_calls();
        runtime_initialized = 1;
    }
}

MonoMethod* lookup_interop_method(const char* method_name)
{
    MonoMethod* method = NULL;
    method = lookup_dotnet_method("PixiEditor.Extensions.Wasm.dll", "PixiEditor.Extensions.Wasm", "Interop", method_name, -1);
    assert(method);

    return method;
}

void invoke_interop_method(MonoMethod* method, void* params)
{
    MonoObject* exception = NULL;
    MonoObject* res = mono_runtime_invoke(method, NULL, params, &exception);
    assert(!exception);

    free(exception);
    free(method);
}

__attribute((export_name("load")))
void load()
{
    initialize_runtime();

    MonoMethod* metod = lookup_interop_method("Load");
    invoke_interop_method(metod, NULL);
}

__attribute((export_name("initialize")))
void initialize()
{
    MonoMethod* metod = lookup_interop_method("Initialize");
    invoke_interop_method(metod, NULL);
}