namespace SaigonBite.Services.Payment
{
    public class CODPayment : IPaymentStrategy
    {
        public async Task<bool> PayAsync(decimal amount)
        {
            await Task.CompletedTask;

            // COD chỉ được phép nếu <= 1,000,000
            return amount <= 1000000;
        }
    }
}