using SEP490_G52_CSMS.Models.Sales;

namespace SEP490_G52_CSMS.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<List<MasterProduct>> GetAllAsync();

        Task<MasterProduct?> GetByIdAsync(int id);

        Task<bool> IsProductNameExistsAsync(string productName, int? excludeProductId = null);

        Task AddAsync(MasterProduct masterProduct);

        Task UpdateAsync(MasterProduct masterProduct);
    }
}