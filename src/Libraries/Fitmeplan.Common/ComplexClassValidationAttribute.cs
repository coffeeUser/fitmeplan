using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Fitmeplan.Common
{
    public class ComplexClassValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var isValid = true;

            if (value == null)
            {
                return isValid;
            }

            var nestedValidationProperties = value.GetType().GetProperties()
                .Where(p => IsDefined(p, typeof(ValidationAttribute)))
                .OrderBy(p => p.Name);

            foreach (var property in nestedValidationProperties)
            {
                var validators = GetCustomAttributes(property, typeof(ValidationAttribute)) as ValidationAttribute[];

                if (validators == null || validators.Length == 0) continue;

                foreach (var validator in validators)

                {
                    var propertyValue = property.GetValue(value, null);

                    var result = validator.GetValidationResult(propertyValue, new ValidationContext(value, null, null));

                    if (result == ValidationResult.Success) continue;

                    isValid = false;

                    break;
                }

                if (!isValid)
                {
                    break;
                }
            }

            return isValid;
        }
    }
}