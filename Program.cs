using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;

namespace Unicodify
{
    class Program
    {
        static void Main()
        {
            var source = new StreamReader(Console.OpenStandardInput()).ReadToEnd();
            //var source = "using System;using System.IO;class ß{static void Main(){ä(new FileInfo(\"i.txt\"),new FileInfo(\"o.txt\"));}static void ä(FileInfo î,FileInfo ö){StreamReader à=new StreamReader(î.OpenRead());StreamWriter b=new StreamWriter(ö.OpenWrite());b.Write(à.ReadToEnd().Replace(\"\\n\",\"\").Replace(\"\\r\",\"\"));à.Close();b.Close();}}";
            //var source = @"class HelloWorld { static void Main() { System.Console.WriteLine(""Hello World\u0021""); } }";

            var tree = CSharpSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax)tree.GetRoot();


            var identifieres =
                root.DescendantTokens()
                .Where(t => t.CSharpKind() == SyntaxKind.IdentifierToken);

            root = root.ReplaceTokens(
                identifieres,
                (id, _) =>
                    SyntaxFactory.Identifier(
                        id.LeadingTrivia,
                        TranslateToUnicodeLiteral(id.Text),
                        id.TrailingTrivia));


            var stringLiterals =
                root.DescendantTokens()
                .Where(t => t.CSharpKind() == SyntaxKind.StringLiteralToken);

            root = root.ReplaceTokens(
                stringLiterals,
                (stringLiteral, _) =>
                    SyntaxFactory.Literal(
                        text: "\"" + TranslateToUnicodeLiteral(stringLiteral.ValueText) + "\"",
                        value: stringLiteral.ValueText));


            var translatedSource = root.ToString();
            Console.Write(translatedSource);
        }

        private static string TranslateToUnicodeLiteral(string str)
        {
            return string.Concat(str.Select(c => string.Format("\\u{0:X4}", (int)c)));
        }
    }
}
