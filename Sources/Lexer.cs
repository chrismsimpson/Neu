
namespace Neu;

public static partial class LexerFunctions {

    public static void Lex(
        FileId fileId, 
        byte[] bytes) {

        var output = new List<Token>();

        var index = 0;

        while (index < bytes.Length) {

            var b = bytes[index];

            var c = ToChar(b);

            switch (c) {

                case ';': {

                    var start = index;

                    index += 1;

                    // output.Add(new )


                    break;
                }

                ///

                default:

                    break;
            }
        }

    }
}