using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLang
{
    public class AttributeCacheHelper<TAttr, KEnum> where TAttr : Attribute where KEnum : Enum
    { 
        /// <summary>
        /// Standard construtor.
        /// </summary>
        public AttributeCacheHelper()
        {
        }

        /// <summary>
        /// Assuming we can only have one of these attributes per token, cache it.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private TAttr CacheAttribute(KEnum token)
        {
            var type = token.GetType();
            var memInfo = type.GetMember(token.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(TAttr), false);

            TAttr data = null;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is TAttr attr)
                {
                    data = attr;
                }
            }

            mCachedData[token] = data;
            return data;
        }

        /// <summary>
        /// Returns and caches an attribute. Throws an exception if the attribute is not attached
        /// to the token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public TAttr GetAttribute(KEnum token)
        {
            if (mCachedData.ContainsKey(token))
            {
                return mCachedData[token];
            }

            var data = CacheAttribute(token);

            if (data != null)
            {
                return data;
            }

            throw new ArgumentException($"{token} does not have a {nameof(TAttr)} attribute");
        }

        private Dictionary<KEnum, TAttr> mCachedData = new();
    }
}
