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
        ScoreText.Text = $"Score: {gameState.Score}";
    }

    private void DrawGrid() {         
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                gridImages[r, c].Source = gridValToImage[gameState.Grid[r, c]];
    }

    private async Task ShowCountDown() {
        for (int i = 3; i > 0; i--) {
            OverlayText.Text = i.ToString();
            await Task.Delay(500);
        }
    }

    private async Task ShowGameOver() {
        await Task.Delay(100);
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "Press Any key to restart";
    }

}
