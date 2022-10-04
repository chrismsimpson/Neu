
namespace Neu;

public partial class Span {

    public FileId FileId { get; init; }

    public int Start { get; init; }
    
    public int End { get; init; }

    ///

    public Span(
        FileId fileId,
        int start,
        int end) {

        this.FileId = fileId;
        this.Start = start;
        this.End = end;
    }
}

public partial class SpanFunctions {

    public static bool Eq(
        Span l,
        Span r) {

        return l.FileId == r.FileId
            && l.Start == r.Start
            && l.End == r.End;
    }
}

///

public partial class Token {

    public Span Span { get; init; }

    ///

    public Token(
        Span span) {

        this.Span = span;
    }
}

    public partial class SingleQuotedStringToken: Token {

        public String Value { get; init; }

        ///

        public SingleQuotedStringToken(
            String value,
            Span span)
            : base(span) {

            this.Value = value;
        }
    }

    public partial class QuotedStringToken: Token {

        public String Value { get; init; }

        ///

        public QuotedStringToken(
            String value,
            Span span)
            : base(span) {

            this.Value = value;
        }
    }

    public partial class NumberToken: Token {

        public Int64 Value { get; init; }

        ///

        public NumberToken(
            Int64 value,
            Span span)
            : base(span) {

            this.Value = value;
        }
    }

    public partial class NameToken: Token {

        public String Value { get; init; }

        ///

        public NameToken(
            String value,
            Span span)
            : base(span) {

            this.Value = value;
        }
    }

    public partial class SemicolonToken: Token {

        public SemicolonToken(Span span)
            : base(span) { }
    }

    public partial class ColonToken: Token {

        public ColonToken(Span span)
            : base(span) { }
    }

    public partial class LParenToken: Token {

        public LParenToken(Span span)
            : base(span) { }
    }

    public partial class RParenToken: Token {

        public RParenToken(Span span)
            : base(span) { }
    }

    public partial class LCurlyToken: Token {

        public LCurlyToken(Span span)
            : base(span) { }
    }

    public partial class RCurlyToken: Token {

        public RCurlyToken(Span span)
            : base(span) { }
    }

    public partial class LSquareToken: Token {

        public LSquareToken(Span span)
            : base(span) { }
    }

    public partial class RSquareToken: Token {

        public RSquareToken(Span span)
            : base(span) { }
    }

    public partial class PercentToken: Token {

        public PercentToken(Span span)
            : base(span) { } 
    }

    public partial class PlusToken: Token {

        public PlusToken(Span span) 
            : base(span) { }
    }

    public partial class MinusToken: Token {

        public MinusToken(Span span)
            : base(span) { }
    }

    public partial class EqualToken: Token {

        public EqualToken(Span span) 
            : base(span) { }
    }

    public partial class PlusEqualToken: Token {

        public PlusEqualToken(Span span) 
            : base(span) { }
    }

    public partial class PlusPlusToken: Token {

        public PlusPlusToken(Span span) 
            : base(span) { }
    }

    public partial class MinusEqualToken: Token {

        public MinusEqualToken(Span span) 
            : base(span) { }
    }

    public partial class MinusMinusToken: Token {

        public MinusMinusToken(Span span) 
            : base(span) { }
    }

    public partial class AsteriskEqualToken: Token {

        public AsteriskEqualToken(Span span) 
            : base(span) { }
    }

    public partial class ForwardSlashEqualToken: Token {

        public ForwardSlashEqualToken(Span span) 
            : base(span) { }
    }

    public partial class PercentEqualToken: Token {

        public PercentEqualToken(
            Span span)
            : base(span) { }
    }

    public partial class NotEqualToken: Token {
        
        public NotEqualToken(Span span) 
            : base(span) { }
    }

    public partial class DoubleEqualToken: Token {
        
        public DoubleEqualToken(Span span) 
            : base(span) { }
    }

    public partial class GreaterThanToken: Token {

        public GreaterThanToken(Span span)
            : base(span) { }
    }

    public partial class GreaterThanOrEqualToken: Token {
        
        public GreaterThanOrEqualToken(Span span) 
            : base(span) { }
    }

    public partial class LessThanToken: Token {

        public LessThanToken(Span span)
            : base(span) { }
    }

    public partial class LessThanOrEqualToken: Token {
        
        public LessThanOrEqualToken(Span span) 
            : base(span) { }
    }

    public partial class AsteriskToken: Token {

        public AsteriskToken(Span span) 
            : base(span) { }
    }

    public partial class AmpersandToken: Token {

        public AmpersandToken(Span span)
            : base(span) { }
    }

    public partial class ForwardSlashToken: Token {

        public ForwardSlashToken(Span span) 
            : base(span) { }
    }

    public partial class ExclamationToken: Token {

        public ExclamationToken(Span span)
            : base(span) { }
    }

    public partial class QuestionToken: Token {

        public QuestionToken(
            Span span)
            : base(span) { }
    }

    public partial class CommaToken: Token {

        public CommaToken(Span span)
            : base(span) { }
    }

    public partial class PeriodToken: Token {

        public PeriodToken(Span span)
            : base(span) { }
    }

    public partial class EolToken: Token {

        public EolToken(Span span)
            : base(span) { }
    }

    public partial class EofToken: Token {

        public EofToken(Span span)
            : base(span) { }
    }

    public partial class UnknownToken: Token {

        public UnknownToken(Span span) 
            : base(span) { }
    }

