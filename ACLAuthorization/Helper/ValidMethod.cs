using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ACLAuthorization.Helper
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidMethod : ValidationAttribute
    {
        public ValidMethod(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
        public override bool IsValid(object value)
        {
            string[] methodArray = new string[] { "get", "post", "put", "delete" };
            var method = (String)value;
            if (value == null) return false;

            return methodArray.Contains(method);
        }
    }
}
