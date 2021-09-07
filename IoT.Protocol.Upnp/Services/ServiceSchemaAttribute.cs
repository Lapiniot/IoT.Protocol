using System.Reflection;

namespace IoT.Protocol.Upnp.Services;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ServiceSchemaAttribute : Attribute
{
    public ServiceSchemaAttribute(string schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        if(schema.Length == 0) throw new ArgumentException("Cannot be empty string.", nameof(schema));

        Schema = schema;
    }

    public string Schema { get; }

    public static string GetSchema(Type serviceType)
    {
        return serviceType.GetCustomAttribute<ServiceSchemaAttribute>()?.Schema ??
               throw new ArgumentException(
                   $"Valid service implementation must be marked with {nameof(ServiceSchemaAttribute)} to denote valid UPnP service schema it implements.");
    }
}