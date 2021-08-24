namespace Words.Web.Entities
{
    using System;

    public record Query(
        string Type,
        string Text,
        int ElapsedMilliseconds,
        DateTime CreatedDate,
        string UserAgent,
        string UserHostAddress,
        int BrowserScreenPixelsHeight,
        int BrowserScreenPixelsWidth);
}