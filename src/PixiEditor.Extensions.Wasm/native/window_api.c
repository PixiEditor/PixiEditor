#include <mono-wasi/driver.h>
#include <string.h>
#include <assert.h>

#include "api.h"

__attribute__((import_name("create_popup_window")))
void create_popup_window(const char* title, int32_t titleLength, uint8_t* data, int32_t length);

// content is byte[] from C#
void internal_create_popup_window(MonoString* title, uint8_t* data, int32_t length)
{
    char* title_utf8 = mono_wasm_string_get_utf8(title);
    create_popup_window(title_utf8, strlen(title_utf8), data, length);
}

void attach_window_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::CreatePopupWindow", internal_create_popup_window);
}