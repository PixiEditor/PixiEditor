using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PixiEditorTests
{
    [CollectionDefinition("Application collection")]
    public class ApplicationCollection : ICollectionFixture<ApplicationFixture>
    {
    }
}
