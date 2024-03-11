using Microsoft.EntityFrameworkCore;

namespace Application.Repositories.Base
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        DbSet<T> Table { get; }
    }
}
