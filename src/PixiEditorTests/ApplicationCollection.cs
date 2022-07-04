using Xunit;

namespace PixiEditorTests
{
    [CollectionDefinition("Application collection")]
    public class ApplicationCollection : ICollectionFixture<ApplicationFixture>
    {
    }
}