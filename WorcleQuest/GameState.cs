using System.Reflection;

namespace Reveche.WorcleQuest;

internal class GameState
{
    public string SelectedWord { get; private set; }
    public char[] Guess { get; set; }
    public int Tries { get; set; }
    public List<(char, LetterState)> CurrentGuess { get; set; }
    public List<List<(char, LetterState)>> PreviousGuesses { get; }
    
    public GameState()
    {
        var wordsList = GetWords();
        Random random = new();

        SelectedWord = wordsList[random.Next(wordsList.Length - 1)];
        Guess = Array.Empty<char>();
        Tries = 0;
        CurrentGuess = new List<(char, LetterState)>();
        PreviousGuesses = new List<List<(char, LetterState)>>();
    }
    
    private static string[] GetWords()
    {
        var assembly = Assembly.GetExecutingAssembly(); // Get File from Assembly
        var stream = assembly.GetManifestResourceStream($"Reveche.WorcleQuest.words.txt")!;
        StreamReader reader = new(stream);
        
        // Read the entire content of the file into a string
        var wordsText = reader.ReadToEnd();

        // Split the string into an array of words using a delimiter (e.g., newline)
        return wordsText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
    }
}