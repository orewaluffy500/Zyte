using zyte;

string filename = args.Length > 0 && args[0].EndsWith(".zy") ? args[0] : "code.zy";

if (!File.Exists(filename))
{
    Console.Error.WriteLine($"unable to find code file '{filename}'");
    Environment.Exit(5);
}

string source = File.ReadAllText(filename);

Lexer lexer = new(source, filename);
Token[] tokens = lexer.MakeTokens();

if (args.Contains("-tok")) Console.WriteLine(string.Join(' ', tokens));

Parser parser = new(tokens, filename);
ASTNode tree = parser.Parse();

if (args.Contains("-ast")) Console.WriteLine(tree);

Interpreter interpreter = new((BodyNode) tree);
interpreter.Interpret();