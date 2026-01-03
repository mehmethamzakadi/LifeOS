namespace LifeOS.Application.Common.Constants;

/// <summary>
/// Centralized response messages for consistent user-facing messages.
/// Supports future localization by keeping all messages in one place.
/// </summary>
public static class ResponseMessages
{
    #region Generic Messages
    
    public static class Generic
    {
        public const string OperationSuccessful = "İşlem başarıyla tamamlandı.";
        public const string OperationFailed = "İşlem sırasında bir hata oluştu.";
        public const string NotFound = "Kayıt bulunamadı.";
        public const string AlreadyExists = "Bu kayıt zaten mevcut.";
        public const string InvalidOperation = "Geçersiz işlem.";
    }
    
    #endregion

    #region Category Messages
    
    public static class Category
    {
        public const string Created = "Kategori bilgisi başarıyla eklendi.";
        public const string Updated = "Kategori bilgisi başarıyla güncellendi.";
        public const string Deleted = "Kategori bilgisi başarıyla silindi.";
        public const string NotFound = "Kategori bilgisi bulunamadı!";
        public const string AlreadyExists = "Bu kategori adı zaten mevcut!";
    }
    
    #endregion

    #region User Messages
    
    public static class User
    {
        public const string Created = "Kullanıcı bilgisi başarıyla eklendi.";
        public const string Updated = "Kullanıcı bilgisi başarıyla güncellendi.";
        public const string Deleted = "Kullanıcı bilgisi başarıyla silindi.";
        public const string NotFound = "Kullanıcı bulunamadı!";
        public const string EmailAlreadyExists = "Bu e-posta adresi zaten kullanılıyor!";
        public const string UsernameAlreadyExists = "Bu kullanıcı adı zaten kullanılıyor!";
    }
    
    #endregion

    #region Role Messages
    
    public static class Role
    {
        public const string Created = "Rol başarıyla eklendi.";
        public const string Updated = "Rol güncellendi.";
        public const string Deleted = "Rol başarıyla silindi.";
        public const string NotFound = "Rol bulunamadı!";
        public const string AlreadyExists = "Bu rol adı zaten mevcut!";
        
        public static string AlreadyExistsWithName(string roleName) 
            => $"Güncellemek istediğiniz {roleName} rolü sistemde mevcut!";
    }
    
    #endregion

    #region Permission Messages
    
    public static class Permission
    {
        public const string Assigned = "İzinler başarıyla atandı.";
        public const string RoleNotFound = "Rol bulunamadı";
    }
    
    #endregion

    #region Auth Messages
    
    public static class Auth
    {
        public const string RegisterSuccess = "Kayıt işlemi başarılı. Giriş yapabilirsiniz.";
        public const string LoginSuccess = "Giriş başarılı.";
        public const string LogoutSuccess = "Çıkış başarılı.";
        public const string InvalidCredentials = "Geçersiz kullanıcı adı veya şifre.";
        public const string EmailAlreadyRegistered = "Bu e-posta adresi zaten kullanılıyor!";
        public const string TokenRefreshed = "Token yenilendi.";
        public const string InvalidRefreshToken = "Geçersiz refresh token.";
    }
    
    #endregion

    #region Book Messages
    
    public static class Book
    {
        public const string Created = "Kitap bilgisi başarıyla eklendi.";
        public const string Updated = "Kitap bilgisi başarıyla güncellendi.";
        public const string Deleted = "Kitap bilgisi başarıyla silindi.";
        public const string NotFound = "Kitap bilgisi bulunamadı!";
        public const string Retrieved = "Kitap bilgisi başarıyla getirildi.";
    }
    
    #endregion

    #region Game Messages
    
    public static class Game
    {
        public const string Created = "Oyun bilgisi başarıyla eklendi.";
        public const string Updated = "Oyun bilgisi başarıyla güncellendi.";
        public const string Deleted = "Oyun bilgisi başarıyla silindi.";
        public const string NotFound = "Oyun bilgisi bulunamadı!";
    }
    
    #endregion

    #region MovieSeries Messages
    
    public static class MovieSeries
    {
        public const string Created = "Film/Dizi bilgisi başarıyla eklendi.";
        public const string Updated = "Film/Dizi bilgisi başarıyla güncellendi.";
        public const string Deleted = "Film/Dizi bilgisi başarıyla silindi.";
        public const string NotFound = "Film/Dizi bilgisi bulunamadı!";
    }
    
    #endregion

    #region PersonalNote Messages
    
    public static class PersonalNote
    {
        public const string Created = "Kişisel not başarıyla eklendi.";
        public const string Updated = "Kişisel not başarıyla güncellendi.";
        public const string Deleted = "Kişisel not başarıyla silindi.";
        public const string NotFound = "Kişisel not bulunamadı!";
    }
    
    #endregion

    #region WalletTransaction Messages
    
    public static class WalletTransaction
    {
        public const string Created = "Cüzdan işlemi başarıyla eklendi.";
        public const string Updated = "Cüzdan işlemi başarıyla güncellendi.";
        public const string Deleted = "Cüzdan işlemi başarıyla silindi.";
        public const string NotFound = "Cüzdan işlemi bulunamadı!";
    }
    
    #endregion
}
