using Shared.Core.Enums;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Shared.Core.Helpers
{
    public static class Helpers
    {
        private static string GetUniqueKey(int maxSize, char[] chars)
        {
            //int maxSize for length of string  
            //char[] chars for contains value for generate our randon number  
            byte[] data = new byte[1];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[maxSize];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length - 1)]);
            }
            return result.ToString();
        }
        public static string GenerateNumericNumber(int length)
        {
            return GetUniqueKey(length, "0123456789".ToCharArray());
        }
        public static string GenerateAlphaNumber(int length)
        {
            string alphanumber = GetUniqueKey(length, "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
            return alphanumber;
        }
        public static string GenerateSpecialCharNumber()
        {
            string SpecialCharNumber = GetUniqueKey(10, "!@#$%^&*()".ToCharArray());
            return SpecialCharNumber;
        }
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
        {
            if (condition)
                return source.Where(predicate);
            else
                return source;
        }
        public static string GeneratePass(AppSource appSource)
        {
            return appSource == AppSource.Mobile ? GenerateNumericNumber(6) : GenerateAlphaNumber(12);
        }
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }        
    }
}
