using System;

namespace Reusable.Markup
{
    [Flags]
    public enum MarkupFormattingOptions
    {
        None = 0,
        PlaceOpeningTagOnNewLine = 1,
        PlaceClosingTagOnNewLine = 2,
        PlaceBothTagsOnNewLine =
            PlaceOpeningTagOnNewLine |
            PlaceClosingTagOnNewLine,
        IsVoid = 4,
        CloseEmptyTag = 8
    }
}