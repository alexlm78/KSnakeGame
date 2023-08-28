using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace KSnakeGame;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    private readonly Dictionary<GridValue, ImageSource> gridValToImage = new() {
        { GridValue.Empty, Images.Empty },
        { GridValue.Snake, Images.Body },
        { GridValue.Food, Images.Food }
        //{ GridValue.DeadBody, Images.DeadBody },
        //{ GridValue.DeadHead, Images.DeadHead }
    };

    private readonly Dictionary<Direction, int> dirToRotation = new()
    {
        { Direction.Up, 0 },
        { Direction.Down, 180 },
        { Direction.Left, 270 },
        { Direction.Right, 90 }
    };

    private readonly int rows = 15, cols=15;
    private readonly Image[,] gridImages;
    private GameState gameState;
    private bool gameRunning;

    public MainWindow() {
        InitializeComponent();
        gridImages = SetupGrid();
        gameState = new GameState(rows, cols);
    }

    private async Task RunGame() { 
        Draw();
        await ShowCountDown();
        Overlay.Visibility = Visibility.Hidden;
        await GameLoop();
        await ShowGameOver();
        gameState = new GameState(rows, cols);
    }

    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
        if (Overlay.Visibility == Visibility.Visible)
            e.Handled = true;

        if (!gameRunning) {
            gameRunning = true;
            await RunGame();
            gameRunning = false;
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e) {
        if (gameState.IsGameOver)
            return;
        
        if (e.Key == Key.Escape)
            Close();

        switch (e.Key) {
            case Key.Up:
                gameState.ChangeDirection(Direction.Up);
                break;
            case Key.Down:
                gameState.ChangeDirection(Direction.Down);
                break;
            case Key.Left:
                gameState.ChangeDirection(Direction.Left);
                break;
            case Key.Right:
                gameState.ChangeDirection(Direction.Right);
                break;
        }
    }

    private async Task GameLoop() {
        while (!gameState.IsGameOver) {
            await Task.Delay(400);
            gameState.Move();
            Draw();
        }
    }

    
    private Image[,] SetupGrid() {
        Image[,] images = new Image[rows, cols];
        GameGrid.Rows = rows;
        GameGrid.Columns = cols;

        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                Image image = new Image();
                image.Source = Images.Empty;
                image.RenderTransformOrigin = new Point(0.5, 0.5);

                images[r, c] = image;
                GameGrid.Children.Add(image);
                //Grid.SetRow(image, r);
                //Grid.SetColumn(image, c);
            }
        }
        return images;
    }

    private void Draw() {
        DrawGrid();
        DrawSnakeHead();
        ScoreText.Text = $"Score: {gameState.Score}";
    }

    private void DrawGrid() {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++) {
                gridImages[r, c].Source = gridValToImage[gameState.Grid[r, c]];
                gridImages[r, c].RenderTransform = Transform.Identity;
            }
    }

    private async Task ShowCountDown() {
        for (int i = 3; i > 0; i--) {
            OverlayText.Text = i.ToString();
            await Task.Delay(500);
        }
    }

    private async Task ShowGameOver() {
        await DrawDeadHeasSnake();
        await Task.Delay(1000);
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "Press Any key to restart";
    }

    private void DrawSnakeHead() {
        Position headPos = gameState.HeadPosition();
        Image img = gridImages[headPos.Row, headPos.Col];
        img.Source = Images.Head;

        int rot = dirToRotation[gameState.SnakeDirection];
        img.RenderTransform = new RotateTransform(rot);
    }

    private async Task DrawDeadHeasSnake() {
        List<Position> snakePos = new List<Position>(gameState.SnakePositions());

        for ( int i = 0; i < snakePos.Count; i++)
        {
            Position pos = snakePos[i];
            ImageSource src = (i == 0) ? Images.DeadHead : Images.DeadBody;
            gridImages[pos.Row, pos.Col].Source = src;
            await Task.Delay(50);
        }
    }
}
