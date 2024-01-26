void attach_logger_calls();
void attach_window_calls();
void attach_layout_builder_calls();
void logger_log_message(MonoString* message);
MonoMethod* lookup_interop_method(const char* method_name);
void invoke_interop_method(MonoMethod* method, void* params);