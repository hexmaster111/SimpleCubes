using System.Diagnostics;
using SDL2;
using static SDL2.SDL;

internal class Program
{
    public static SDLRenderer Renderer;
    private static GameEngineState GameEngineState;
    private static GameBoard GameBoard;
    private static Ui Ui;


    private static void Main(string[] args)
    {
        GameBoard = new GameBoard(100);
        Renderer = new SDLRenderer(HandleEvent, Render, 800, 600);
        GameEngineState = new GameEngineState();
        Ui = new Ui();
        Renderer.Run();
    }

    private static void Render(RenderArgs obj)
    {
        Ui.Update(ref GameEngineState, ref GameBoard);
        if (!GameEngineState.IsPaused)
        {
            GameBoard.Update();
        }

        if (GameEngineState.IsMouseDown)
        {
            // GameEngineState.IsPaused = true;
            GameBoard.SetCellState(GameEngineState.MouseLocation, true);
        }
        else if (!GameEngineState.IsMouseDown)
        {
            // GameEngineState.IsPaused = false;
        }

        GameBoard.Render(obj, ref GameEngineState);
        Ui.Render(obj, GameEngineState);

    }

    private static void HandleEvent(SDL.SDL_Event obj)
    {
        //TODO: Handle mouse events
        Console.WriteLine(obj.type);
        switch (obj.type)
        {
            case SDL_EventType.SDL_MOUSEMOTION:
                {
                    GameEngineState.SetMouseLocation(obj.motion);
                    break;
                }
            case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                {
                    GameEngineState.IsMouseDown = true;
                    break;
                }
            case SDL_EventType.SDL_MOUSEBUTTONUP:
                {
                    GameEngineState.IsMouseDown = false;
                    break;
                }
            case SDL_EventType.SDL_KEYDOWN:
                {
                    if (obj.key.keysym.sym == SDL_Keycode.SDLK_SPACE)
                    {
                        GameEngineState.IsPaused = !GameEngineState.IsPaused;
                    }

                    if (obj.key.keysym.sym == SDL_Keycode.SDLK_c)
                    {
                        GameBoard.Clear();
                    }

                    if (obj.key.keysym.sym == SDL_Keycode.SDLK_s)
                    {
                        GameBoard.Update();
                    }


                    if (obj.key.keysym.sym == SDL_Keycode.SDLK_r)
                    {
                        GameBoard = new GameBoard(100);
                    }

                    break;
                }
        }
    }
}

public class Ui
{
    private SDL_Rect PlayPauseButton;
    private SDL_Rect StepButton;
    private SDL_Rect ClearButton;
    private bool IsLayedOut = false;
    private SDL_Color PlayButtonColorPlaying = new SDL_Color() { a = 255, r = 0, g = 255, b = 0 };
    private SDL_Color PlayButtonColorPaused = new SDL_Color() { a = 255, r = 255, g = 0, b = 0 };
    private SDL_Color StepButtonColor = new SDL_Color() { a = 255, r = 0, g = 255, b = 255 };
    private SDL_Color ClearButtonColor = new SDL_Color() { a = 255, r = 255, g = 255, b = 0 };

    public Ui()
    {
    }

    private void Verifylayout(RenderArgs obj)
    {
        if (!IsLayedOut)
        {
            int buttonWidth = obj.ScreenWidth_Px / 3;
            int buttonHeight = obj.ScreenHeight_px / 10;
            PlayPauseButton = new SDL_Rect() { x = buttonWidth, y = 0, w = buttonWidth, h = buttonHeight };
            StepButton = new SDL_Rect() { x = buttonWidth * 2, y = 0, w = buttonWidth, h = buttonHeight };
            ClearButton = new SDL_Rect() { x = 0, y = 0, w = buttonWidth, h = buttonHeight };
            IsLayedOut = true;
        }

    }

    internal void Render(RenderArgs obj, in GameEngineState gameEngineState)
    {
        Verifylayout(obj);
        if (gameEngineState.IsPaused)
            PlayButtonColorPaused.SetAsActiveColor(obj.Renderer);
        else PlayButtonColorPlaying.SetAsActiveColor(obj.Renderer);
        SDL_RenderFillRect(obj.Renderer, ref PlayPauseButton);

        StepButtonColor.SetAsActiveColor(obj.Renderer);
        SDL_RenderFillRect(obj.Renderer, ref StepButton);

        ClearButtonColor.SetAsActiveColor(obj.Renderer);
        SDL_RenderFillRect(obj.Renderer, ref ClearButton);

        if (gameEngineState.IsMouseDown)
            SDL_SetRenderDrawColor(obj.Renderer, 255, 0, 0, 255);
        else SDL_SetRenderDrawColor(obj.Renderer, 0, 0, 255, 255);
        SDL_RenderFillRect(obj.Renderer, ref gameEngineState.MouseRect);
    }




