namespace Reveche.WorcleQuest;

internal static class GameEngine
{
    internal static void Play(GameState gameState)
    {
        while (true)
        {
            
            var guess = gameState.Guess;
            var selectedWord = gameState.SelectedWord;
            var currentGuess = gameState.CurrentGuess;
        
            foreach (var letter in guess)
            {
                CheckLetter(selectedWord, letter, guess, currentGuess);
            }
            //check if all letters are correct
            if (currentGuess.Count == 5 && currentGuess.All(x => x.Item2 == LetterState.Correct))
            {
                Console.WriteLine("You win!");
                break;
            }
        
            NextRound(gameState);
        }
    }
    
    internal static void CheckLetter(string word, char letter, IReadOnlyList<char> guess, List<(char, LetterState)> currentGuess)
    {
        var letterState = LetterState.Wrong;
        var letterIndex = word.IndexOf(letter);
        if (letterIndex != -1)
        {
            letterState = guess[letterIndex] == letter ? LetterState.Correct : LetterState.Misplaced;
        }
        //TODO: how about when there are two of the same letter in the word?
        

        Console.BackgroundColor = (ConsoleColor)letterState;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(letter);
        Console.ResetColor();
        
        currentGuess.Add((letter, letterState));
    }
    
    internal static void NextRound(GameState gameState)
    {
        gameState.PreviousGuesses.Add(gameState.CurrentGuess);
        gameState.CurrentGuess = new List<(char, LetterState)>();
        gameState.Tries++;
        Console.Clear();
        PrintPreviousGuesses(gameState);
        PrintCurrentGuess(gameState);
        
        string guess;
        while (true)
        {
            guess = Console.ReadLine() ?? string.Empty;
            if (guess != string.Empty && guess.Length == 5) break;
        }

        gameState.Guess = guess.ToCharArray();
    }
    
    internal static void PrintGameState(GameState gameState)
    {
        Console.WriteLine($"Tries: {gameState.Tries}");
        Console.WriteLine($"Previous Guesses: {gameState.PreviousGuesses.Count}");
        Console.WriteLine($"Current Guess: {gameState.CurrentGuess.Count}");
        Console.WriteLine($"Selected Word: {gameState.SelectedWord}");
        Console.WriteLine($"Guess: {new string(gameState.Guess)}");
    }
    
    internal static void PrintPreviousGuesses(GameState gameState)
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
    
    internal static void PrintCurrentGuess(GameState gameState)
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
    
    
}

internal enum LetterState
{
    Wrong = ConsoleColor.Gray,
    Correct = ConsoleColor.DarkGreen,
    Misplaced = ConsoleColor.Yellow,
}

//TODO: Check if letter placement is correct and if the word is a valid word.