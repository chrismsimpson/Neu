
namespace Neu;

public static partial class NeuInterpreterFunctions {

    public static NeuOperation Execute(
        this NeuInterpreter interpreter,
        NeuNode node,
        params object[] arguments) {
            
        interpreter.Enter(node);

        ///
        
        

        ///

            // TODO: Add hoists

        ///

        var enterPos = interpreter.Stack.Count();

        ///

        var lastValue = NeuOperation.Void;

        ///

        var done = false;

        ///

        for (var i = 0; i < node.Children.Count() && !done; i++) {

            var child = node.Children.ElementAt(i);

            ///

            if (child is NeuPunc) {

                continue;
            }

            ///

            var childResult = interpreter.Execute(child);

            ///

            switch (childResult) {

                case NeuReturnResult returnResult:

                    lastValue = returnResult;

                    done = true;

                    break;

                ///

                case NeuValue value:

                    lastValue = value;

                    break;

                ///

                default:

                    break;
            }
        }

        ///

        interpreter.Unwind(enterPos, node);

        ///

        interpreter.Exit(node);

        ///

        return lastValue;
    }


    public static NeuOperation Execute(
        this NeuInterpreter interpreter,
        Node node) {

        switch (node) {

            /// AST Nodes

            case NeuDeclaration decl:
            
                return interpreter.Execute(decl);

            ///
        
            case NeuExpression expr:

                return interpreter.Execute(expr);

            ///

            case NeuStatement stmt:

                return interpreter.Execute(stmt);

            ///

            case NeuNode neuNode:

                return interpreter.Execute(neuNode);

            /// Tokens

            case NeuLiteral literal:

                return interpreter.Execute(literal);

            ///

            case NeuIdentifier id:

                return interpreter.Execute(id);

            ///

            case NeuKeyword k when k.KeywordType == NeuKeywordType.True:

                return new NeuBool(true);

            ///

            case NeuKeyword k when k.KeywordType == NeuKeywordType.False:

                return new NeuBool(false);

            ///

            case var n:
                
                WriteLine($"NeuInterpreter no op: {n.Dump()}");

                return NeuValue.Void;
        }
    }
}