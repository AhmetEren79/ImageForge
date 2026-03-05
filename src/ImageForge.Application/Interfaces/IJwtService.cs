// IJwtService.cs — JWT token oluşturma ve doğrulama interface'i.
// Task 3'te implement edilecek.

using ImageForge.Domain.Entities;

namespace ImageForge.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    Guid? ValidateToken(string token);
}
