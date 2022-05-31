
namespace Neu;

public static partial class Program {

    public static void Main(
        String[] args) {

        var parser = new Compiler();

        ///

        foreach (var arg in args) {

            var compiledOrError = parser.Compile(arg);

            switch (compiledOrError) {

                case var _ when compiledOrError.Error is ParserError pe:

                    DisplayError(parser, pe.Content, pe.Span);

                    break;

                case var _ when compiledOrError.Error is Error e:

                    WriteLine($"Error: {compiledOrError.Error}");

                    break;

                default:

                    WriteLine("Success!");

                    break;
            }           
        }
    }

    public static void DisplayError(Compiler parser, String? msg, Span span) {
        
        if (msg is String m) {

            WriteLine($"Error: {m}");
        }
        else {
            
            WriteLine("Error");
        }
    
        WriteLine("-----");

        var fileContents = parser.GetFileContents(span.FileId);

        var index = 0;

        while (index <= fileContents.Length) {

            var c = ' ';

            if (index < fileContents.Length) {

                c = ToChar(fileContents[index]);
            }

            if ((index >= span.Start && index < span.End) || 
                (span.Start == span.End && index == span.Start)) {

                // In the error span

                if (c == ' ') {

                    Write(c);
                } 
                else {

                    Write(c);
                }
            }
            else {

                Write(c);
            }

            index += 1;
        }

        WriteLine("");
        
        WriteLine("-----");
    }
}