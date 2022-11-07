
namespace Neu;

public enum ErrorType {

    IO,
    String,
    Parser,
    TypeCheck,
    Validation
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

public static partial class ErrorFunctions {

    public static Span GetSpan(
        this Error e) {

        switch (e) {

            case ParserError pe: 
                return pe.Span;

            case ParserErrorWithHint peh:
                return peh.Span;
           
            case TypeCheckError te:
                return te.Span;
           
            case ValidationError ve: 
                return ve.Span;
           
            case TypecheckErrorWithHint teh: 
                return teh.Span;
            
            default:
                throw new Exception();
        }
    }
}

public partial class StringError: Error {

    public StringError(
        String content)
        : base(ErrorType.String, content) { }
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

public partial class ParserErrorWithHint: Error {

    public Span Span { get; init; }

    public String HintString { get; init; }

    public Span HintSpan { get; init; }

    ///

    public ParserErrorWithHint(
        String content, 
        Span span,
        String hintString,
        Span hintSpan)
        : base(ErrorType.Parser, content) {

        this.Span = span;
        this.HintString = hintString;
        this.HintSpan = hintSpan;
    }
}

public partial class TypeCheckError: Error {

    public Span Span { get; init; }

    ///

    public TypeCheckError(
        String content, 
        Span span)
        : base(ErrorType.TypeCheck, content) {

        this.Span = span;
    }
}

public partial class TypecheckErrorWithHint: Error {

    public Span Span { get; init; }

    public String HintString { get; init; }

    public Span HintSpan { get; init; }

    ///

    public TypecheckErrorWithHint(
        String content, 
        Span span,
        String hintString,
        Span hintSpan)
        : base(ErrorType.TypeCheck, content) {

        this.Span = span;
        this.HintString = hintString;
        this.HintSpan = hintSpan;
    }
}

public partial class ValidationError: Error {

    public Span Span { get; init; }

    ///

    public ValidationError(
        String content, 
        Span span)
        : base(ErrorType.Validation, content) {

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
        Error e) {

        this.Error = e;
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
