namespace zyte;


struct Position(string filename)
{
    public int Index = 0;
    public int Line = 1;
    public int Column = 1;
    public string FileName = filename;

    public void Next(bool isNewline)
    {
        Index++;
        Column++;

        if (isNewline)
        {
            Column = 1;
            Line++;
        }
    }

    public override readonly string ToString()
    {
        return $"line {Line}, column {Column} of '{FileName}'";
    }
}