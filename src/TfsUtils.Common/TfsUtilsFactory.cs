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

        public static ITfsUtil<TArgType> GetUtil<TUtilType, TArgType>(ITfsUtilInitializer initializer, params TArgType[] args) where TUtilType : ITfsUtil
        {
            return Activator.CreateInstance(typeof(TUtilType), initializer, args) as ITfsUtil<TArgType>;
        }

        public static ITfsUtil<TArgType> GetUtil<TArgType>(Type utilType, ITfsUtilInitializer initializer)
        {
            return Activator.CreateInstance(utilType, initializer) as ITfsUtil<TArgType>;
        }

        public static ITfsUtilInitializer GetInitializer<TSetting>(Uri tfsServerUrl, TSetting commandSettings) where TSetting: class, new()
        {
            var initializerType = typeof(TfsUtilInitializer<>).MakeGenericType(commandSettings.GetType());
            return Activator.CreateInstance(initializerType, tfsServerUrl, commandSettings) as ITfsUtilInitializer;
        }
    }
}