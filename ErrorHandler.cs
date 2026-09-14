namespace zyte;



class ErrorHandler
{
    public static void RTError(string label, string message, Position site)
    {
        Console.Error.WriteLine($"runtime error: {label}");
        Console.Error.WriteLine($"  what: {message}");
        Console.Error.WriteLine($"  where: line {site.Line}, column {site.Column} in '{site.FileName}'");
        Environment.Exit(3);
    }

    public static void SyntaxError(string label, string message, Position site)
    {
        Console.Error.WriteLine($"syntax error: {label}");
        Console.Error.WriteLine($"  what: {message}");
        Console.Error.WriteLine($"  where: line {site.Line}, column {site.Column} in '{site.FileName}'");
        Environment.Exit(3);
    }
}