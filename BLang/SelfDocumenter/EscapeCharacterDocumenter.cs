using BLang.Error;
using BLang.Syntax;

namespace BLang.SelfDocumenter
{
    /// <summary>
    /// Class responsible for documenting the errors that can occur.
    /// </summary>
    public static class EscapeCharacterDocumenter
    {
        /// <summary>
        /// Produces documentaton for the error feature.
        /// </summary>
        /// <param name="outputStream"></param>
        public static void WriteDocumentation(StreamWriter outputStream)
        {
            outputStream.WriteLine("Escape characters:");

            foreach (var escapeChar in Enum.GetValues<eEscapeCharacter>())
            {
                outputStream.WriteLine($"\\{escapeChar.InputChar()}: {escapeChar}");
            }
        }
    }
}
