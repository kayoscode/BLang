using BLang.Syntax;

namespace BLang
{
    public class EscapeCharacterAttribute : Attribute
    {
        public char InputChar { get; }
        public char ResultChar { get; }

        public EscapeCharacterAttribute(char inputCharacter, char outputCharacter)
        {
            InputChar = inputCharacter;
            ResultChar = outputCharacter;
        }
    }

    public static class EscapeCharacterAttributeUtils
    {
        public static char GetInputChar(this eEscapeCharacter token)
        {
            return mCachedAttributes.GetAttribute(token).InputChar;
        }

        public static char GetResultChar(this eEscapeCharacter token)
        {
            return mCachedAttributes.GetAttribute(token).ResultChar;
        }

        private static readonly AttributeCacheHelper<EscapeCharacterAttribute, eEscapeCharacter> mCachedAttributes = new();
    }
}
