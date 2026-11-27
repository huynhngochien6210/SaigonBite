namespace SaigonBite.Services.Payment
{
    public class MoMoPayment : IPaymentStrategy
    {
        public async Task<bool> PayAsync(decimal amount)
        {
            await Task.CompletedTask;

            return true;
        }
    }
}