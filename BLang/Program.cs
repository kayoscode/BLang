using BLang;

public class Program
{
    public static void Main(string[] args)
    {
        string fileName = "test-file.txt";

        Tokenizer.Token token = new();
        Tokenizer tokenizer = new();

        if (File.Exists(fileName))
        {
            tokenizer.SetStream(new StreamReader(fileName));
        }
        else
        {
            throw new FileNotFoundException();
        }

        while (tokenizer.NextToken(token))
        {
            token.PrintToken();
        }
    }
}