namespace zyte;


class KeywordClass
{
    public static readonly string[] Keywords = [
        "wln", "w",                                 // OUTPUT
        "rei", "rech", "rek",                       // INPUT
        "halt",                                     // SYSTEM-LEVEL
        "cpy",                                      // MEMORY-RELATED (REGISTERS)
        "if", "else", "elseif", "then",             // FLOW-RELATED
        "while", "break", "next",                   // LOOP-RELATED
        "eq", "gt", "lt"                            // CONDITIONAL
    ];
}