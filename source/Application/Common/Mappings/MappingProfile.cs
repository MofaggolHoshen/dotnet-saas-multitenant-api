using System.Reflection;
using AutoMapper;

namespace Application.Common.Mappings;

/// <summary>
/// AutoMapper profile that automatically discovers and applies mapping configurations
/// from types implementing IMapFrom&lt;T&gt;.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);

        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == mapFromType))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);

            var methodInfo = type.GetMethod(nameof(IMapFrom<object>.Mapping))
                ?? type.GetInterface(mapFromType.Name)?.GetMethod(nameof(IMapFrom<object>.Mapping));

            methodInfo?.Invoke(instance, new object[] { this });
        }
    }
}
