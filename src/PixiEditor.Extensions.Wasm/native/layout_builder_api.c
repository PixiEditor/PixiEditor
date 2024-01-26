#include <mono-wasi/driver.h>
#include <string.h>
#include <assert.h>

#include "api.h"

__attribute((export_name("raise_element_event")))
void raise_element_event(int32_t elementId, char* eventName)
{
    MonoMethod* method = lookup_interop_method("EventRaised");
    MonoString* eventNameString = mono_wasm_string_from_js (eventName);
    void* args[] = { elementId, eventNameString };
    invoke_interop_method(method, args);
}

void attach_layout_builder_calls()
{

}