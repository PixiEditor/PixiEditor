#include <assert.h>
#include <driver.h>
#include <string.h>
#include <mono/metadata/object.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/appdomain.h>

#include "api.h"

__attribute((import_name("subscribe_to_event")))
void subscribe_to_event(int32_t elementId, char* eventName, int32_t length);

__attribute__((import_name("state_changed")))
void state_changed(int32_t elementId, uint8_t* data, int32_t length);

void internal_subscribe_to_event(int32_t elementId, MonoString* eventName)
{
    char* eventNameString = mono_string_to_utf8(eventName);
    subscribe_to_event(elementId, eventNameString, strlen(eventNameString));
}

__attribute((export_name("raise_element_event")))
void raise_element_event(int32_t elementId, const char* eventName)
{
    MonoMethod* method = lookup_interop_method("EventRaised");
    MonoString* monoEventName = mono_string_new(mono_domain_get(), eventName);
    void* args[] = { &elementId, monoEventName };
    invoke_interop_method(method, args);

    free(method);
}

void attach_layout_builder_calls()
{
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::SubscribeToEvent", internal_subscribe_to_event);
    mono_add_internal_call("PixiEditor.Extensions.Wasm.Interop::StateChanged", state_changed);
}