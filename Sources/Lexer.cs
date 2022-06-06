
namespace Neu;

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

                            output.Add(new PlusEqualToken(new Span(fileId, start, start + 1)));

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

                            output.Add(new MinusEqualToken(new Span(fileId, start, start + 1)));

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

                            output.Add(new AsteriskEqualToken(new Span(fileId, start, start + 1)));

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

                            output.Add(new ForwardSlashEqualToken(new Span(fileId, start, start + 1)));

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

                            output.Add(new DoubleEqualToken(new Span(fileId, start, start + 1)));

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

                            output.Add(new GreaterThanOrEqualToken(new Span(fileId, start, start + 1)));

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

                            output.Add(new LessThanOrEqualToken(new Span(fileId, start, start + 1)));

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

                            new NotEqualToken(new Span(fileId, start, start + 1));

                            continue;
                        }
                    }

                    output.Add(new ExclamationToken(new Span(fileId, start, start + 1)));

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

    // public static ErrorOr<Token> LexItem(
    //     FileId fileId,
    //     byte[] bytes,
    //     ref int index) {

    //     if (bytes[index].IsAsciiDigit()) {

    //         // Number

    //         var start = index;

    //         while (index < bytes.Length && bytes[index].IsAsciiDigit()) {

    //             index += 1;
    //         }

    //         var str = UTF8.GetString(bytes[start..index]);

    //         Int64 number = 0;

    //         if (Int64.TryParse(str, out number)) {

    //             return new ErrorOr<Token>(new NumberToken(number, new Span(fileId, start, start + 1)));
    //         }
    //         else {

    //             return new ErrorOr<Token>("could not parse int", new Span(fileId, start, index));
    //         }
    //     }
    //     else if (ToChar(bytes[index]) == '"') {

    //         // Quoted string

    //         var start = index;

    //         index += 1;

    //         var escaped = false;

    //         while (index < bytes.Length && (escaped || ToChar(bytes[index]) != '"')) {

    //             if (!escaped && ToChar(bytes[index]) == '\\') {

    //                 escaped = true;
    //             }
    //             else {

    //                 escaped = false;
    //             }

    //             index += 1;
    //         }

    //         if (index == bytes.Length || ToChar(bytes[index]) != '"') {

    //             return new ErrorOr<Token>("Expected quote", new Span(fileId, index, index));
    //         }

    //         // Everything but the quotes

    //         var str = UTF8.GetString(bytes[(start + 1)..(index)]);

    //         var end = index;

    //         index += 1;

    //         return new ErrorOr<Token>(new QuotedStringToken(str, new Span(fileId, start, end)));
    //     }
    //     else {

    //         // Symbol name

    //         var start = index;

    //         index += 1;

    //         var escaped = false;

    //         while (index < bytes.Length 
    //             && (bytes[index].IsAsciiAlphanumeric() || ToChar(bytes[index]) == '_')) {

    //             if (!escaped && ToChar(bytes[index]) == '\\') {

    //                 escaped = true;
    //             }
    //             else {

    //                 escaped = false;
    //             }

    //             index += 1;
    //         }

    //         // Everything but the quotes

    //         var str = UTF8.GetString(bytes[start..index]);

    //         return new ErrorOr<Token>(new NameToken(str, new Span(fileId, start, index)));
    //     }
    // }

    public static (Token, Error?) LexItem(FileId fileId, byte[] bytes, ref int index) {

        Error? error = null;

        if (bytes[index].IsAsciiDigit()) {

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
        else if (ToChar(bytes[index]) == '"') {

            // Quoted String

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
                    "Expected quote", 
                    new Span(fileId, index, index));
            }

            // Everything but the quotes

            var str = UTF8.GetString(bytes[(start + 1)..index]);

            var end = index;

            index += 1;

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
}