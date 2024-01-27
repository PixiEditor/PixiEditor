void attach_logger_calls();
void attach_window_calls();
void attach_layout_builder_calls();
void logger_log_message(MonoString* message);
void log_message(const char* message, int32_t messageLength);
MonoMethod* lookup_interop_method(const char* method_name);
void invoke_interop_method(MonoMethod* method, void* params);