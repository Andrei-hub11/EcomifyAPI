using System.Reflection;

internal static class DependencyValidationExtensions
{
    /// <summary>
    /// Gets the direct dependencies of a class, considering constructors and parameters of methods.
    /// </summary>
    /// <param name="type">Type of the class that will be analyzed.</param>
    /// <returns>List of types that represent the direct dependencies of the class.</returns>
    public static List<Type> GetDirectDependencies(this Type type)
    {
        var directDependencies = new HashSet<Type>();

        // 1. Get the dependencies of the Constructor
        var constructors = type.GetConstructors();
        foreach (var constructor in constructors)
        {
            foreach (var param in constructor.GetParameters())
            {
                directDependencies.Add(param.ParameterType);
            }
        }

        // 2. Get the dependencies of the Public Methods (ex.: parameters of Actions or internal methods)
        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
        foreach (var method in methods)
        {
            foreach (var param in method.GetParameters())
            {
                directDependencies.Add(param.ParameterType);
            }
        }

        return directDependencies.ToList();
    }

    /// <summary>
    /// Verifies if the direct dependencies of a class belong only to the allowed namespaces.
    /// </summary>
    /// <param name="type">Type of the class that will be analyzed.</param>
    /// <param name="allowedNamespaces">Allowed namespaces for dependencies.</param>
    /// <exception cref="Exception">Throws an exception if it finds an invalid dependency.</exception>
    public static void AssertHasValidDependencies(this Type type, List<string> allowedNamespaces)
    {
        var directDependencies = type.GetDirectDependencies();

        //  Validate if all direct dependencies are allowed
        foreach (var dependency in directDependencies)
        {
            var dependencyNamespace = dependency.Namespace;

            if (!allowedNamespaces.Any(ns => dependencyNamespace?.StartsWith(ns) ?? false))
            {
                throw new Exception($"❌ [ERRO] The class {type.Name} has an invalid dependency: {dependency.FullName}");
            }
        }
    }

    /*  public static bool ContainsOnlyProperties(this Type dtoType)
     {
         if (!HasPositionalRecordSignature(dtoType))
             return false;

         var properties = dtoType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
         if (!properties.Any())
             return false;

         if (HasCustomPublicMethods(dtoType))
             return false;

         return true;
     }

     private static bool HasPositionalRecordSignature(Type type)
     {
         var hasCloneMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                 .Any(m => m.Name.Contains("<Clone>$"));

         var hasDeconstructMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                       .Any(m => m.Name == "Deconstruct");

         var hasPrintMembersMethod = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                       .Any(m => m.Name == "PrintMembers");

         // Verify if it implements IEquatable<T>
         var implementsEquatable = type.GetInterfaces()
                                      .Any(i => i.IsGenericType &&
                                               i.GetGenericTypeDefinition() == typeof(IEquatable<>));

         // Verify if it has backing fields with characteristic names
         var hasBackingFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                                   .Any(f => f.Name.Contains("k__BackingField"));

         return hasCloneMethod && hasDeconstructMethod && hasPrintMembersMethod &&
                implementsEquatable && hasBackingFields;
     }

     private static bool HasCustomPublicMethods(Type type)
     {
         var recordGeneratedMethodNames = new HashSet<string> {
         "ToString",
         "Equals",
         "GetHashCode",
         "Deconstruct",
         "GetType",
         "<Clone>$",
         "op_Equality",
         "op_Inequality"
     };

         // Get all public non-static methods
         var publicMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                 .Where(m => !m.IsSpecialName); // Exclude getters/setters of properties

         // Verify if there is any method that is not in the list of generated methods
         // and is not a getter/setter (IsSpecialName)
         foreach (var method in publicMethods)
         {
             if (!recordGeneratedMethodNames.Contains(method.Name) &&
                 !method.Name.StartsWith("get_") &&
                 !method.Name.StartsWith("set_"))
             {
                 return true; // Found a custom public method
             }
         }

         return false;
     } */
}