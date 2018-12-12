using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace EPiServer.Commerce.OrderStore.Model
{
    public class EnsureMinimumElementsAttribute : ValidationAttribute
    {
        readonly int _minElements;

        public EnsureMinimumElementsAttribute(int minElements)
        {
            _minElements = minElements;
        }

        public override bool IsValid(object value)
        {
            if (value is IList list)
            {
                return list.Count >= _minElements;
            }
            return false;
        }
    }
}
