using System.Reflection;

namespace BlazorSessionScopedContainer.Misc
{
    internal class Helper
    {
        public static IEnumerable<MethodInfo> GetMethodsWithAttribute(Type type, Type attributeType)
        {
            var currentMethods = type.GetMethods();
            foreach (var method in currentMethods)
            {
                if (method.CustomAttributes.Any(p => p.AttributeType.Equals(attributeType)))
                {
                    yield return method;
                }
            }
        }
    }
}
