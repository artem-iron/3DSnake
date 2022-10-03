using _3DSnake.Models;

var exit = false;
var playField = new PlayFieldModel(18);
var debugInfo = "";

Console.CursorVisible = false;

Task.Factory.StartNew(() =>
{
    while (!exit && !playField.GameOver)
    {
        var key = Console.ReadKey();

        debugInfo += $"{key.Key}, {key.KeyChar}, {key.Modifiers}\n";

        if (key.Key == ConsoleKey.Q)
        {
            exit = true;
        }

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                playField.Direction = Direction.Up;
                break;
            case ConsoleKey.DownArrow:
                playField.Direction = Direction.Down;
                break;
            case ConsoleKey.LeftArrow:
                playField.Direction = Direction.Left;
                break;
            case ConsoleKey.RightArrow:
                playField.Direction = Direction.Right;
                break;
            case ConsoleKey.Z:
                playField.Direction = Direction.Forward;
                break;
            case ConsoleKey.X:
                playField.Direction = Direction.Backward;
                break;
            case ConsoleKey.W:
                playField.Side = Sides.TopYX;
                break;
            case ConsoleKey.S:
                playField.Side = Sides.FrontZX;
                break;
            case ConsoleKey.D:
                playField.Side = Sides.RightYZ;
                break;
            default:
                break;
        }
    }
});

while (!exit && !playField.GameOver)
{
    Console.SetCursorPosition(0, 0);

    Console.WriteLine(playField.RenderFrame());

    debugInfo += playField.GetSnakeString();

    Console.WriteLine(debugInfo);

    debugInfo = "";

    playField.Update();

    Thread.Sleep(100);
}
