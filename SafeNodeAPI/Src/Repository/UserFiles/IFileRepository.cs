using SafeNodeAPI.Models.DTO;

namespace SafeNodeAPI.Src.Repository.UserFiles
{
    public interface IFileRepository
    {
        Task<FileRecord?> GetFileByIdAsync(int id);
        Task<FileRecord> CreateFileAsync(FileRecord file);
    }
}
