namespace SaigonBite.Services.Payment
{
    public class PaymentContext
    {
        private IPaymentStrategy? _paymentStrategy;

        public void SetStrategy(IPaymentStrategy paymentStrategy)
        {
            _paymentStrategy = paymentStrategy;
        }

        public async Task<bool> PayAsync(decimal amount)
        {
            if (_paymentStrategy == null)
            {
                throw new InvalidOperationException(
                    "Payment strategy has not been selected.");
            }

            return await _paymentStrategy.PayAsync(amount);
        }
    }
}