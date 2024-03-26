#include <mono-wasi/driver.h>
#include <string.h>
#include <assert.h>

__attribute__((import_name("log_message")))
void log_message(const char* message, int32_t messageLength);

void logger_log_message(MonoString* message)
{
    char* message_utf8 = mono_wasm_string_get_utf8(message);
    log_message(message_utf8, strlen(message_utf8));
}

void attach_logger_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::LogMessage", logger_log_message);
}