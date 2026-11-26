namespace SaigonBite.Services.Payment
{
    public class ApplePayPayment : IPaymentStrategy
    {
        public async Task<bool> PayAsync(decimal amount)
        {
            // TODO: Integrate Apple Pay later
            await Task.CompletedTask;

            return true;
        }
    }
}