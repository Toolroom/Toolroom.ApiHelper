using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Toolroom.ApiHelper
{
    public static class StringExtensions
    {
        #region Email
        /// <summary>
        /// Determines if the string is a valid email address format
        /// </summary>
        /// <param name="email">The email address to check</param>
        public static bool IsValidEmailAddressFormat(this string email)
        {
            // regex taken from http://www.regular-expressions.info/email.html
            // can contain invalid email address but raw syntax is checked
            var r = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            return r.IsMatch(email);
        }
        #endregion

        public static string ToRelationshipName(this string propertyName)
        {
            StringBuilder sb = new StringBuilder();
            if (propertyName.EndsWith("Id", false, CultureInfo.InvariantCulture))
                propertyName = propertyName.Substring(0, propertyName.Length - 2);
            foreach (var character in propertyName)
            {
                if (sb.Length > 0 && char.IsUpper(character))
                    sb.Append("-");
                sb.Append(char.ToLower(character));
            }
            return sb.ToString();
        }
    }
}