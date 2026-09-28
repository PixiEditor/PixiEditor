namespace PixiEditor.ViewModels.ExtensionManager;

public class ShowcaseItem
{

}

public class VideoShowcaseItem : ShowcaseItem
{
    public string VideoUrl { get; set; }

    public VideoShowcaseItem(string videoUrl)
    {
        VideoUrl = videoUrl;
    }
}

public class ImageShowcaseItem : ShowcaseItem
{
    public string ImageUrl { get; set; }

    public ImageShowcaseItem(string imageUrl)
    {
        ImageUrl = imageUrl;
    }
}
