using Models;

namespace Repository;
public interface ICustomerRepository
{
    Task<Customer?> GetAsync(Guid id);
}
