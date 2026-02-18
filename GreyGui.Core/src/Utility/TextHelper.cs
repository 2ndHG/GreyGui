using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GreyGui;

public static class TextHelper
{
    public static HashSet<char> LineStartForbiddenChars { get; private set; } = [.. "）〉》」』】〕）］｝〕〉》」』】〕！），．：；？。、"];
    public static HashSet<char> LineEndForbiddenChars { get; private set; } = [.. "（〈《「『【〔（［｛〔〈《「『【〔＄（［｛￡￥"];

    public static bool IsLineStartForbidden(char c) => LineStartForbiddenChars.Contains(c);
    public static bool IsLineEndForbidden(char c) => LineEndForbiddenChars.Contains(c);
    public static bool IsCjk(char c)
    {
        return (c >= '\u4E00' && c <= '\u9FFF') ||
            (c >= '\u3400' && c <= '\u4DBF') ||
            (c >= '\u3000' && c <= '\u303F') ||
            (c >= '\u3040' && c <= '\u30FF');
    }
}