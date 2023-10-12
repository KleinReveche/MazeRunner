using Reveche.MazeRunner;

namespace MazeRunner.Gui;

public partial class OptionsPage
{
    private readonly GameState _gameState;
    private readonly GameEngine _gameEngine;

    public OptionsPage(GameState gameState, GameEngine gameEngine)
    {
        _gameState = gameState;
        _gameEngine = gameEngine;
        InitializeComponent();
    }

    private void Play(object sender, EventArgs e)
    {
        
    }
}