using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace TfsUtils.Common.Configuration
{
    public class TypeConverters
    {
        /// <summary>
        /// Defines methods that can convert values from one type to another.
        /// </summary>
        public interface ITypeConverter
        {
            /// <summary>
            /// Order to execute an <see cref="ITypeConverter" /> in.
            /// </summary>
            int Order { get; }

            /// <summary>
            /// Converts the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value is to be converted to.</param>
            /// <returns>Converted value.</returns>
            object Convert(object value, Type type);

            /// <summary>
            /// Indicates whether it can convert the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value needs to be converted to.</param>
            /// <returns>Boolean response.</returns>
            bool CanConvert(object value, Type type);
        }

        /// <summary>
        /// Converts values to Guids.
        /// </summary>
        public class GuidConverter : ITypeConverter
        {
            #region Implementation of ITypeConverter

            /// <summary>
            /// Converts the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value is to be converted to.</param>
            /// <returns>Converted value.</returns>
            public object Convert(object value, Type type)
            {
                object convertedValue = null;

                if (value is string)
                {
                    convertedValue = new Guid(value as string);
                }
                if (value is byte[])
                {
                    convertedValue = new Guid(value as byte[]);
                }

                return convertedValue;
            }

            /// <summary>
            /// Indicates whether it can convert the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value needs to be converted to.</param>
            /// <returns>Boolean response.</returns>
            public bool CanConvert(object value, Type type)
            {
                return type == typeof(Guid);
            }

            /// <summary>
            /// Order to execute an <see cref="ITypeConverter" /> in.
            /// </summary>
            public int Order
            {
                get { return 100; }
            }

            #endregion
        }

        /// <summary>
        /// Converts values to Enums.
        /// </summary>
        public class EnumConverter : ITypeConverter
        {
            #region Implementation of ITypeConverter

            /// <summary>
            /// Converts the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value is to be converted to.</param>
            /// <returns>Converted value.</returns>
            public object Convert(object value, Type type)
            {
                // Handle Nullable types
                var conversionType = Nullable.GetUnderlyingType(type) ?? type;

                var convertedValue = Enum.Parse(conversionType, value.ToString());

                return convertedValue;
            }

            /// <summary>
            /// Indicates whether it can convert the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value needs to be converted to.</param>
            /// <returns>Boolean response.</returns>
            public bool CanConvert(object value, Type type)
            {
                return type.IsEnum && (value != null) && !string.IsNullOrWhiteSpace(value.ToString());
            }

            /// <summary>
            /// Order to execute an <see cref="ITypeConverter" /> in.
            /// </summary>
            public int Order
            {
                get { return 100; }
            }

            #endregion
        }

        /// <summary>
        /// Converts values to TimeSpan.
        /// </summary>
        public class TimeSpanConverter : ITypeConverter
        {
            #region Implementation of ITypeConverter

            /// <summary>
            /// Converts the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value is to be converted to.</param>
            /// <returns>Converted value.</returns>
            public object Convert(object value, Type type)
            {
                object convertedValue = TimeSpan.Parse(value.ToString(), CultureInfo.InvariantCulture);

                return convertedValue;
            }

            /// <summary>
            /// Indicates whether it can convert the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value needs to be converted to.</param>
            /// <returns>Boolean response.</returns>
            public bool CanConvert(object value, Type type)
            {
                return type == typeof(TimeSpan);
            }

            /// <summary>
            /// Order to execute an <see cref="ITypeConverter" /> in.
            /// </summary>
            public int Order
            {
                get { return 200; }
            }

            #endregion
        }

        /// <summary>
        /// Converts values types.
        /// </summary>
        public class ValueTypeConverter : ITypeConverter
        {
            #region Implementation of ITypeConverter

            /// <summary>
            /// Converts the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value is to be converted to.</param>
            /// <returns>Converted value.</returns>
            public object Convert(object value, Type type)
            {
                // Handle Nullable types
                var conversionType = Nullable.GetUnderlyingType(type) ?? type;

                var convertedValue = System.Convert.ChangeType(value, conversionType);

                return convertedValue;
            }

            /// <summary>
            /// Indicates whether it can convert the given value to the requested type.
            /// </summary>
            /// <param name="value">Value to convert.</param>
            /// <param name="type">Type the value needs to be converted to.</param>
            /// <returns>Boolean response.</returns>
            public bool CanConvert(object value, Type type)
            {
                return type.IsValueType && !type.IsEnum && (type != typeof(Guid)) && (type != typeof(TimeSpan));
            }

            /// <summary>
            /// Order to execute an <see cref="ITypeConverter" /> in.
            /// </summary>
            public int Order
            {
                get { return 500; }
            }

            #endregion
        }

        /// <summary>
        /// Converts generic enumerable types.
        /// </summary>
        public class EnumerableConverter : ITypeConverter
        {
            public object Convert(object value, Type type)
            {
                var conversionType = Nullable.GetUnderlyingType(type) ?? type;
                var conversionParamType = conversionType.GetGenericArguments()[0];
                var targetData = value as IEnumerable;

                var convertedValue = typeof(List<>).MakeGenericType(conversionParamType);
                var targetEntityList = Activator.CreateInstance(convertedValue);

                foreach (var data in targetData)
                {
                    var result = System.Convert.ChangeType(data, conversionParamType);
                    convertedValue.GetMethod("Add").Invoke(targetEntityList, new[] {result});
                }

                return targetEntityList;
            }

            public bool CanConvert(object value, Type type)
            {
                return type.IsConstructedGenericType && typeof(IEnumerable).IsAssignableFrom(type);
            }

            public int Order
            {
                get { return 1000; }
            }
        }
    }
}