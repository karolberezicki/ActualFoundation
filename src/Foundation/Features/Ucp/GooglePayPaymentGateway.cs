using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;

namespace Foundation.Features.Ucp;

public class GooglePayPaymentGateway : AbstractPaymentGateway, IPaymentPlugin
{
    public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
    {
        // In production, extract the Google Pay token and forward to a real payment processor
        // (Stripe, Braintree, Adyen, etc.)
        // var token = payment.Properties["GooglePayToken"]?.ToString();
        return PaymentProcessingResult.CreateSuccessfulResult("Google Pay payment processed.");
    }

    public override bool ProcessPayment(Payment payment, ref string message)
    {
        var result = ProcessPayment(null, payment);
        message = result.Message;
        return result.IsSuccessful;
    }
}