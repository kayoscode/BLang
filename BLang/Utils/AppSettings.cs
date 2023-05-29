namespace BLang.Utils
{
    public static class AppSettings
    {
        /// <summary>
        /// 1 Kb for the buffer size when loading the files.
        /// </summary>
        public const int TokenizerBufferSize = 4096;

        /// <summary>
        /// Whether or not the software should generate documentation for the system.
        /// TODO: set from run args
        /// </summary>
        public static bool GenerateDocs = true;
    }
}
