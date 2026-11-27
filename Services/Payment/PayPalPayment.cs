namespace SaigonBite.Services.Payment
{
    public class PayPalPayment : IPaymentStrategy
    {
        public async Task<bool> PayAsync(decimal amount)
        {
            await Task.CompletedTask;

            return true;
        }
    }
}