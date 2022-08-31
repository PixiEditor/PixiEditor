using System;
using PixiEditor.DrawingApi.Core.Exceptions;

namespace PixiEditor.DrawingApi.Core.Bridge
{
    public class DrawingBackendApi
    {
        private static IDrawingBackend _current;

        public static IDrawingBackend Current
        {
            get
            {
                if (_current == null)
                    throw new NullReferenceException("Either drawing backend was not yet initialized or reference was somehow lost.");

                return _current;
            }
        }
        
        public void SetupBackend(IDrawingBackend backend)
        {
            if (Current != null)
            {
                throw new InitializationDuplicateException("Drawing backend was already initialized.");
            }
            
            _current = backend;
            backend.Setup();
        }
    }
}
