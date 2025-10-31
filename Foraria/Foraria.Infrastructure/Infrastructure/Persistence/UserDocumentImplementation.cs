using Foraria.Domain.Repository;
using Foraria.Interface.DTOs;
using ForariaDomain;
using Microsoft.EntityFrameworkCore;

namespace Foraria.Infrastructure.Persistence
{
    public class UserDocumentImplementation : IUserDocumentRepository
    {
        private readonly ForariaContext _context;

        public UserDocumentImplementation(ForariaContext context)
        {
            _context = context;
        }

        public async Task Add(UserDocument userDocument)
        {
            _context.UserDocuments.Add(userDocument);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserDocument>> GetAll()
        {
            return await _context.UserDocuments
                .Include(d => d.User)
                .Include(d => d.Consortium)
                .ToListAsync();
        }

        public async Task<UserDocument?> GetById(int id)
        {
            return await _context.UserDocuments
                .Include(d => d.User)
                .Include(d => d.Consortium)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public void Update(UserDocument userDocument)
        {
            _context.UserDocuments.Update(userDocument);
        }

        public async Task<List<UserDocument>> GetByCategoryAsync(string category, int? userId = null)
        {
            var query = _context.UserDocuments
                .Include(d => d.User)
                .Include(d => d.Consortium)
                .Where(d => d.Category.ToLower() == category.ToLower());

            if (userId.HasValue)
                query = query.Where(d => d.User_id == userId.Value);

            return await query.ToListAsync();
        }

        public async Task<DateTime?> GetLastUploadDateAsync(int? userId = null)
        {
            var query = _context.UserDocuments.AsQueryable();

            if (userId.HasValue)
                query = query.Where(d => d.User_id == userId.Value);

            return await query
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => (DateTime?)d.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<UserDocumentStatsDto> GetStatsAsync(int? userId = null)
        {
            var query = _context.UserDocuments
                .Include(d => d.User)
                .Include(d => d.Consortium)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(d => d.User_id == userId.Value);

            var all = await query.ToListAsync();

            var totalUserDocs = all.Count(d => d.User.Role.Description != "Consorcio");
            var totalConsortiumDocs = all.Count(d => d.User.Role.Description == "Consorcio");
            var totalCombined = all.Count;

            var documentsByCategory = all
                .GroupBy(d => d.Category)
                .ToDictionary(g => g.Key, g => g.Count());

            var lastUploadDate = all
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefault()?.CreatedAt;

            return new UserDocumentStatsDto
            {
                TotalUserDocuments = totalUserDocs,
                TotalConsortiumDocuments = totalConsortiumDocs,
                TotalCombined = totalCombined,
                DocumentsByCategory = documentsByCategory,
                LastUploadDate = lastUploadDate
            };
        }
    }
}
