#include <mono-wasi/driver.h>

__attribute__((import_name("log_message")))
void log_message(const char* message);

void attach_internal_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::LogMessage", (void*)log_message);
}