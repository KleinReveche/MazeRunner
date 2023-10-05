using System.Reflection;
using System.Text;

namespace Reveche.WorcleQuest;

public class GameEngine
{
    private readonly GameState _gameState;
    
    public GameEngine(GameState gameState)
    {
        _gameState = gameState;
    }
    
    public void Play(out bool result)
    {
        var win = false;
        
        while (true)
        {
            GetGuess();
            
            var guess = _gameState.Guess;
            var selectedWord = _gameState.SelectedWord;
            var currentGuess = _gameState.CurrentGuess;
        
            CheckLetters(selectedWord, guess, currentGuess);
            
            // Check if all letters are correct
            if (currentGuess.Count == 5 && currentGuess.All(x => x.Item2 == LetterState.Correct))
            {
                Console.WriteLine("\nYou win!");
                win = true;
                break;
            }

            if (_gameState.Tries == 5)
            {
                Console.WriteLine($"\n You Lose! The word is {selectedWord}");
                break;
            }
        
            NextRound();
        }
        
        result = win;
    }
    
    private void GetGuess()
    {
        string guess;
        Console.SetCursorPosition(6, 17 + _gameState.Tries);
        while (true)
        {
            guess = Console.ReadLine() ?? string.Empty;
            if (guess != string.Empty && guess.Length == 5) break;
        }

        _gameState.Guess = guess;
    }
    
    private static void CheckLetters(string word, string guess, ICollection<(char, LetterState)> currentGuess)
    {
        foreach (var letter in guess)
        {
            var letterState = LetterState.Wrong;
            var letterIndex = word.IndexOf(letter);
            
            if (letterIndex != -1)
            {
                letterState = guess[letterIndex] == letter ? LetterState.Correct : LetterState.Misplaced;
            }

            currentGuess.Add((letter, letterState));
        }
    }
    
    private static void CheckLetterOld(string word, string guess, ICollection<(char, LetterState)> currentGuess)
    {
        for (var i = 0; i < guess.Length; i++)
        {
            var remainingGuess = new StringBuilder(guess);
            var letter = word[i];
            var letterState = LetterState.Wrong;
            var guessIndex = remainingGuess.ToString().IndexOf(letter);

            if (guessIndex != -1)
            {
                if (guessIndex == i)
                {
                    letterState = LetterState.Correct;
                }
                else
                {
                    letterState = LetterState.Misplaced;

                    // Check for the number of misplaced instances of this letter in the guessed word
                    var misplacedCount = remainingGuess.ToString().Count(ch => ch == letter && remainingGuess.ToString().IndexOf(ch) != i);

                    // If there's more than one misplaced instance, mark it as Wrong
                    if (misplacedCount > 1)
                    {
                        letterState = LetterState.Wrong;
                    }
                }
            
                // Mark the guessed letter as processed to avoid re-checking it
                remainingGuess[guessIndex] = ' ';
            }

            Console.BackgroundColor = (ConsoleColor)letterState;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(letter);
            Console.ResetColor();
        
            currentGuess.Add((letter, letterState));
        }
    }

    private void NextRound()
    {
        _gameState.PreviousGuesses.Add(_gameState.CurrentGuess);
        _gameState.CurrentGuess = new List<(char, LetterState)>();
        _gameState.Tries++;
        Console.SetCursorPosition(6, 2);
        PrintPreviousGuesses();
        //PrintCurrentGuess(gameState);
    }

    private void PrintPreviousGuesses()
    {
        foreach (var previousGuess in _gameState.PreviousGuesses)
        {
            Console.SetCursorPosition(6,17 + _gameState.PreviousGuesses.IndexOf(previousGuess));
            foreach (var (letter, letterState) in previousGuess)
            {
                Console.BackgroundColor = (ConsoleColor)letterState;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(letter);
                Console.ResetColor();
            }
            Console.WriteLine();
        }
    }
    
    public static string[] GetWords()
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

public enum LetterState
{
    Wrong = ConsoleColor.Gray,
    Correct = ConsoleColor.DarkGreen,
    Misplaced = ConsoleColor.Yellow,
}

//TODO: Check if letter placement is correct and if the word is a valid word.