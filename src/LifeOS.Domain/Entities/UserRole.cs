using LifeOS.Domain.Common;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Kullanıcı ve Rol arasındaki many-to-many ilişkiyi temsil eder.
/// Bir kullanıcının hangi rollere sahip olduğunu tanımlar.
/// </summary>
public sealed class UserRole : BaseEntity
{
    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property: İlişkili kullanıcı
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Rol ID'si
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Navigation property: İlişkili rol
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Rolün kullanıcıya atandığı tarih
    /// </summary>
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UserRole'ü soft delete ile siler
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Silinmiş UserRole'ü geri yükler (soft delete'i kaldırır)
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedDate = null;
    }
}
