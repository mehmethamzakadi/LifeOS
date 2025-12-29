/**
 * Backend'deki Permissions.cs ile senkronize edilmiş permission constant'ları
 * Format: {ModuleName}.{PermissionType}
 */
export const Permissions = {
  // Dashboard
  DashboardView: "Dashboard.View",

  // User Management
  UsersCreate: "Users.Create",
  UsersRead: "Users.Read",
  UsersUpdate: "Users.Update",
  UsersDelete: "Users.Delete",
  UsersViewAll: "Users.ViewAll",

  // Role Management
  RolesCreate: "Roles.Create",
  RolesRead: "Roles.Read",
  RolesUpdate: "Roles.Update",
  RolesDelete: "Roles.Delete",
  RolesViewAll: "Roles.ViewAll",
  RolesAssignPermissions: "Roles.AssignPermissions",

  // Category Management
  CategoriesCreate: "Categories.Create",
  CategoriesRead: "Categories.Read",
  CategoriesUpdate: "Categories.Update",
  CategoriesDelete: "Categories.Delete",
  CategoriesViewAll: "Categories.ViewAll",

  // Media Management
  MediaUpload: "Media.Upload",

  // Activity Logs
  ActivityLogsView: "ActivityLogs.View",
} as const;

export type Permission = (typeof Permissions)[keyof typeof Permissions];
