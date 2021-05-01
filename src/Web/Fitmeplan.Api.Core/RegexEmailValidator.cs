using System.Text.RegularExpressions;

namespace Fitmeplan.Api.Core
{
    public class RegexEmailValidator : IEmailValidator
    {
        private readonly string _regex = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return true;
            }
            var regex = new Regex(_regex);
            return regex.Match(email).Success;
        }
    }
}
