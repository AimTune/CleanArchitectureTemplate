using Application.Repositories.Test;
using Persistence.Contexts;
using Persistence.Repositories.Base;


namespace Persistence.Repositories.Test
{
    public class TestReadRepository : ReadRepository<TestEntity>, ITestReadRepository
    {
        public TestReadRepository(AppDbContext context) : base(context)
        {
        }
    }
}
