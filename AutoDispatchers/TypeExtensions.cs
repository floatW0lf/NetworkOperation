using System;
using System.Linq;

namespace TemplateDispatcher
{
    public static class TypeExtensions
    {
        public static string ToCSharpFormat(this Type type)
        {
            return PrettyTypeName(type);
        }
        
        static string PrettyTypeName(Type t)
        {
            if (t.IsGenericType)
            {
                return $"{t.Namespace}.{t.Name.Substring(0, t.Name.LastIndexOf("`", StringComparison.InvariantCulture))}<{string.Join(", ", t.GetGenericArguments().Select(PrettyTypeName))}>";
            }
            return t.FullName;
        }
    }
}