namespace Reveche.WorcleQuest;

internal class GameState
{
    public string SelectedWord { get; private set; }
    public string Guess { get; set; }
    public int Tries { get; set; }
    public List<(char, LetterState)> CurrentGuess { get; set; }
    public List<List<(char, LetterState)>> PreviousGuesses { get; }
    
    public GameState()
    {
        var wordsList = GameEngine.GetWords();
        Random random = new();

        SelectedWord = wordsList[random.Next(wordsList.Length - 1)];
        Guess = "";
        Tries = 0;
        CurrentGuess = new List<(char, LetterState)>();
        PreviousGuesses = new List<List<(char, LetterState)>>();
    }
    
}