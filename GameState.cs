using System;
using System.Collections.Generic;
using System.Security.RightsManagement;

namespace KSnakeGame;

public class GameState
{
    public int Rows { get; }
    public int Cols { get; }
    public GridValue[,] Grid { get; }
    public Position SnakeHead { get; }
    public Position SnakeTail { get; }
    public Direction SnakeDirection { get; private set; }
    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }

    private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
    private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
    private readonly Random random = new Random();

    public GameState(int rows, int cols, GridValue[,] grid, Position snakeHead, Position snakeTail, Direction snakeDirection, int score, bool isGameOver)
    {
        Rows = rows;
        Cols = cols;
        Grid = grid;
        SnakeHead = snakeHead;
        SnakeTail = snakeTail;
        SnakeDirection = snakeDirection;
        Score = score;
        IsGameOver = isGameOver;
    }

    public GameState(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        Grid = new GridValue[rows, cols];
        SnakeDirection = Direction.Right;
        AddSnake();
        AddFood();
    }

    private void AddSnake()
    {
        int r = Rows / 2;

        for (int c = 1; c<=3; c++)
        {
            Grid[r,c] = GridValue.Snake;
            snakePositions.AddFirst(new Position(r,c));
        }
    }

    private IEnumerable<Position> EmptyPositions()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Grid[r,c] == GridValue.Empty)
                {
                    yield return new Position(r,c);
                }
            }
        }
    }

    public GameState MoveSnake(Direction dir)
    {
        var newHead = SnakeHead.Translate(dir);
        var newGrid = (GridValue[,])Grid.Clone();
        var newScore = Score;
        var newIsGameOver = IsGameOver;

        if (newGrid[newHead.Row, newHead.Col] == GridValue.Food)
        {
            newScore++;
            newGrid[SnakeTail.Row, SnakeTail.Col] = GridValue.Empty;
        }
        else if (newGrid[newHead.Row, newHead.Col] == GridValue.Snake)
        {
            newIsGameOver = true;
        }

        newGrid[newHead.Row, newHead.Col] = GridValue.Snake;
        newGrid[SnakeHead.Row, SnakeHead.Col] = GridValue.Empty;

        return new GameState(Rows, Cols, newGrid, newHead, newIsGameOver ? SnakeTail : SnakeTail.Translate(SnakeDirection), newIsGameOver ? SnakeDirection : dir, newScore, newIsGameOver);
    }

    public GameState AddFood(Position pos)
    {
        var newGrid = (GridValue[,])Grid.Clone();
        newGrid[pos.Row, pos.Col] = GridValue.Food;
        return new GameState(Rows, Cols, newGrid, SnakeHead, SnakeTail, SnakeDirection, Score, IsGameOver);
    }

    public void AddFood()
    {
        List<Position> empty = new List<Position>(EmptyPositions());
        if (empty.Count == 0)
        {
            return;
        }

        Position pos = empty[random.Next(empty.Count)];
        Grid[pos.Row, pos.Col] = GridValue.Food;
    }

    public GameState RemoveFood(Position pos)
    {
        var newGrid = (GridValue[,])Grid.Clone();
        newGrid[pos.Row, pos.Col] = GridValue.Empty;
        return new GameState(Rows, Cols, newGrid, SnakeHead, SnakeTail, SnakeDirection, Score, IsGameOver);
    }

    public Position HeadPosition()
    {
        return snakePositions.First.Value;
    }
    
    public Position TailPosition()
    {
        return snakePositions.Last.Value;
    }

    public IEnumerable<Position> SnakePositions()
    {
        return snakePositions;
    }

    private void AddHead(Position pos)
    {
        snakePositions.AddFirst(pos);
        Grid[pos.Row, pos.Col] = GridValue.Snake;
    }

    private void RemoveTail()
    {
        Position tail = snakePositions.Last.Value;
        Grid[tail.Row, tail.Col] = GridValue.Empty;
        snakePositions.RemoveLast();
    }

    private Direction GetLastDirection()
    {
        if (dirChanges.Count == 0)
            return SnakeDirection;

        return dirChanges.Last.Value;
    }

    private bool CanChangeDirection(Direction newDir)
    { 
        if(dirChanges.Count == 2)
            return false;

        Direction lastDir = GetLastDirection();
        return newDir != lastDir && newDir != lastDir.Opposite();
    }

    public void ChangeDirection(Direction dir)
    {
        if (CanChangeDirection(dir))
            dirChanges.AddLast(dir);
    }

    private bool IsOutside(Position pos)
    {
        return pos.Row < 0 || pos.Row >= Rows || pos.Col < 0 || pos.Col >= Cols;
    }

    private GridValue WillHit(Position newHeadPos)
    {
        if(IsOutside(newHeadPos))
            return GridValue.Outside;
        
        if (newHeadPos == TailPosition())
            return GridValue.Empty;
        
        return Grid[newHeadPos.Row, newHeadPos.Col];
    }

    public void Move()
    {
        if(dirChanges.Count > 0)
        {
            SnakeDirection = dirChanges.First.Value;
            dirChanges.RemoveFirst();
        }

        Position newHeadPos = HeadPosition().Translate(SnakeDirection);
        GridValue hit = WillHit(newHeadPos);

        if (hit == GridValue.Outside || hit == GridValue.Snake)
        {
            IsGameOver = true;
        }else if(hit == GridValue.Empty)
        {
            RemoveTail();
            AddHead(newHeadPos);
        }else if( hit == GridValue.Food)
        {
            AddHead(newHeadPos);
            Score++;
            AddFood();
        }
    }
}
