using Application.Repositories.Test;
using Persistence.Contexts;
using Persistence.Repositories.Base;


namespace Persistence.Repositories.Test
{
    public class TestWriteRepository : WriteRepository<TestEntity>, ITestWriteRepository
    {
        public TestWriteRepository(AppDbContext context) : base(context)
        {
        }
    }
}
