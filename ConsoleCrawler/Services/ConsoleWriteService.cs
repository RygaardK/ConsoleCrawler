using System.Text.RegularExpressions;

namespace Service;


public class ConsoleWriteService
{
    /// <summary>
    /// usage: WriteColor("This is my [message] with inline [color] changes.", ConsoleColor.Yellow);
    /// </summary>
    /// <param name="message"></param>
    /// <param name="color"></param>
    public void WriteTextColor(string message, ConsoleColor color)
    {
        var pieces = Regex.Split(message, @"(\[[^\]]*\])");

        for (int i = 0; i < pieces.Length; i++)
        {
            string piece = pieces[i];

            if (piece.StartsWith("[") && piece.EndsWith("]"))
            {
                Console.ForegroundColor = color;
                //piece = piece.Substring(1, piece.Length - 2);
                piece = piece[1..^1]; // Intellisense
            }

            Console.Write(piece);
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Everything in the message will contain the settings.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="backgroundColor"></param>
    /// <param name="textColor"></param>
    public void WriteBoxColor(string message, ConsoleColor backgroundColor, ConsoleColor textColor)
    {
        Console.BackgroundColor = backgroundColor;
        Console.ForegroundColor = textColor;
        Console.Write(message);
        Console.ResetColor();
        Console.WriteLine();
    }

    public void WriteBoxDebug(string message, Exception ex)
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("### " + message + " ###");
        Console.ResetColor();
        Console.WriteLine(" - " + ex.ToString());
        Console.WriteLine();
    }
}
