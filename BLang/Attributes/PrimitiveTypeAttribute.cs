using BLang.Syntax;

namespace BLang
{
    public class PrimitiveTypeAttribute : Attribute
    {
        public string Name;
        public int DataSize;

        public PrimitiveTypeAttribute(string name, int dataSize)
        {
            Name = name;
            DataSize = dataSize;
        }
    }

    public static class PrimitiveTypeAttributeData
    {
        // Primitive types start at 0.
        private const int PRIMITIVE_TYPE_OFFSET = 1000;

        public static string Name(this ePrimitiveType token)
        {
            return mCacheHelper.GetAttribute(token).Name;
        }

        public static int Code(this ePrimitiveType token)
        {
            return (int)token + PRIMITIVE_TYPE_OFFSET;
        }

        public static int DataSize(this ePrimitiveType token)
        {
            return mCacheHelper.GetAttribute(token).DataSize;
        }

        private static AttributeCacheHelper<PrimitiveTypeAttribute, ePrimitiveType> mCacheHelper = new();
    }
}
