#include <mono-wasi/driver.h>
#include <string.h>
#include <assert.h>

#include "api.h"

__attribute((import_name("subscribe_to_event")))
void subscribe_to_event(int32_t elementId, char* eventName, int32_t length);

__attribute__((import_name("state_changed")))
void state_changed(int32_t elementId, uint8_t* data, int32_t length);

void internal_subscribe_to_event(int32_t elementId, MonoString* eventName)
{
    char* eventNameString = mono_wasm_string_get_utf8(eventName);
    subscribe_to_event(elementId, eventNameString, strlen(eventNameString));
}

__attribute((export_name("raise_element_event")))
void raise_element_event(int32_t elementId, const char* eventName)
{
    MonoMethod* method = lookup_interop_method("EventRaised");
    void* args[] = { &elementId, mono_wasm_string_from_js(eventName) };
    invoke_interop_method(method, args);

    free(method);
}

void attach_layout_builder_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::SubscribeToEvent", internal_subscribe_to_event);
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::StateChanged", state_changed);
}