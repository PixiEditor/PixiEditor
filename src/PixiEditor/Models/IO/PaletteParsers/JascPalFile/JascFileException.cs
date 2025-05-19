using System.Runtime.Serialization;
using PixiEditor.Extensions.Exceptions;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Models.IO.PaletteParsers.JascPalFile;


internal class JascFileException : RecoverableException
{
    public JascFileException() { }

    public JascFileException(LocalizedString displayMessage) : base(displayMessage) { }

    public JascFileException(LocalizedString displayMessage, Exception innerException) : base(displayMessage, innerException) { }

    public JascFileException(LocalizedString displayMessage, string exceptionMessage) : base(displayMessage, exceptionMessage) { }

    public JascFileException(LocalizedString displayMessage, string exceptionMessage, Exception innerException) : base(displayMessage, exceptionMessage, innerException) { }

    protected JascFileException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
