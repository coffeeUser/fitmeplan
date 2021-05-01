using System;
using System.ComponentModel.DataAnnotations;

namespace Fitmeplan.Common
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class PhoneNumberAttribute : DataTypeAttribute
    {
        public PhoneNumberAttribute()
          : base(DataType.PhoneNumber)
        {
            //this.DefaultErrorMessage = SR.PhoneAttribute_Invalid;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;
            string str1 = value as string;
            if (string.IsNullOrEmpty(str1))
                return true;
            string str2 = PhoneNumberAttribute.RemoveExtension(str1.Replace("+", string.Empty).TrimEnd());
            bool flag = false;
            foreach (char c in str2)
            {
                if (char.IsDigit(c))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
                return false;
            foreach (char c in str2)
            {
                if (!char.IsDigit(c) && !char.IsWhiteSpace(c) && "-()".IndexOf(c) == -1)
                    return false;
            }
            return true;
        }

        private static string RemoveExtension(string potentialPhoneNumber)
        {
            int length1 = potentialPhoneNumber.LastIndexOf("ext.", StringComparison.OrdinalIgnoreCase);
            if (length1 >= 0 && PhoneNumberAttribute.MatchesExtension(potentialPhoneNumber.Substring(length1 + "ext.".Length)))
                return potentialPhoneNumber.Substring(0, length1);
            int length2 = potentialPhoneNumber.LastIndexOf("ext", StringComparison.OrdinalIgnoreCase);
            if (length2 >= 0 && PhoneNumberAttribute.MatchesExtension(potentialPhoneNumber.Substring(length2 + "ext".Length)))
                return potentialPhoneNumber.Substring(0, length2);
            int length3 = potentialPhoneNumber.LastIndexOf("x", StringComparison.OrdinalIgnoreCase);
            if (length3 >= 0 && PhoneNumberAttribute.MatchesExtension(potentialPhoneNumber.Substring(length3 + "x".Length)))
                return potentialPhoneNumber.Substring(0, length3);
            return potentialPhoneNumber;
        }

        private static bool MatchesExtension(string potentialExtension)
        {
            potentialExtension = potentialExtension.TrimStart();
            if (potentialExtension.Length == 0)
                return false;
            foreach (char c in potentialExtension)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }
    }
}
