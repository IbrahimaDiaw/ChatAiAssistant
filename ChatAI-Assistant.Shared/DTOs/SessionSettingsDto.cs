using System.ComponentModel.DataAnnotations;

namespace ChatAI_Assistant.Shared.DTOs;

public class SessionSettingsDto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Key cannot exceed 100 characters")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, ErrorMessage = "Value cannot exceed 2000 characters")]
    public string Value { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    // Audit fields from BaseEntity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Type-safe value parsing
    public T GetValue<T>() where T : class
    {
        try
        {
            if (typeof(T) == typeof(string))
                return Value as T ?? default(T)!;

            return System.Text.Json.JsonSerializer.Deserialize<T>(Value) ?? default(T)!;
        }
        catch
        {
            return default(T)!;
        }
    }

    public void SetValue<T>(T value)
    {
        if (value is string stringValue)
        {
            Value = stringValue;
        }
        else
        {
            Value = System.Text.Json.JsonSerializer.Serialize(value);
        }
        UpdatedAt = DateTime.UtcNow;
    }
}