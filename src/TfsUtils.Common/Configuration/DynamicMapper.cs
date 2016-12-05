using System;
using System.Collections.Generic;
using System.Linq;

namespace TfsUtils.Common.Configuration
{
    /// <summary>
    /// Maps source object properties to targetType properties, using property names and <see cref="TypeConverters"/>.
    /// </summary>
    public class DynamicMapper
    {
        private static readonly IEnumerable<TypeConverters.ITypeConverter> TypeConverters;

        static DynamicMapper()
        {
            TypeConverters = new TypeConverters.ITypeConverter[]
            {
                new TypeConverters.GuidConverter(),
                new TypeConverters.EnumConverter(),
                new TypeConverters.TimeSpanConverter(),
                new TypeConverters.ValueTypeConverter(),
                new TypeConverters.EnumerableConverter()
            }.AsEnumerable();
        }

        public static T MapTo<T>(object expando)
        {
            var entity = Activator.CreateInstance<T>();

            return (T) InternalMapProperties(expando, entity);
        }

        public static object MapTo(object expando, Type targetType)
        {
            var entity = Activator.CreateInstance(targetType);

            return InternalMapProperties(expando, entity);
        }

        private static object InternalMapProperties(object expando, object entity)
        {
            var expandoProperties = expando as IDictionary<string, object>;

            if (expandoProperties == null)
            {
                return entity;
            }

            foreach (var expandoEntry in expandoProperties)
            {
                var propertyInfo = entity.GetType().GetProperty(expandoEntry.Key);

                if (propertyInfo == null)
                {
                    continue;
                }

                var value = expandoEntry.Value;

                var flag = false;
                foreach (var typeConverter in TypeConverters.OrderBy(x => x.Order))
                {
                    if (!typeConverter.CanConvert(value, propertyInfo.PropertyType))
                    {
                        continue;
                    }

                    var convertedValue = typeConverter.Convert(value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(entity, convertedValue, null);
                    flag = true;
                    break;
                }

                if (!flag)
                {
                    propertyInfo.SetValue(entity, value, null);
                }
            }
            return entity;
        }
    }
}