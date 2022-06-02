
namespace Neu;

public enum ErrorType {

    IO,
    Parser
}

public partial class Error {

    public ErrorType ErrorType { get; init; }

    public String? Content { get; init; }

    ///

    public Error(
        ErrorType errorType,
        String? content = null) {

        this.ErrorType = errorType;
        this.Content = content;
    }
}

public partial class ParserError: Error {

    public Span Span { get; init; }

    ///

    public ParserError(
        String content,
        Span span)
        : base(ErrorType.Parser, content) {

        this.Span = span;
    }
}

public partial class ErrorOr<Result> {

    [MaybeNull]
    public Result? Value { get; init; }

    [MaybeNull]
    public Error? Error { get; init; }

    ///

    public ErrorOr(
        Result? value) {

        this.Value = value;
    }

    public ErrorOr(
        ErrorType errorType,
        String? content = null) {

        this.Error = new Error(errorType, content);
    }

    public ErrorOr(
        String content,
        Span span) {

        this.Error = new ParserError(content, span);
    }
}

public class ErrorOrVoid {

    [MaybeNull]
    public Error? Error { get; init; }

    ///

    public ErrorOrVoid() {

        this.Error = null;
    }

    public ErrorOrVoid(Error e) {

        this.Error = e;
    }

    public ErrorOrVoid(
        ErrorType errorType,
        String? content = null) {

        this.Error = new Error(errorType, content);
    }
}
