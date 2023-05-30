using BLang;
using BLang.SelfDocumentation;
using BLang.SelfDocumenter;
using BLang.Utils;

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

        Parser parser = new Parser();

        if (File.Exists(fileName))
        {
            var reader = new StreamReader(fileName);

            try
            {
                parser.ParseFile(reader);
            }
            finally
            {
                reader.Close();
            }
        }
        else
        {
            throw new FileNotFoundException();
        }
    }
}