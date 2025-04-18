using System.Text;

namespace PixiEditor.Models.User;

public static class UsernameGenerator
{
    private static List<string> adjectives { get; } = new List<string>
    {
        "Quick", "Lazy", "Sleepy", "Happy", "Sad", "Angry", "Excited", "Bored",
        "Curious", "Clever", "Brave", "Shy", "Bold", "Witty", "Charming", "Swift",
        "Silly", "Wise", "Eager", "Jolly", "Fierce", "Gentle", "Playful", "Mysterious",
        "Cunning", "Daring", "Lively", "Noble", "Radiant", "Serene", "Vibrant", "Melted"
    };

    private static List<string> nouns { get; } = new List<string>
    {
        "Fox", "Bear", "Wolf", "Eagle", "Lion", "Tiger", "Dragon", "Phoenix",
        "Shark", "Dolphin", "Whale", "Falcon", "Hawk", "Owl", "Raven", "Sparrow",
        "Turtle", "Frog", "Lizard", "Snake", "Spider", "Ant", "Bee", "Butterfly",
        "Potato", "Pixel", "Vector", "Brush", "Artist", "Knight", "Wizard", "Ninja"
    };

    public static string GenerateUsername(string hash)
    {
        Random random = new Random(Encoding.UTF8.GetBytes(hash).Sum(b => b));
        string adjective = GetRandomElement(adjectives, random);
        string noun = GetRandomElement(nouns, random);

        return $"{adjective}{noun}";
    }

    private static string GetRandomElement(List<string> set, Random random)
    {
        int index = random.Next(set.Count);
        string element = set[index];
        return element;
    }
}
