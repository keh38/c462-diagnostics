using System;
using System.Collections.Generic;
using System.Text;

namespace KLib.Expressions
{
    // -------------------------------------------------------------------------
    // TokenType
    // -------------------------------------------------------------------------
    // Every distinct thing the tokenizer can produce. The parser will switch on
    // these values — having them as an enum means the compiler catches typos and
    // a switch with no default will warn you about unhandled cases.

    public enum TokenType
    {
        // Literals
        Number,         // 42   3.14   .5

        // Names — identifiers cover function names (linspace, THR, M),
        // ear codes (L, R, B), and metric names (TinnitusCF).
        // The parser decides which is which from context.
        Identifier,

        // Arithmetic operators
        Plus,           // +
        Minus,          // -
        Star,           // *
        Slash,          // /
        Caret,          // ^

        // Grouping and structure
        LParen,         // (
        RParen,         // )
        LBracket,       // [
        RBracket,       // ]

        // Separators
        Colon,          // :   (range operator, a:b or a:step:b)
        Comma,          // ,   (function argument separator)

        // Sentinel — always the last token in the list
        EOF
    }

    // -------------------------------------------------------------------------
    // Token
    // -------------------------------------------------------------------------
    // A single token: its type, its raw text from the source string, and its
    // position. Position is only used for error messages — it's cheap to store
    // and invaluable when something goes wrong.

    public class Token
    {
        public TokenType Type     { get; }
        public string    Text     { get; }   // raw characters from source
        public int       Position { get; }   // zero-based index into source string

        public Token(TokenType type, string text, int position)
        {
            Type     = type;
            Text     = text;
            Position = position;
        }

        // Convenience: test type without pulling Type into a local variable
        public bool Is(TokenType t) => Type == t;

        public override string ToString() => $"[{Type} \"{Text}\" @{Position}]";
    }

    // -------------------------------------------------------------------------
    // Tokenizer
    // -------------------------------------------------------------------------
    // Single public method: Tokenize(string) → List<Token>
    //
    // Design notes:
    //   - _pos is the cursor; Peek() looks without consuming, Advance() consumes.
    //   - On an unrecognised character we throw immediately with position info.
    //     It's better to fail loudly here than to produce a token list that
    //     confuses the parser.

    public class Tokenizer
    {
        private string _source;
        private int    _pos;

        // ------------------------------------------------------------------
        // Public entry point
        // ------------------------------------------------------------------

        public List<Token> Tokenize(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _pos    = 0;

            var tokens = new List<Token>();

            while (!AtEnd())
            {
                SkipWhitespace();
                if (AtEnd()) break;

                var token = ReadNextToken();
                tokens.Add(token);
            }

            // Always close with EOF so the parser has a clean sentinel and
            // never has to range-check the token list.
            tokens.Add(new Token(TokenType.EOF, "", _pos));
            return tokens;
        }

        // ------------------------------------------------------------------
        // Core scanning
        // ------------------------------------------------------------------

        private Token ReadNextToken()
        {
            int start = _pos;
            char c    = Peek();

            // --- Numbers ---------------------------------------------------
            // A number starts with a digit or a bare decimal point.
            // Examples: 42  3.14  .5
            // We do NOT handle leading minus here — that's a unary operator
            // and the parser handles it in the grammar (unary → '-' unary).
            // This keeps the tokenizer simple and avoids ambiguity between
            // binary minus (a - b) and unary minus (-b).
            if (char.IsDigit(c) || (c == '.' && IsDigitAt(_pos + 1)))
            {
                return ReadNumber(start);
            }

            // --- Identifiers -----------------------------------------------
            // Identifiers start with a letter or underscore and may contain
            // letters, digits, underscores, and dots.
            // The dot is included because metric names like "Tinnitus.CF" are
            // legal in the existing system.
            if (char.IsLetter(c) || c == '_')
            {
                return ReadIdentifier(start);
            }

            // --- Single-character tokens ------------------------------------
            Advance(); // consume the character we already peeked at

            switch (c)
            {
                case '+': return new Token(TokenType.Plus,     "+", start);
                case '-': return new Token(TokenType.Minus,    "-", start);
                case '*': return new Token(TokenType.Star,     "*", start);
                case '/': return new Token(TokenType.Slash,    "/", start);
                case '^': return new Token(TokenType.Caret,    "^", start);
                case '(': return new Token(TokenType.LParen,   "(", start);
                case ')': return new Token(TokenType.RParen,   ")", start);
                case '[': return new Token(TokenType.LBracket, "[", start);
                case ']': return new Token(TokenType.RBracket, "]", start);
                case ':': return new Token(TokenType.Colon,    ":", start);
                case ',': return new Token(TokenType.Comma,    ",", start);

                default:
                    throw new ExpressionException(
                        $"Unexpected character '{c}' at position {start}");
            }
        }

        // ------------------------------------------------------------------
        // Number reader
        // ------------------------------------------------------------------
        // Consumes an optional integer part, an optional decimal point and
        // fractional part. Does not handle scientific notation (e.g. 1e5) —
        // add that here if researchers ever need it.

        private Token ReadNumber(int start)
        {
            var sb = new StringBuilder();

            // Integer part
            while (!AtEnd() && char.IsDigit(Peek()))
                sb.Append(Advance());

            // Fractional part
            if (!AtEnd() && Peek() == '.' && IsDigitAt(_pos + 1))
            {
                sb.Append(Advance()); // consume '.'
                while (!AtEnd() && char.IsDigit(Peek()))
                    sb.Append(Advance());
            }

            return new Token(TokenType.Number, sb.ToString(), start);
        }

        // ------------------------------------------------------------------
        // Identifier reader
        // ------------------------------------------------------------------

        private Token ReadIdentifier(int start)
        {
            var sb = new StringBuilder();

            while (!AtEnd() && (char.IsLetterOrDigit(Peek()) || Peek() == '_' || Peek() == '.'))
                sb.Append(Advance());

            return new Token(TokenType.Identifier, sb.ToString(), start);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private bool AtEnd()              => _pos >= _source.Length;
        private char Peek()               => _source[_pos];
        private char Advance()            => _source[_pos++];
        private bool IsDigitAt(int index) => index < _source.Length && char.IsDigit(_source[index]);

        private void SkipWhitespace()
        {
            while (!AtEnd() && char.IsWhiteSpace(Peek()))
                Advance();
        }
    }

    // -------------------------------------------------------------------------
    // ExpressionException
    // -------------------------------------------------------------------------
    // A dedicated exception type so callers can distinguish expression errors
    // from other exceptions without parsing the message string.
    // The parser will also throw this — defining it here so both files can use it.

    public class ExpressionException : Exception
    {
        public ExpressionException(string message) : base(message) { }
    }
}
