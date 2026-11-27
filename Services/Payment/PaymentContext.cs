namespace SaigonBite.Services.Payment
{
    public class PaymentContext
    {
        private IPaymentStrategy? _strategy;

        public void SetStrategy(IPaymentStrategy strategy)
        {
            _strategy = strategy;
        }

        public async Task<bool> PayAsync(decimal amount)
        {
            if (_strategy == null)
            {
                throw new InvalidOperationException(
                    "Payment strategy has not been selected.");
            }

            return await _strategy.PayAsync(amount);
        }
    }
}