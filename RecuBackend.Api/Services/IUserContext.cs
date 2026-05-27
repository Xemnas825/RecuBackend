namespace RecuBackend.Api.Services;

/// <summary>
/// Identificador del usuario actual. Temporal hasta la rama JWT:
/// enviar cabecera <c>X-User-Id</c> con un GUID válido.
/// </summary>
public interface IUserContext
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}