    internal void Update(ref GameEngineState gameEngineState, ref GameBoard gameBoard)
    {
        if (gameEngineState.IsMouseDown)
        {
            if (SDL_HasIntersection(ref gameEngineState.MouseRect, ref PlayPauseButton) == SDL_bool.SDL_TRUE)
            {
                gameEngineState.IsMouseDown = false;
                gameEngineState.IsPaused = !gameEngineState.IsPaused;
                Console.WriteLine(gameEngineState.IsPaused ? "Paused" : "Playing");
            }
            else if (SDL_HasIntersection(ref gameEngineState.MouseRect, ref StepButton) == SDL_bool.SDL_TRUE)
            {
                gameEngineState.IsMouseDown = false;
                gameEngineState.IsPaused = false;
                gameBoard.Update();
                gameEngineState.IsPaused = true;
                Console.WriteLine("Step");
            }
            else if (SDL_HasIntersection(ref gameEngineState.MouseRect, ref ClearButton) == SDL_bool.SDL_TRUE)
            {
                gameEngineState.IsMouseDown = false;
                gameBoard.Clear();
                Console.WriteLine("Clear");
            }
        }
    }
}


public class GameEngineState
{
    public bool IsPaused;

    public bool IsMouseDown;
    public int GamePixelScaleWidth;
    public int GamePixelScaleHeight;
    public SDL_Point MouseLocation;


    internal void SetMouseLocation(SDL_MouseMotionEvent motion)
    {
        if (GamePixelScaleHeight == 0) return;
        MouseLocation = new SDL_Point() { x = motion.x, y = motion.y };
        MouseRect = new SDL_Rect()
        {
            x = MouseLocation.x - (MouseLocation.x % GamePixelScaleWidth),
            y = MouseLocation.y - (MouseLocation.y % GamePixelScaleHeight),
            w = GamePixelScaleWidth,
            h = GamePixelScaleHeight
        };

    }

    public SDL_Rect MouseRect;
}

public class GameBoard
{
    private GameCell[][] GameCells;
    private int sizeSq;


    public GameBoard(int sizeSq)
    {
        this.sizeSq = sizeSq;
        Clear();
    }

    internal void Clear(bool random = true)
    {
        GameCells = new GameCell[sizeSq][];
        for (int i = 0; i < GameCells.Length; i++)
        {
            GameCells[i] = new GameCell[sizeSq];
            for (int j = 0; j < GameCells[i].Length; j++)
            {
                GameCells[i][j] = new GameCell();
                if (random)
                {
                    if (Random.Shared.Next(0, 4) == 1)
                    {
                        GameCells[i][j].Color = new SDL_Color() { a = 255, r = 255, g = 255, b = 255 };
                    }
                }
            }
        }
    }

    internal void Render(RenderArgs obj, ref GameEngineState gameEngineState)
    {
        int sandHeight = obj.ScreenHeight_px / GameCells.Length;
        int sandWidth = obj.ScreenWidth_Px / GameCells.First().Length;

        gameEngineState.GamePixelScaleHeight = sandHeight;
        gameEngineState.GamePixelScaleWidth = sandWidth;

        for (int rowIndex = 0; rowIndex < GameCells.Length; rowIndex++)
        {
            GameCell[]? cellRow = GameCells[rowIndex];
            for (int colIndex = 0; colIndex < cellRow.Length; colIndex++)
            {
                GameCell? cell = cellRow[colIndex];

                if (!cell.HasBeenPlaced) cell.Place(new SDL_Rect()
                {
                    x = colIndex * sandWidth,
                    y = rowIndex * sandHeight,
                    w = sandWidth,
                    h = sandHeight
                });

                SDL_SetRenderDrawColor(obj.Renderer, cell.Color.r, cell.Color.g, cell.Color.b, cell.Color.a);
                SDL_RenderFillRect(obj.Renderer, ref GameCells[rowIndex][colIndex].RenderLocation);
            };
        }
    }

    internal void SetCellState(SDL_Point mouseLocation, bool v)
    {

        int cellX = mouseLocation.x / GameCells.First().First().RenderLocation.w;
        int cellY = mouseLocation.y / GameCells.First().First().RenderLocation.h;

        if (cellY > GameCells.First().Count()) return;
        if (cellX > GameCells.First().Count()) return;

        GameCells[cellY][cellX].Color = v ? new SDL_Color() { a = 255, r = 255, g = 255, b = 255 } : new SDL_Color() { a = 255, r = 0, g = 0, b = 0 };
    }

