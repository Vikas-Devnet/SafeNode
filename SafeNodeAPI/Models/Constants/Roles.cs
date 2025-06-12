namespace SafeNodeAPI.Models.Constants
{
    public enum UserRole
    {
        Admin = 1,   // Full control: can manage permissions, edit, delete
        Editor = 2,  // Can edit files/folders, but not manage permissions
        Viewer = 3   // Can only view content
    }
    public static class Roles
    {
        public const string Admin = nameof(UserRole.Admin);
        public const string Editor = nameof(UserRole.Editor);
        public const string Viewer = nameof(UserRole.Viewer);
    }
}
