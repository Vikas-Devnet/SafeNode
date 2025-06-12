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
                .AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
        }
    }
}
