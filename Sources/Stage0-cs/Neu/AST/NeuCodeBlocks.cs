
namespace Neu;

public partial class NeuCodeBlockItem: NeuNode {

    public NeuCodeBlockItem(
        IEnumerable<Node> children,
        ISourceLocation start,
        ISourceLocation end)
        : base(children, start, end) { }
}

///

public partial class NeuCodeBlockItemList: NeuNode {

    public NeuCodeBlockItemList(
        IEnumerable<Node> children,
        ISourceLocation start,
        ISourceLocation end)
        : base(children, start, end) { }

    ///

    public NeuCodeBlockItemList(
        IEnumerable<NeuCodeBlockItem> codeBlockItems)
        : base(codeBlockItems, new UnknownLocation(), new UnknownLocation()) { }
}

///

public partial class NeuCodeBlock: NeuNode {

    public NeuCodeBlock(
        IEnumerable<Node> children,
        ISourceLocation start,
        ISourceLocation end)
        : base(children, start, end) { }

    ///

    public NeuCodeBlock(
        NeuCodeBlockItemList codeBlockItemList)
        : base(
            new Node[] {
                new NeuPunc('{', new UnknownLocation(), new UnknownLocation(), NeuPuncType.LeftBrace),
                codeBlockItemList,
                new NeuPunc('}', new UnknownLocation(), new UnknownLocation(), NeuPuncType.RightBrace),
            }, 
            new UnknownLocation(), 
            new UnknownLocation()) { }
}
