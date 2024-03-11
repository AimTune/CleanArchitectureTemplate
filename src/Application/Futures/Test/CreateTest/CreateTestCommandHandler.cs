using Application.Abstractions.EventBus;
using Application.Repositories.Test;

namespace Application.Futures.Test.CreateTest
{
    public class CreateTestCommandHandler
        (ITestWriteRepository testWriteRepository, IUnitOfWork unitOfWork, IEventBus eventBus) : ICommandHandler<CreateTestCommand, bool>
    {
        private readonly ITestWriteRepository _testWriteRepository = testWriteRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IEventBus _eventBus = eventBus;

        public async Task<bool> Handle(CreateTestCommand request, CancellationToken cancellationToken)
        {
            /*
            var test = new TestEntity
            {
                Name = request.Name
            };

            var status = await _testWriteRepository.AddAsync(test);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            */
            
            await _eventBus.PublishAsync(new TestCreatedEvent
            {
                Id = 1,
                Name = request.Name
            }, cancellationToken);

            return true;
        }
    }
}
