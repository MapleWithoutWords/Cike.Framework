namespace Cike.Core.Models;

public class Brackets
{
    public char Open { get; }

    public char Close { get; }

    public Brackets(char open, char close)
    {
        Open = open;
        Close = close;
    }

    public static Brackets Angle => new('<', '>');

    public static Brackets Square => new('[', ']');
}