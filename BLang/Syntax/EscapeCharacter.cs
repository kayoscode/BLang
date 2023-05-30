using System.Diagnostics;

namespace BLang.Syntax
{
    /// <summary>
    /// The characters in the system that can be part of an escape sequence.
    /// Limited to a single character for now (after the backslash)
    /// </summary>
    public enum eEscapeCharacter
    {
        [EscapeCharacter('\'', '\'')]
        SingleQuote,
        [EscapeCharacter('"', '"')]
        DoubleQuote,
        [EscapeCharacter('\\', '\\')]
        Backslash,
        [EscapeCharacter('n', '\n')]
        NewLine,
        [EscapeCharacter('r', '\r')]
        CarriageReturn,
        [EscapeCharacter('t', '\t')]
        Tab,
        [EscapeCharacter('b', '\b')]
        Backspace,
        [EscapeCharacter('f', '\f')]
        FormFeed,
        [EscapeCharacter('a', '\a')]
        Alarm,
        [EscapeCharacter('v', '\v')]
        VerticalTab,
        [EscapeCharacter('0', '\0')]
        Zero,
    }

    public static class EscapeCharacterUtils
    {
        static EscapeCharacterUtils()
        {
            // Fill list of escape character replacements.
            foreach (var escapeCharacter in Enum.GetValues<eEscapeCharacter>())
            {
                var inputChar = escapeCharacter.GetInputChar();
                var resultChar = escapeCharacter.GetResultChar();

                if (mEscapeCharacterReplacements.ContainsKey(inputChar))
                {
                    Trace.Assert(false, $"Escape character defined more than once: {inputChar}");
                }

                mEscapeCharacterReplacements[inputChar] = resultChar;
            }
        }

        /// <summary>
        /// If the escape sequence exists, return it, otherwise return false.
        /// </summary>
        /// <param name="character"></param>
        /// <param name="resultCharacter"></param>
        /// <returns></returns>
        public static bool TryGetEscapeCharacter(char character, out char resultCharacter)
        {
            if (mEscapeCharacterReplacements.ContainsKey(character))
            {
                resultCharacter = mEscapeCharacterReplacements[character];
                return true;
            }

            resultCharacter = '\0';
            return false;
        }


        private static Dictionary<char, char> mEscapeCharacterReplacements = new();
    }
}
