using System.Reflection;
using System.Text;

namespace Reveche.WorcleQuest;

internal static class GameEngine
{
    internal static void Play(GameState gameState)
    {
        while (true)
        {
            GetGuess(gameState);
            
            var guess = gameState.Guess;
            var selectedWord = gameState.SelectedWord;
            var currentGuess = gameState.CurrentGuess;
        
            CheckLetters(selectedWord, guess, currentGuess);
            
            // Check if all letters are correct
            if (currentGuess.Count == 5 && currentGuess.All(x => x.Item2 == LetterState.Correct))
            {
                Console.WriteLine("\nYou win!");
                break;
            }

            if (gameState.Tries == 5)
            {
                Console.WriteLine("\n You Lose!");
                break;
            }
        
            NextRound(gameState);
        }
    }
    
    private static void GetGuess(GameState gameState)
    {
        var wordsList = GetWords();
        string guess;
        while (true)
        {
            guess = Console.ReadLine() ?? string.Empty;
            if (guess != string.Empty && guess.Length == 5) break;
        }

        gameState.Guess = guess;
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

            Console.BackgroundColor = (ConsoleColor)letterState;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(letter);
            Console.ResetColor();

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
    
    private static void NextRound(GameState gameState)
    {
        gameState.PreviousGuesses.Add(gameState.CurrentGuess);
        gameState.CurrentGuess = new List<(char, LetterState)>();
        gameState.Tries++;
        Console.Clear();
        PrintPreviousGuesses(gameState);
        PrintCurrentGuess(gameState);
    }
    
    internal static void PrintGameState(GameState gameState)
    {
        Console.WriteLine($"Tries: {gameState.Tries}");
        Console.WriteLine($"Previous Guesses: {gameState.PreviousGuesses.Count}");
        Console.WriteLine($"Current Guess: {gameState.CurrentGuess.Count}");
        Console.WriteLine($"Selected Word: {gameState.SelectedWord}");
        Console.WriteLine($"Guess: {new string(gameState.Guess)}");
    }
    
    private static void PrintPreviousGuesses(GameState gameState)
    {
        foreach (var previousGuess in gameState.PreviousGuesses)
        {
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
    
    private static void PrintCurrentGuess(GameState gameState)
    {
        foreach (var (letter, letterState) in gameState.CurrentGuess)
        {
            Console.BackgroundColor = (ConsoleColor)letterState;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(letter);
            Console.ResetColor();
        }
        Console.WriteLine();
    }
    
    
    internal static string[] GetWords()
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

internal enum LetterState
{
    Wrong = ConsoleColor.Gray,
    Correct = ConsoleColor.DarkGreen,
    Misplaced = ConsoleColor.Yellow,
}

//TODO: Check if letter placement is correct and if the word is a valid word.