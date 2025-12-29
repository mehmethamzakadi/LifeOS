import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { User, Mail, Phone, Lock, Save, X } from 'lucide-react';
import toast from 'react-hot-toast';
import { getCurrentUserProfile, updateProfile, changePassword } from '../../features/profile/api';
import { UpdateProfileFormValues, ChangePasswordFormValues } from '../../features/profile/types';
import { Button } from '../../components/ui/button';
import { Input } from '../../components/ui/input';
import { Label } from '../../components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/card';
import { ImageUploadField } from '../../components/forms/image-upload-field';
import { handleApiError, showApiResponseError } from '../../lib/api-error';
import { resolveApiAssetUrl } from '../../lib/utils';
import { useAuthStore } from '../../stores/auth-store';

const profileSchema = z.object({
  userName: z.string().min(3, 'Kullanıcı adı en az 3 karakter olmalıdır'),
  email: z.string().email('Geçerli bir email adresi giriniz'),
  phoneNumber: z.string().optional().or(z.literal('')),
  profilePictureUrl: z.string().optional().or(z.literal(''))
});

const passwordSchema = z.object({
  currentPassword: z.string().min(1, 'Mevcut şifre boş olamaz'),
  newPassword: z.string().min(8, 'Yeni şifre en az 8 karakter olmalıdır'),
  confirmPassword: z.string().min(1, 'Şifre onayı boş olamaz')
}).refine((data) => data.newPassword === data.confirmPassword, {
  message: 'Yeni şifre ve şifre onayı eşleşmiyor',
  path: ['confirmPassword']
}).refine((data) => data.newPassword !== data.currentPassword, {
  message: 'Yeni şifre mevcut şifre ile aynı olamaz',
  path: ['newPassword']
});

type ProfileFormSchema = z.infer<typeof profileSchema>;
type PasswordFormSchema = z.infer<typeof passwordSchema>;

