using BLang;
using BLang.Error;

public class Program
{
    public static void Main(string[] args)
    {
        string fileName = "test-file.txt";

        ParserContext context = new();
        Tokenizer tokenizer = new();

        if (File.Exists(fileName))
        {
            tokenizer.SetStream(new StreamReader(fileName));
        }
        else
        {
            throw new FileNotFoundException();
        }

        while (tokenizer.NextToken(context))
        {
            context.Token.PrintToken();
        }
    }
}