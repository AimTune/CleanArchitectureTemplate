namespace Application.Futures.Test.CreateTest
{
    public class CreateTestCommand : ICommand<bool>
    {
        public string Name { get; set; } = string.Empty;
    }
}