export function ProfilePage() {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState('profile');
  const currentUserId = useAuthStore((state) => state.user?.userId);

  // Queries
  // ✅ Query key'e kullanıcı ID'si ekle - kullanıcı değiştiğinde cache otomatik ayrılır
  const { data: profile, isLoading } = useQuery({
    queryKey: ['profile', 'current', currentUserId],
    queryFn: getCurrentUserProfile,
    enabled: !!currentUserId // Kullanıcı ID yoksa query çalışmasın
  });

  // Forms
  const profileForm = useForm<ProfileFormSchema>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      userName: '',
      email: '',
      phoneNumber: '',
      profilePictureUrl: ''
    }
  });

  const passwordForm = useForm<PasswordFormSchema>({
    resolver: zodResolver(passwordSchema),
    defaultValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    }
  });

  // Update form when profile data loads
  useEffect(() => {
    if (profile) {
      profileForm.reset({
        userName: profile.userName,
        email: profile.email,
        phoneNumber: profile.phoneNumber || '',
        profilePictureUrl: profile.profilePictureUrl || ''
      });
    }
  }, [profile, profileForm]);

  // Mutations
  const updateProfileMutation = useMutation({
    mutationFn: (data: UpdateProfileFormValues) => updateProfile(data),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Profil güncellenemedi');
        return;
      }
      toast.success(result.message || 'Profil başarıyla güncellendi');
      // ✅ Kullanıcı ID'si ile invalidate et
      queryClient.invalidateQueries({ queryKey: ['profile', 'current', currentUserId] });
    },
    onError: (error) => handleApiError(error, 'Profil güncellenirken hata oluştu')
  });

  const changePasswordMutation = useMutation({
    mutationFn: (data: ChangePasswordFormValues) => changePassword(data),
    onSuccess: (result) => {
      if (!result.success) {
        showApiResponseError(result, 'Şifre değiştirilemedi');
        return;
      }
      toast.success(result.message || 'Şifre başarıyla değiştirildi');
      passwordForm.reset();
    },
    onError: (error) => handleApiError(error, 'Şifre değiştirilirken hata oluştu')
  });

  // Handlers
  const onProfileSubmit = (values: ProfileFormSchema) => {
    updateProfileMutation.mutate({
      userName: values.userName,
      email: values.email,
      phoneNumber: values.phoneNumber || undefined,
      profilePictureUrl: values.profilePictureUrl || undefined
    });
  };

  const onPasswordSubmit = (values: PasswordFormSchema) => {
    changePasswordMutation.mutate({
      currentPassword: values.currentPassword,
      newPassword: values.newPassword,
      confirmPassword: values.confirmPassword
    });
  };

  const profilePictureUrl = profileForm.watch('profilePictureUrl');
  const profileImageUrl = resolveApiAssetUrl(profilePictureUrl || '');

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-muted-foreground">Yükleniyor...</div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold">Profil Ayarları</h1>
        <p className="text-muted-foreground mt-2">Hesap bilgilerinizi ve şifrenizi yönetin</p>
      </div>

      {/* Profile Info Card */}
      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <div className="relative">
              <div className="h-24 w-24 rounded-full border-4 border-background bg-muted flex items-center justify-center overflow-hidden shadow-lg">
                {profileImageUrl ? (
                  <img
                    src={profileImageUrl}
                    alt="Profil fotoğrafı"
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <User className="h-12 w-12 text-muted-foreground" />
                )}
              </div>
              {profile?.emailConfirmed && (
                <div className="absolute bottom-0 right-0 h-6 w-6 rounded-full bg-green-500 border-2 border-background flex items-center justify-center">
                  <Mail className="h-3 w-3 text-white" />
                </div>
              )}
            </div>
            <div>
              <CardTitle className="text-2xl">{profile?.userName}</CardTitle>
              <CardDescription className="flex items-center gap-2 mt-1">
                <Mail className="h-4 w-4" />
                {profile?.email}
              </CardDescription>
              {profile?.phoneNumber && (
                <CardDescription className="flex items-center gap-2 mt-1">
                  <Phone className="h-4 w-4" />
                  {profile.phoneNumber}
                </CardDescription>
              )}
            </div>
          </div>
        </CardHeader>
      </Card>

      {/* Tabs */}
      <div className="space-y-6">
        <div className="flex gap-2 border-b">
          <button
            type="button"
            onClick={() => setActiveTab('profile')}
            className={`px-4 py-2 font-medium transition-colors ${
              activeTab === 'profile'
                ? 'border-b-2 border-primary text-primary'
                : 'text-muted-foreground hover:text-foreground'
            }`}
          >
            Profil Bilgileri
          </button>
          <button
            type="button"
            onClick={() => setActiveTab('password')}
            className={`px-4 py-2 font-medium transition-colors ${
              activeTab === 'password'
                ? 'border-b-2 border-primary text-primary'
                : 'text-muted-foreground hover:text-foreground'
            }`}
          >
            Şifre Değiştir
          </button>
        </div>

        {/* Profile Tab */}
        {activeTab === 'profile' && (
          <Card>
            <CardHeader>
              <CardTitle>Profil Bilgileri</CardTitle>
              <CardDescription>
                Kullanıcı adı, e-posta ve telefon numaranızı güncelleyin
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={profileForm.handleSubmit(onProfileSubmit)} className="space-y-6">
                {/* Profile Picture */}
                <ImageUploadField
                  label="Profil Fotoğrafı"
                  description="Profil fotoğrafınızı yükleyin (Maksimum 2MB, JPG/PNG)"
                  value={profileForm.watch('profilePictureUrl') || ''}
                  onChange={(value) => profileForm.setValue('profilePictureUrl', value)}
                  onRemove={() => profileForm.setValue('profilePictureUrl', '')}
                  scope="profiles"
                  resizeMode="crop"
                  maxWidth={400}
                  maxHeight={400}
                />

                {/* User Name */}
                <div className="space-y-2">
                  <Label htmlFor="userName" className="flex items-center gap-2">
                    <User className="h-4 w-4" />
                    Kullanıcı Adı
                  </Label>
                  <Input
                    id="userName"
                    {...profileForm.register('userName')}
                    placeholder="Kullanıcı adınız"
                  />
                  {profileForm.formState.errors.userName && (
                    <p className="text-sm text-destructive">
                      {profileForm.formState.errors.userName.message}
                    </p>
                  )}
                </div>

                {/* Email */}
                <div className="space-y-2">
                  <Label htmlFor="email" className="flex items-center gap-2">
                    <Mail className="h-4 w-4" />
                    E-posta Adresi
                  </Label>
                  <Input
                    id="email"
                    type="email"
                    {...profileForm.register('email')}
                    placeholder="E-posta adresiniz"
                  />
                  {profileForm.formState.errors.email && (
                    <p className="text-sm text-destructive">
                      {profileForm.formState.errors.email.message}
                    </p>
                  )}
                  {profile?.emailConfirmed && (
                    <p className="text-sm text-green-600 flex items-center gap-1">
                      <Mail className="h-3 w-3" />
                      E-posta adresiniz doğrulanmış
                    </p>
                  )}
                </div>

                {/* Phone Number */}
                <div className="space-y-2">
                  <Label htmlFor="phoneNumber" className="flex items-center gap-2">
                    <Phone className="h-4 w-4" />
                    Telefon Numarası
                  </Label>
                  <Input
                    id="phoneNumber"
                    type="tel"
                    {...profileForm.register('phoneNumber')}
                    placeholder="Telefon numaranız (opsiyonel)"
                  />
                  {profileForm.formState.errors.phoneNumber && (
                    <p className="text-sm text-destructive">
                      {profileForm.formState.errors.phoneNumber.message}
                    </p>
                  )}
                </div>

                {/* Submit Button */}
                <div className="flex justify-end gap-3">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => profileForm.reset()}
                    disabled={updateProfileMutation.isPending}
                  >
                    <X className="h-4 w-4 mr-2" />
                    İptal
                  </Button>
                  <Button type="submit" disabled={updateProfileMutation.isPending}>
                    <Save className="h-4 w-4 mr-2" />
                    {updateProfileMutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        )}

        {/* Password Tab */}
        {activeTab === 'password' && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Lock className="h-5 w-5" />
                Şifre Değiştir
              </CardTitle>
              <CardDescription>
                Güvenliğiniz için düzenli olarak şifrenizi değiştirmenizi öneririz
              </CardDescription>
            </CardHeader>
            <CardContent>
              <form onSubmit={passwordForm.handleSubmit(onPasswordSubmit)} className="space-y-6">
                {/* Current Password */}
                <div className="space-y-2">
                  <Label htmlFor="currentPassword">Mevcut Şifre</Label>
                  <Input
                    id="currentPassword"
                    type="password"
                    {...passwordForm.register('currentPassword')}
                    placeholder="Mevcut şifrenizi giriniz"
                  />
                  {passwordForm.formState.errors.currentPassword && (
                    <p className="text-sm text-destructive">
                      {passwordForm.formState.errors.currentPassword.message}
                    </p>
                  )}
                </div>

                {/* New Password */}
                <div className="space-y-2">
                  <Label htmlFor="newPassword">Yeni Şifre</Label>
                  <Input
                    id="newPassword"
                    type="password"
                    {...passwordForm.register('newPassword')}
                    placeholder="Yeni şifrenizi giriniz (en az 8 karakter)"
                  />
                  {passwordForm.formState.errors.newPassword && (
                    <p className="text-sm text-destructive">
                      {passwordForm.formState.errors.newPassword.message}
                    </p>
                  )}
                </div>

                {/* Confirm Password */}
                <div className="space-y-2">
                  <Label htmlFor="confirmPassword">Yeni Şifre (Tekrar)</Label>
                  <Input
                    id="confirmPassword"
                    type="password"
                    {...passwordForm.register('confirmPassword')}
                    placeholder="Yeni şifrenizi tekrar giriniz"
                  />
                  {passwordForm.formState.errors.confirmPassword && (
                    <p className="text-sm text-destructive">
                      {passwordForm.formState.errors.confirmPassword.message}
                    </p>
                  )}
                </div>

                {/* Password Requirements */}
                <div className="rounded-lg bg-muted p-4">
                  <p className="text-sm font-medium mb-2">Şifre Gereksinimleri:</p>
                  <ul className="text-sm text-muted-foreground space-y-1 list-disc list-inside">
                    <li>En az 8 karakter uzunluğunda olmalıdır</li>
                    <li>Mevcut şifrenizden farklı olmalıdır</li>
                  </ul>
                </div>

                {/* Submit Button */}
                <div className="flex justify-end gap-3">
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => passwordForm.reset()}
                    disabled={changePasswordMutation.isPending}
                  >
                    <X className="h-4 w-4 mr-2" />
                    İptal
                  </Button>
                  <Button type="submit" disabled={changePasswordMutation.isPending}>
                    <Lock className="h-4 w-4 mr-2" />
                    {changePasswordMutation.isPending ? 'Değiştiriliyor...' : 'Şifreyi Değiştir'}
                  </Button>
                </div>
              </form>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
