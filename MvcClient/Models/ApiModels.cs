namespace Uam.AdvancedProgramming.MvcClient.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public T? Result { get; set; }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string SessionToken { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public int ExpiresIn { get; set; }
}

public class VerifyOtpRequestDto
{
    public string Code { get; set; } = string.Empty;
}

public class VerifyOtpApiRequestDto
{
    public string SessionToken { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}

public class VerifyOtpResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public int ExpiresIn { get; set; }
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class StudentDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }
}

public class StudentUpsertDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }
}

public class RoleDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }
}

public class RoleUpsertDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UserDto
{
    public int Id { get; set; }

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}

public class UserUpsertDto
{
    public int RoleId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }
}

public class LaboratoryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Building { get; set; } = string.Empty;

    public int Floor { get; set; }

    public int Capacity { get; set; }

    public bool IsActive { get; set; }
}

public class LaboratoryUpsertDto
{
    public string Name { get; set; } = string.Empty;

    public string Building { get; set; } = string.Empty;

    public int Floor { get; set; }

    public int Capacity { get; set; }
}

public class EquipmentDto
{
    public int Id { get; set; }

    public int LaboratoryId { get; set; }

    public string LaboratoryName { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateOnly? PurchaseDate { get; set; }

    public bool IsActive { get; set; }
}

public class EquipmentUpsertDto
{
    public int LaboratoryId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateOnly? PurchaseDate { get; set; }
}