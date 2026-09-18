namespace zyte;


class KeywordClass
{
    public static readonly string[] Keywords = [
        "wln", "w", "_",                                            // OUTPUT
        "rei", "rech", "rek",                                      // INPUT
        "halt",                                                   // SYSTEM-LEVEL
        "cpy",                                                   // MEMORY-RELATED (REGISTERS)
        "if", "else", "elseif", "then",                         // FLOW-RELATED
        "while", "break", "next", "for", "to", "step",         // LOOP-RELATED
        "and", "or", "xor", "not",                            // LOGICAL
        "eq", "gt", "lt",                                    // CONDITIONAL
        "func", "return", "out",                            // FUNC-RELATED
        "fetch",                                           // MISC
    ];
}