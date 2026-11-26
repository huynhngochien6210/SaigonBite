namespace SaigonBite.Services.Payment
{
    public class CODPayment : IPaymentStrategy
    {
        public async Task<bool> PayAsync(decimal amount)
        {
            await Task.CompletedTask;

            return amount <= 1000000;
        }
    }
}