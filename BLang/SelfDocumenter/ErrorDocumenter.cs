using BLang.Error;

namespace BLang.SelfDocumentation
{
    /// <summary>
    /// Class responsible for documenting the errors that can occur.
    /// </summary>
    public static class ErrorDocumenter
    {
        /// <summary>
        /// Produces documentaton for the error feature.
        /// </summary>
        /// <param name="outputStream"></param>
        public static void WriteDocumentation(StreamWriter outputStream)
        {
            foreach (var errorSeries in Enum.GetValues<eParseErrorSeries>())
            {
                outputStream.WriteLine($"Series: {errorSeries.Prefix()}");
                outputStream.WriteLine($"{errorSeries.Description()}");
                outputStream.WriteLine();

                foreach (var error in Enum.GetValues<eParseError>())
                {
                    if (error.Series() == errorSeries)
                    {
                        outputStream.WriteLine($"Error [{error.ErrorCode()}]: {error.Name()} in series {error.Series()}");
                        outputStream.WriteLine($"{error.Description()}");
                        outputStream.WriteLine();
                    }
                }
            }
        }
    }
}
