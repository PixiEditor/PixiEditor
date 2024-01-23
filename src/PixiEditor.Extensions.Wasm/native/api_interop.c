#include <mono-wasi/driver.h>
#include <string.h>
#include <assert.h>

MonoMethod* method_init;

__attribute__((import_name("log_message")))
void log_message(const char* message, int32_t messageLength);

__attribute__((import_name("create_popup_window")))
void create_popup_window(const char* title, int32_t titleLength, const char* content, int32_t contentLength);

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

void logger_log_message(MonoString* message)
{
    char* message_utf8 = mono_wasm_string_get_utf8(message);
    log_message(message_utf8, strlen(message_utf8));
}

void logger_create_popup_window(MonoString* title, MonoString* content)
{
    char* title_utf8 = mono_wasm_string_get_utf8(title);
    char* content_utf8 = mono_wasm_string_get_utf8(content);
    create_popup_window(title_utf8, strlen(title_utf8), content_utf8, strlen(content_utf8));
}

void attach_internal_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::LogMessage", logger_log_message);
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::CreatePopupWindow", logger_create_popup_window);
}