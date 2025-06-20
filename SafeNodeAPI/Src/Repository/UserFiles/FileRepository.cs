﻿using Microsoft.EntityFrameworkCore;
using SafeNodeAPI.Data;
using SafeNodeAPI.Models.DTO;

namespace SafeNodeAPI.Src.Repository.UserFiles
{
    public class FileRepository(SafeNodeDbContext _dbContext) : IFileRepository
    {
        public async Task<FileRecord> CreateFileAsync(FileRecord file)
        {
            _dbContext.FileRecords.Add(file);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the file.", ex);
            }
            return file;
        }

        public Task<FileRecord?> GetFileByIdAsync(int id)
        {
            return _dbContext.FileRecords
                .Include(f => f.Folder)
                .Include(f => f.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<FileRecord?> UpdateFileAsync(FileRecord file)
        {
            _dbContext.Update(file);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the file.", ex);
            }
            return file;
        }

        public async Task<List<FileRecord>> GetFilesByFolderIdAsync(int? folderId)
        {
            var query = _dbContext.FileRecords
                .Include(f => f.Folder)
                .Include(f => f.User)
                .Include(f => f.FilePermissions!)
                    .ThenInclude(fp => fp.User)
                .Where(f => !f.IsDeleted);

            query = folderId.HasValue
                ? query.Where(f => f.FolderId == folderId.Value)
                : query.Where(f => f.FolderId == null);

            return await query.ToListAsync();
        }



    }
}
