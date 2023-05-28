using BLang.Syntax;

namespace BLang
{
    /// <summary>
    /// Holds a list of all reserve words in the system.
    /// </summary>
    public class ReserveTable
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        public ReserveTable()
        {
            var allTokens = new List<eReserveWord>(Enum.GetValues<eReserveWord>());

            foreach (var token in allTokens)
            {
                mReserveWords.Add(token.ReserveWord(), token.Code());
            }
        }

        /// <summary>
        /// Gets the code for a reserve word, -1 if the code doesn't exist.
        /// </summary>
        /// <param name="reserveWord"></param>
        /// <returns></returns>
        public int GetReserveCode(string reserveWord)
        {
            if (mReserveWords.ContainsKey(reserveWord))
            {
                return mReserveWords[reserveWord];
            }

            return -1;
        }

        public IReadOnlyDictionary<string, int> ReserveWords => mReserveWords;
        private Dictionary<string, int> mReserveWords = new();
    }

    /// <summary>
    /// Holds a list of all reserve words in the system.
    /// </summary>
    public class TypeTable
    {
        /// <summary>
        /// Standard constructor.
        /// </summary>
        public TypeTable()
        {
            var allTokens = new List<ePrimitiveType>(Enum.GetValues<ePrimitiveType>());

            foreach (var token in allTokens)
            {
                mTypeCodes.Add(token.Name(), token.Code());
                mTypeSizes.Add(token.Name(), token.DataSize());
            }
        }

        /// <summary>
        /// Gets the code for a reserve word, -1 if the code doesn't exist.
        /// </summary>
        /// <param name="reserveWord"></param>
        /// <returns></returns>
        public int GetTypeCode(string typeName)
        {
            if (mTypeCodes.ContainsKey(typeName))
            {
                return mTypeCodes[typeName];
            }

            return -1;
        }

        public IReadOnlyDictionary<string, int> TypeCodes => mTypeCodes;
        private Dictionary<string, int> mTypeCodes = new();

        public IReadOnlyDictionary<string, int> TypeSizes => mTypeSizes;
        private Dictionary<string, int> mTypeSizes = new();
    }
}
