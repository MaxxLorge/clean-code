﻿using System;

namespace Markdown
{
    public abstract class TokenIdentifier
    {
        protected TokenIdentifier(string tag, Func<TemporaryToken, Token> tokenCreator)
        {
            Tag = tag;
            TokenCreator = tokenCreator;
        }

        protected string Tag { get; }
        protected Func<TemporaryToken, Token> TokenCreator { get; }

        public bool Identify(string[] paragraphs, TemporaryToken temporaryToken, out Token identifiedToken)
        {
            if (IsValid(paragraphs, temporaryToken))
            {
                identifiedToken = TokenCreator(temporaryToken);
                return true;
            }

            identifiedToken = null;
            return false;
        }

        protected abstract bool IsValid(string[] paragraphs, TemporaryToken temporaryToken);

    }
}