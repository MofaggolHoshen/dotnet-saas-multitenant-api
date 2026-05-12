using AutoMapper;

namespace Application.Common.Mappings;

/// <summary>
/// Interface for types that can define their own AutoMapper mapping configuration.
/// Implementing this interface allows DTOs to specify how they map from domain entities
/// while keeping the mapping logic close to the DTO definition.
/// </summary>
/// <typeparam name="T">The source type to map from.</typeparam>
public interface IMapFrom<T>
{
    /// <summary>
    /// Configures the mapping from the source type to the implementing type.
    /// Default implementation creates a standard mapping.
    /// </summary>
    /// <param name="profile">The AutoMapper profile to configure.</param>
    void Mapping(Profile profile)
    {
        profile.CreateMap(typeof(T), GetType());
    }
}
