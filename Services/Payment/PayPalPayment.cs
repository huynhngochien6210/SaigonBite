namespace SaigonBite.Services.Payment
{
    public class PayPalPayment : IPaymentStrategy
    {
        public async Task<bool> PayAsync(decimal amount)
        {
            // TODO: Integrate PayPal later
            await Task.CompletedTask;

            return true;
        }
    }
}