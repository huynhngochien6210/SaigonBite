namespace SaigonBite.Services.Payment
{
    public interface IPaymentStrategy
    {
        Task<bool> PayAsync(decimal amount);
    }
}