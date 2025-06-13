using SafeNodeAPI.Models.DTO;

namespace SafeNodeAPI.Src.Repository.DocumentFolder
{
    public interface IFolderRepository
    {
        Task<Folder> AddFolderAsync(Folder folder);
        Task<Folder?> GetFolderByIdAsync(int id);
        Task<IEnumerable<Folder?>> GetRootFoldersByUserIdAsync(int userId);
        Task DeleteFolderById(int folderId);
    }
}
