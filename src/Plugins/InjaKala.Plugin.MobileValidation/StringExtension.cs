using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace InjaKalaIR.Plugin.MobileValidation
{
    internal static class StringExtension
    {
        public static bool IsEmptyPicture(this string pictureUrl)
        {
            return string.IsNullOrEmpty(pictureUrl) ? false : pictureUrl.EndsWith("default-image.png");
        }

        public static string NormalizerMobileNumber(this string number)
        {
            number = number.Replace("+98", "0").Replace("0098", "");
            if (number.StartsWith("98"))
            {
                number = number.Replace("98", "");
            }

            return number.PersianNumbersToEnglish();
        }

        public static string PersianNumbersToEnglish(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                    .Replace('\u06f0', '0')
                    .Replace('\u06f1', '1')
                    .Replace('\u06f2', '2')
                    .Replace('\u06f3', '3')
                    .Replace('\u06f4', '4')
                    .Replace('\u06f5', '5')
                    .Replace('\u06f6', '6')
                    .Replace('\u06f7', '7')
                    .Replace('\u06f8', '8')
                    .Replace('\u06f9', '9');
        }

        public static string EnglishNumbersToPersian(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                    .Replace('0', '\u06f0')
                    .Replace('1', '\u06f1')
                    .Replace('2', '\u06f2')
                    .Replace('3', '\u06f3')
                    .Replace('4', '\u06f4')
                    .Replace('5', '\u06f5')
                    .Replace('6', '\u06f6')
                    .Replace('7', '\u06f7')
                    .Replace('8', '\u06f8')
                    .Replace('9', '\u06f9');
        }

        public static bool IsValidMobileNumber(this string mobileNumber)
        {
            mobileNumber = mobileNumber.GetEnglishNumber();

            if (string.IsNullOrEmpty(mobileNumber))
                return false;

            mobileNumber = mobileNumber.Trim();

            return Regex.IsMatch(mobileNumber, @"^(0|\+98)?([ ]|,|-|[()]){0,2}9[0|1|2|3|4|5|6|7|8|9]([ ]|,|-|[()]){0,3}(?:[0-9]([ ]|,|-|[()]){0,2}){8}$", RegexOptions.IgnoreCase);
        }

        public static string ConvertNumerals(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return input
                    .Replace('\u06f0', '0')
                    .Replace('\u06f1', '1')
                    .Replace('\u06f2', '2')
                    .Replace('\u06f3', '3')
                    .Replace('\u06f4', '4')
                    .Replace('\u06f5', '5')
                    .Replace('\u06f6', '6')
                    .Replace('\u06f7', '7')
                    .Replace('\u06f8', '8')
                    .Replace('\u06f9', '9');
        }

        public static string GetEnglishNumber(this string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;
            for (var i = 1776; i < 1786; i++)
            {
                data = data.Replace(Convert.ToChar(i), Convert.ToChar(i - 1728));
            }
            return data;
        }

        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        public static bool IsValidMobileNumberOrEmail(this string mobileNumberOrEmail)
        {
            return mobileNumberOrEmail.IsValidMobileNumber() || mobileNumberOrEmail.IsValidEmail();
        }
    }
}