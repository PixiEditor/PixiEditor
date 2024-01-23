#include <mono-wasi/driver.h>
#include <string.h>
#include <assert.h>

__attribute__((import_name("create_popup_window")))
void create_popup_window(const char* title, int32_t titleLength, const char* content, int32_t contentLength);

void logger_create_popup_window(MonoString* title, MonoString* content)
{
    char* title_utf8 = mono_wasm_string_get_utf8(title);
    char* content_utf8 = mono_wasm_string_get_utf8(content);
    create_popup_window(title_utf8, strlen(title_utf8), content_utf8, strlen(content_utf8));
}

void attach_window_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::CreatePopupWindow", logger_create_popup_window);
}