namespace SampleExtension.WASM;

public static class Program
{
    public static void Main(string[] args)
    {
        SampleExtension sampleExtension = new SampleExtension();
        sampleExtension.OnLoaded();
        sampleExtension.OnInitialized();
    }
}
