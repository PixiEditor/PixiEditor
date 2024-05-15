#include <assert.h>
#include <driver.h>
#include <string.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/appdomain.h>

#include "api.h"

__attribute__((import_name("create_popup_window")))
int32_t create_popup_window(const char* title, int32_t titleLength, uint8_t* data, int32_t length);

// content is byte[] from C#
int32_t internal_create_popup_window(MonoString* title, uint8_t* data, int32_t length)
{
    char* title_utf8 = mono_string_to_utf8(title);
    return create_popup_window(title_utf8, strlen(title_utf8), data, length);
}

__attribute__((import_name("set_window_title")))
void set_window_title(int32_t windowHandle, const char* title, int32_t titleLength);

void internal_set_window_title(int32_t windowHandle, MonoString* title)
{
    char* title_utf8 = mono_string_to_utf8(title);
    set_window_title(windowHandle, title_utf8, strlen(title_utf8));
}

__attribute__((import_name("get_window_title")))
char* get_window_title(int32_t windowHandle);

MonoString* internal_get_window_title(int32_t windowHandle)
{
    MonoString* str = mono_string_new(mono_get_root_domain(), get_window_title(windowHandle));
    return str;
}

__attribute__((import_name("show_window")))
void show_window(int32_t windowHandle);

void internal_show_window(int32_t windowHandle)
{
    show_window(windowHandle);
}

__attribute__((import_name("close_window")))
void close_window(int32_t windowHandle);

void internal_close_window(int32_t windowHandle)
{
    close_window(windowHandle);
}

void attach_window_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::CreatePopupWindow", internal_create_popup_window);
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::SetWindowTitle", internal_set_window_title);
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::GetWindowTitle", internal_get_window_title);
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::ShowWindow", internal_show_window);
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::CloseWindow", internal_close_window);
}