public static partial class TokenFunctions {

    public static bool Eq(
        Token l, Token r) {

        switch (true) {

            case var _ when
                l is QuotedStringToken lq
                && r is QuotedStringToken rq:
                return lq.Value == rq.Value 
                    && SpanFunctions.Eq(lq.Span, rq.Span);

            case var _ when
                l is NumberToken ln
                && r is NumberToken rn:
                return ln.Value == rn.Value 
                    && SpanFunctions.Eq(ln.Span, rn.Span);

            case var _ when
                l is NameToken ln
                && r is NameToken rn:
                return ln.Value == rn.Value
                    && SpanFunctions.Eq(ln.Span, rn.Span);

            case var _ when l.GetType() == r.GetType():
                return SpanFunctions.Eq(l.Span, r.Span);

            default: 
                return false;
        }
    }
}

    ///


public static partial class LexerFunctions {

    public static (List<Token>, Error?) Lex(
        FileId fileId, 
        byte[] bytes) {

        var output = new List<Token>();

        var index = 0;

        Error? error = null;

        while (index < bytes.Length) {

            var b = bytes[index];

            var c = ToChar(b);

            switch (c) {

                case ';': {

                    var start = index;

                    index += 1;

                    output.Add(new SemicolonToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case ':': {

                    var start = index;

                    index += 1;

                    output.Add(new ColonToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '+': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new PlusEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                        else if (ToChar(bytes[index]) == '+') {

                            index += 1;

                            output.Add(new PlusPlusToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                    }

                    output.Add(new PlusToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '-': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new MinusEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                        else if (ToChar(bytes[index]) == '-') {

                            index += 1;

                            output.Add(new MinusMinusToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                    }

                    output.Add(new MinusToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '*': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new AsteriskEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                    }

                    output.Add(new AsteriskToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '/': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new ForwardSlashEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                        else if (ToChar(bytes[index]) == '/') {

                            // We are in a comment, skip it

                            while (index < bytes.Length) {

                                if (ToChar(bytes[index]) == '\n') {

                                    index += 1;

                                    break;
                                }

                                index += 1;
                            }

                            continue;
                        }
                    }

                    output.Add(new ForwardSlashToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '=': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new DoubleEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                    }

                    output.Add(new EqualToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '>': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new GreaterThanOrEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                    }

                    output.Add(new GreaterThanToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '<': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new LessThanOrEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                    }

                    output.Add(new LessThanToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '!': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new NotEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                    }

                    output.Add(new ExclamationToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '&': {

                    var start = index;

                    index += 1;

                    output.Add(new AmpersandToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '%': {

                    var start = index;

                    index += 1;

                    if (index < bytes.Length) {

                        if (ToChar(bytes[index]) == '=') {

                            index += 1;

                            output.Add(new PercentEqualToken(new Span(fileId, start, start + 2)));

                            continue;
                        }
                    }

                    output.Add(new PercentToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '?': {

                    var start = index;

                    index += 1;

                    output.Add(new QuestionToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case ',': {

                    var start = index;

                    index += 1;

                    output.Add(new CommaToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '.': {

                    var start = index;

                    index += 1;

                    output.Add(new PeriodToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '(': {

                    var start = index;

                    index += 1;

                    output.Add(new LParenToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case ')': {

                    var start = index;

                    index += 1;

                    output.Add(new RParenToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '{': {

                    var start = index;
                    
                    index += 1;

                    output.Add(new LCurlyToken(new Span(fileId, start, start + 1)));
                    
                    break;
                }

                ///

                case '}': {

                    var start = index;

                    index += 1;

                    output.Add(new RCurlyToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '[': {

                    var start = index;

                    index += 1;

                    output.Add(new LSquareToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case ']': {

                    var start = index;

                    index += 1;

                    output.Add(new RSquareToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                case '\r':
                case ' ':
                case '\t': {

                    // Ignore a stand-alone carriage return

                    index += 1;

                    break;
                }

                ///

                case '\n': {

                    // If the next character is a newline, we're looking at an EOL (end of line) token.

                    var start = index;

                    index += 1;

                    output.Add(new EolToken(new Span(fileId, start, start + 1)));

                    break;
                }

                ///

                default:  {

                    // Otherwise, try to consume a token.

                    var (token, lexErr) = LexItem(fileId, bytes, ref index);

                    error = error ?? lexErr;

                    output.Add(token);

                    break;
                }
            }
        }

        output.Add(new EofToken(new Span(fileId, index, index)));

        return (output, error);
    }

    ///

    public static (Token, Error?) LexItem(FileId fileId, byte[] bytes, ref int index) {

        Error? error = null;

        if (bytes[index] == '0' && index + 2 < bytes.Length && bytes[index + 1] == 'x') {
            
            // Hex number
            
            var start = index;

            index += 2;

            while (index < bytes.Length && bytes[index].IsAsciiHexDigit()) {

                index += 1;
            }

            var str = UTF8.GetString(bytes[(start + 2)..index]);

            Int64 number = 0;

            if (Int64.TryParse(str, System.Globalization.NumberStyles.HexNumber, null, out number)) {

                return (
                    new NumberToken(number, new Span(fileId, start, index)),
                    null);
            }
            else {

                return (
                    new UnknownToken(new Span(fileId, start, index)),
                    new ParserError(
                        "could not parse hex",
                        new Span(fileId, start, index)));
            }
        } 
        else if (bytes[index] == '0' && index + 2 < bytes.Length && bytes[index + 1] == 'b') {
            
            // Binary number
            
            var start = index;
            
            index += 2;
            
            while (index < bytes.Length && (bytes[index] == '0' || bytes[index] == '1')) {
                
                index += 1;
            }

            var str = UTF8.GetString(bytes[(start + 2)..index]);

            try {

                var number = ToInt64(str, 2);

                return (
                    new NumberToken(number, new Span(fileId, start, index)),
                    null);
            }
            catch {
                
                return (
                    new UnknownToken(new Span(fileId, start, index)), 
                    new ParserError(
                        "could not parse binary number", 
                        new Span(fileId, start, index)));
            }
        }
        else if (bytes[index].IsAsciiDigit()) {

            // Number

            var start = index;

            while (index < bytes.Length && bytes[index].IsAsciiDigit()) {

                index += 1;
            }

            var str = UTF8.GetString(bytes[start..index]);

            Int64 number = 0;

            if (Int64.TryParse(str, out number)) {

                return (
                    new NumberToken(number, new Span(fileId, start, index)),
                    null);
            }
            else {

                return (
                    new UnknownToken(new Span(fileId, start, index)),
                    new ParserError(
                        "could not parse int", 
                        new Span(fileId, start, index)));
            }
        }
        else if (ToChar(bytes[index]) == '\'') {

            // Character

            var start = index;

            index += 1;

            var escaped = false;

            while (index < bytes.Length && (escaped || bytes[index] != '\'')) {

                if (!escaped && bytes[index] == '\\') {

                    escaped = true;
                }
                else {

                    escaped = false;
                }

                index += 1;
            }

            if (index == bytes.Length || bytes[index] != '\'') {

                error = error ?? 
                    new ParserError(
                        "expected single quote", 
                        new Span(fileId, index, index));
            }

            // Everything but the quotes

            var str = UTF8.GetString(bytes[(start + 1)..index]);

            index += 1;

            var end = index;

            return (
                new SingleQuotedStringToken(
                    str, 
                    new Span(fileId, start, end)),
                error);
        }
        else if (ToChar(bytes[index]) == '"') {

            // Quoted string

            var start = index;

            index += 1;

            var escaped = false;

            while (index < bytes.Length && (escaped || ToChar(bytes[index]) != '"')) {

                if (!escaped && ToChar(bytes[index]) == '\\') {

                    escaped = true;
                }
                else {

                    escaped = false;
                }

                index += 1;
            }

            if (index == bytes.Length || ToChar(bytes[index]) != '"') {

                error = new ParserError(
                    "expected quote", 
                    new Span(fileId, index, index));
            }

            // Everything but the quotes

            var str = UTF8.GetString(bytes[(start + 1)..index]);

            index += 1;

            var end = index;

            return (
                new QuotedStringToken(
                    str, 
                    new Span(fileId, start, end)),
                error);
        }
        else if (bytes[index].IsAsciiAlphanumeric() || ToChar(bytes[index]) == '_') {

            // Symbol name

            var start = index;

            index += 1;

            var escaped = false;

            while (index < bytes.Length 
                && (bytes[index].IsAsciiAlphanumeric() || (ToChar(bytes[index]) == '_'))) {

                if (!escaped && ToChar(bytes[index]) == '\\') {

                    escaped = true;
                }
                else {

                    escaped = false;
                }

                index += 1;
            }

            // Everything but the quotes

            var str = UTF8.GetString(bytes[start..index]);

            return (
                new NameToken(
                    str, 
                    new Span(fileId, start, index)),
                error);
        }
        else {

            var span = new Span(fileId, index, index + 1);

            error = error ?? new ParserError("unknown character", span);

            index += 1;

            return (
                new UnknownToken(span),
                error);
        }
    }
}

public static partial class CharExtensions {

    public static bool IsAsciiAlphanumeric(
        this byte b) {

        var c = ToChar(b);

        return Char.IsLetterOrDigit(c);
    }

    public static bool IsAsciiDigit(
        this byte b) {

        switch (ToChar(b)) {

            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                return true;

            default:
                return false;
        }
    }

    public static bool IsAsciiHexDigit(
        this byte b) {

        switch (ToChar(b)) {

            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case 'a':
            case 'b':
            case 'c':
            case 'd':
            case 'e':
            case 'f':
            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
                return true;

            default:
                return false;
        }
    }
}