    internal void Update()
    {
        for (int cellRowIndex = 0; cellRowIndex < GameCells.Length; cellRowIndex++)
        {
            GameCell[]? cellRow = GameCells[cellRowIndex];
            for (int cellColIndex = 0; cellColIndex < cellRow.Length; cellColIndex++)
            {
                GameCell? cell = cellRow[cellColIndex];

                CellsAround cellsNearby = new CellsAround(
                    cellRowIndex > 0 && cellColIndex > 0 ? GameCells[cellRowIndex - 1][cellColIndex - 1] : null,
                    cellRowIndex > 0 ? GameCells[cellRowIndex - 1][cellColIndex] : null,
                    cellRowIndex > 0 && cellColIndex < GameCells.Length - 1 ? GameCells[cellRowIndex - 1][cellColIndex + 1] : null,
                    cellColIndex > 0 ? GameCells[cellRowIndex][cellColIndex - 1] : null,
                    cellColIndex < GameCells.Length - 1 ? GameCells[cellRowIndex][cellColIndex + 1] : null,
                    cellRowIndex < GameCells.Length - 1 && cellColIndex > 0 ? GameCells[cellRowIndex + 1][cellColIndex - 1] : null,
                    cellRowIndex < GameCells.Length - 1 ? GameCells[cellRowIndex + 1][cellColIndex] : null,
                    cellRowIndex < GameCells.Length - 1 && cellColIndex < GameCells.Length - 1 ? GameCells[cellRowIndex + 1][cellColIndex + 1] : null
                );

                cell.Update(cellRowIndex, cellColIndex, cellsNearby);
            }
        }


        for (int cellRowIndex = 0; cellRowIndex < GameCells.Length; cellRowIndex++)
        {
            GameCell[]? cellRow = GameCells[cellRowIndex];
            for (int cellColIndex = 0; cellColIndex < cellRow.Length; cellColIndex++)
            {
                GameCell? cell = cellRow[cellColIndex];
                if (cell == null) continue;
                cell.SetNewColor();
            }
        }
    }
}

public record CellsAround(GameCell? TopLeft, GameCell? Top, GameCell? TopRight,
                          GameCell? Left, GameCell? Right,
                           GameCell? BottomLeft, GameCell? Bottom, GameCell? BottomRight);

public class GameCell
{
    public Rules Rules;
    public SDL_Color Color = new SDL_Color() { a = 255, r = 0, g = 0, b = 0 };
    private SDL_Color _colorBuffer;
    public SDL_Rect RenderLocation;
    public bool HasBeenPlaced = false;

    internal void Place(SDL_Rect sDL_Rect)
    {
        RenderLocation = sDL_Rect;
        HasBeenPlaced = true;
    }

    public void SetNewColor()
    {
        Color = _colorBuffer;
    }

    private int GetCellsAliveAroundCount(CellsAround cellsAround)
    {
        int count = 0;
        if (cellsAround.TopLeft?.Color.r == 255) count++;
        if (cellsAround.Top?.Color.r == 255) count++;
        if (cellsAround.TopRight?.Color.r == 255) count++;
        if (cellsAround.Left?.Color.r == 255) count++;
        if (cellsAround.Right?.Color.r == 255) count++;
        if (cellsAround.BottomLeft?.Color.r == 255) count++;
        if (cellsAround.Bottom?.Color.r == 255) count++;
        if (cellsAround.BottomRight?.Color.r == 255) count++;
        return count;
    }

    internal void Update(int cellRowIndex, int cellColIndex, CellsAround cellsNear)
    {
        int cellsAliveAroundCount = GetCellsAliveAroundCount(cellsNear);

        switch (cellsAliveAroundCount)
        {
            case 0:
            case 1:
                _colorBuffer = new SDL_Color() { a = 255, r = 0, g = 0, b = 0 };
                break;
            case 2:
                break;
            case 3:
                _colorBuffer = new SDL_Color() { a = 255, r = 255, g = 255, b = 255 };
                break;
            default:
                _colorBuffer = new SDL_Color() { a = 255, r = 0, g = 0, b = 0 };
                break;
        }

    }

    public override string ToString()
    {
        return (Color.r == 255 ? "Alive" : "Dead") + $"x:{RenderLocation.x} y: {RenderLocation.y}";
    }
}


public class Rules
{
}
