using System;
using TfsUtils.Common.Model;

namespace TfsUtils.Common
{
    public class TfsUtilsFactory
    {
        public static ITfsUtil<TArgType> GetUtil<TUtilType, TArgType>(ITfsUtilInitializer initializer) where TUtilType : ITfsUtil
        {
            return Activator.CreateInstance(typeof(TUtilType), initializer) as ITfsUtil<TArgType>;
        }

        public static ITfsUtil<TArgType> GetUtil<TArgType>(Type utilType, ITfsUtilInitializer initializer)
        {
            return Activator.CreateInstance(utilType, initializer) as ITfsUtil<TArgType>;
        }
    }
}