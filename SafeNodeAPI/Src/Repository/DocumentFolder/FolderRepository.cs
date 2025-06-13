using Microsoft.EntityFrameworkCore;
using SafeNodeAPI.Data;
using SafeNodeAPI.Models.DTO;

namespace SafeNodeAPI.Src.Repository.DocumentFolder
{
    public class FolderRepository(SafeNodeDbContext _dbContext) : IFolderRepository
    {
        public async Task<Folder> AddFolderAsync(Folder folder)
        {
            _dbContext.Add(folder);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while creating the folder.", ex);
            }
            return folder;
        }

        public Task<Folder?> GetFolderByIdAsync(int id)
        {
            return _dbContext.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Files)
                .Include(f => f.ParentFolder)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Folder?>> GetRootFoldersByUserIdAsync(int userId)
        {
            return await _dbContext.Folders
                .Include(f => f.SubFolders)
                .Include(f => f.Files)
                .Include(f => f.ParentFolder)
                .Where(f => f.CreatedByUserId == userId && f.ParentFolderId == null)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task DeleteFolderById(int folderId)
        {
            var folder = await _dbContext.Folders.FindAsync(folderId) ?? throw new KeyNotFoundException("Folder not found.");
            _dbContext.Folders.Remove(folder);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while deleting the folder.", ex);
            }
        }
    }
}
