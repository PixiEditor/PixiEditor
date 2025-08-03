namespace PixiEditor.Helpers;

public static class RenderApiPreferenceManager
{
    public static string? FirstReadApiPreference { get; } = TryReadRenderApiPreference() ?? null;
    public static string? TryReadRenderApiPreference()
    {
        try
        {
            using var stream =
                new FileStream(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "PixiEditor",
                        "render_api.config"), FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            string? renderApi = reader.ReadLine();
            if (string.IsNullOrEmpty(renderApi))
            {
                return null;
            }

            return renderApi;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static void UpdateRenderApiPreference(string renderApi)
    {
        using var stream =
            new FileStream(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PixiEditor",
                    "render_api.config"), FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream);
        writer.WriteLine(renderApi);
    }
}
