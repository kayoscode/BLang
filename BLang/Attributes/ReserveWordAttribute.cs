namespace BLang
{
    /// <summary>
    /// Attribute storing data for reserve words.
    /// </summary>
    public class ReserveWordAttribute : Attribute
    {
        public string ReserveText;

        public ReserveWordAttribute(string reserveText)
        {
            ReserveText = reserveText;
        }
    }

    public static class ReserveTokenAttributeData
    {
        // Reserve words are the first codes in the set.
        private const int RESERVE_CODE_OFFSET = 0;

        public static string ReserveWord(this eReserveWord token)
        {
            return mCacheHelper.GetAttribute(token).ReserveText;
        }

        public static int Code(this eReserveWord token)
        {
            return (int)token + RESERVE_CODE_OFFSET;
        }

        private static AttributeCacheHelper<ReserveWordAttribute, eReserveWord> mCacheHelper = new();
    }
}
