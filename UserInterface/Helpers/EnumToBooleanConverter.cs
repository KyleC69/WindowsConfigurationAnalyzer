// Created:  2025/10/29
// Solution:
// Project:
// File:
// 
// All Rights Reserved 2025
// Kyle L Crowder



using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;



namespace KC.WindowsConfigurationAnalyzer.UserInterface.Helpers;



public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            if (!Enum.IsDefined(typeof(ElementTheme), value))
            {
                throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
            }

            var enumValue = Enum.Parse(typeof(ElementTheme), enumString);

            return enumValue.Equals(value);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }





    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return parameter is string enumString
            ? Enum.Parse(typeof(ElementTheme), enumString)
            : throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }
}