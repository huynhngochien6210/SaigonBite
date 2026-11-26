namespace SaigonBite.Services.Payment
{
    public class MoMoPayment : IPaymentStrategy
    {
        public async Task<bool> PayAsync(decimal amount)
        {
            // TODO: Integrate MoMo later
            await Task.CompletedTask;

            return true;
        }
    }
}