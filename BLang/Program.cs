using BLang;
using BLang.SelfDocumentation;
using BLang.SelfDocumenter;
using BLang.Utils;
using System.Security.Cryptography.X509Certificates;

public class Program
{
    public static void Main(string[] args)
    {
        if (AppSettings.GenerateDocs)
        {
            string docsFile = "documentation.txt";
            var fileStream = new StreamWriter(docsFile);

            ErrorDocumenter.WriteDocumentation(fileStream);
            EscapeCharacterDocumenter.WriteDocumentation(fileStream);
            fileStream.Close();
        }

        string fileName = "test-file.txt";

        ParserContext context = new();
        Tokenizer tokenizer = new(context);

        if (File.Exists(fileName))
        {
            var writer = new StreamReader(fileName);

            try
            {
                tokenizer.SetStream(writer);

                while (tokenizer.NextToken())
                {
                    context.Token.PrintToken();
                }
            }
            finally
            {
                writer.Close();
            }
        }
        else
        {
            throw new FileNotFoundException();
        }
    }
}