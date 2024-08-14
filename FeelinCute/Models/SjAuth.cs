using System;
namespace FeelinCute.Models
{
    public class AuthResponse
{
    public int Code { get; set; }
    public bool Result { get; set; }
    public string Message { get; set; }
    public AuthData Data { get; set; }
}

public class AuthData
{
    public string AccessToken { get; set; }
    public string AccessTokenExpiryDate { get; set; }
    public string RefreshToken { get; set; }
    public string RefreshTokenExpiryDate { get; set; }
    public string CreateDate { get; set; }
}
}
