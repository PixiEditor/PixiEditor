using System;

namespace PixiEditor.DrawingApi.Core.Exceptions;

public class ShaderCompilationException : Exception
{
    public ShaderCompilationException(string errors) : base(errors)
    {
    }
}
