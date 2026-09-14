namespace zyte;


class Position(string filename)
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

    public Position Copy()
    {
        return new(FileName)
        {
            Index = Index,
            Line = Line,
            Column = Column
        };
    }
}