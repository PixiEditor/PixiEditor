#include <assert.h>
#include <driver.h>
#include <mono/metadata/object.h>
#include <string.h>
#include <mono/metadata/exception.h>

__attribute__((import_name("log_message")))
void log_message(const char* message, int32_t messageLength);

void logger_log_message(MonoString* message)
{
    char* message_utf8 = mono_string_to_utf8(message);
    log_message(message_utf8, strlen(message_utf8));
}

void attach_logger_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::LogMessage", logger_log_message);
}