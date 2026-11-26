namespace SaigonBite.Services.Payment
{
    public class VNPayPayment : IPaymentStrategy
    {
        public async Task<bool> PayAsync(decimal amount)
        {
            // TODO: Integrate VNPay later
            await Task.CompletedTask;

            return true;
        }
    }
}