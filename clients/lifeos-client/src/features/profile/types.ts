export interface UserProfile {
  id: string;
  userName: string;
  email: string;
  phoneNumber?: string;
  profilePictureUrl?: string;
  emailConfirmed: boolean;
  createdDate: string;
}

export interface UpdateProfileFormValues {
  userName: string;
  email: string;
  phoneNumber?: string;
  profilePictureUrl?: string;
}

export interface ChangePasswordFormValues {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
