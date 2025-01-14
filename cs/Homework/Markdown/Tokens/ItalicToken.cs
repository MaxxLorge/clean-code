﻿namespace Markdown.Tokens
{
    public class ItalicToken : MarkdownToken
    {
        public ItalicToken(string value, string selector, int paragraphIndex, int startIndex) : base(value, selector, paragraphIndex, startIndex)
        {
        }

        public override string OpenHtmlTag => "<i>";
        public override string CloseHtmlTag => "</i>";
    }
}