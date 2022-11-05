namespace PixiEditor.UpdateModule;

public class UpdateChannel
{
    public string Name { get; }
    public string RepositoryOwner { get; }
    public string RepositoryName { get; }
    public string ApiUrl { get; } 
    public string IncompatibleFileApiUrl { get; }

    public UpdateChannel(string name, string repositoryOwner, string repositoryName)
    {
        Name = name;
        RepositoryOwner = repositoryOwner;
        RepositoryName = repositoryName;
        ApiUrl = $"https://api.github.com/repos/{repositoryOwner}/{repositoryName}/releases/latest";
        IncompatibleFileApiUrl = "https://raw.githubusercontent.com/" + $"{repositoryOwner}/{repositoryName}/" + "{0}/incompatible.json";
    }
}
