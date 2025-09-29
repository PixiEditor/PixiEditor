using Avalonia.Input;

namespace PixiEditor.Models.Commands.Templates.Providers.Parsers;

public static class KeyParser
{
    public static bool TryParseSpecial(string key, out Key parsed)
    {
        switch (key.ToLower())
        {
            case "shift":
                parsed = Key.LeftShift;
                return true;
            case "ctrl":
                parsed = Key.LeftCtrl;
                return true;
            case "alt":
                parsed = Key.LeftAlt;
                return true;
            case "win":
                parsed = Key.LWin;
                return true;
            case ".":
                parsed = Key.OemPeriod;
                return true;
            case ",":
                parsed = Key.OemComma;
                return true;
            case "+":
                parsed = Key.OemPlus;
                return true;
            case "-":
                parsed = Key.OemMinus;
                return true;
            case "/":
                parsed = Key.OemQuestion;
                return true;
            case "*":
                parsed = Key.Multiply;
                return true;
            case "\\":
                parsed = Key.Oem5;
                return true;
            case "'":
                parsed = Key.OemQuotes;
                return true;
            case "\"":
                parsed = Key.Oem7;
                return true;
            case ";":
                parsed = Key.OemSemicolon;
                return true;
            case ":":
                parsed = Key.Oem1;
                return true;
            case "<":
                parsed = Key.Oem102;
                return true;
            case ">":
                parsed = Key.Oem102;
                return true;
            case "~":
                parsed = Key.Oem3;
                return true;
            case "!":
                parsed = Key.D1;
                return true;
            case "@":
                parsed = Key.D2;
                return true;
            case "#":
                parsed = Key.D3;
                return true;
            case "$":
                parsed = Key.D4;
                return true;
            case "%":
                parsed = Key.D5;
                return true;
            case "^":
                parsed = Key.D6;
                return true;
            case "&":
                parsed = Key.D7;
                return true;
            case "[" or "{":
                parsed = Key.OemOpenBrackets;
                return true;
            case "]" or "}":
                parsed = Key.OemCloseBrackets;
                return true;
            case "_":
                parsed = Key.OemMinus;
                return true;
            case "=":
                parsed = Key.OemPlus;
                return true;
            case "(":
                parsed = Key.D9;
                return true;
            case ")":
                parsed = Key.D0;
                return true;
            case "|":
                parsed = Key.Oem5;
                return true;
            default:
                parsed = Key.None;
                return false;
        }
    }
}
