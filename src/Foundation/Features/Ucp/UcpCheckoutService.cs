using EPiServer.Logging;
using Foundation.Features.CatalogContent.Services;
using Foundation.Features.Checkout.Services;
using Foundation.Features.Ucp.Models;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;
using Microsoft.Extensions.Options;

namespace Foundation.Features.Ucp;

public class UcpCheckoutService : IUcpCheckoutService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderGroupFactory _orderGroupFactory;
    private readonly IOrderGroupCalculator _orderGroupCalculator;
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly ICurrentMarket _currentMarket;
    private readonly IMarketService _marketService;
    private readonly ReferenceConverter _referenceConverter;
    private readonly IContentLoader _contentLoader;
    private readonly IPricingService _pricingService;
    private readonly IShippingService _shippingService;
    private readonly IPromotionEngine _promotionEngine;
    private readonly UcpConfiguration _config;
    private readonly ILogger _log = LogManager.GetLogger(typeof(UcpCheckoutService));

    public UcpCheckoutService(
        IOrderRepository orderRepository,
        IOrderGroupFactory orderGroupFactory,
        IOrderGroupCalculator orderGroupCalculator,
        IPaymentProcessor paymentProcessor,
        ICurrentMarket currentMarket,
        IMarketService marketService,
        ReferenceConverter referenceConverter,
        IContentLoader contentLoader,
        IPricingService pricingService,
        IShippingService shippingService,
        IPromotionEngine promotionEngine,
        IOptions<UcpConfiguration> options)
    {
        _orderRepository = orderRepository;
        _orderGroupFactory = orderGroupFactory;
        _orderGroupCalculator = orderGroupCalculator;
        _paymentProcessor = paymentProcessor;
        _currentMarket = currentMarket;
        _marketService = marketService;
        _referenceConverter = referenceConverter;
        _contentLoader = contentLoader;
        _pricingService = pricingService;
        _shippingService = shippingService;
        _promotionEngine = promotionEngine;
        _config = options.Value;
    }

    public Task<UcpCheckoutSessionResponse> CreateSessionAsync(CreateCheckoutSessionRequest request)
    {
        var sessionId = "session_" + Guid.NewGuid().ToString("N");
        var cartName = $"UcpCheckout-{sessionId}";
        var market = _marketService.GetMarket(new MarketId(_config.DefaultMarketId));
        var currency = string.IsNullOrEmpty(request.Currency)
            ? market.DefaultCurrency
            : new Currency(request.Currency);

        var cart = _orderRepository.LoadOrCreateCart<ICart>(
            _config.UcpCustomerId, cartName, _currentMarket);
        cart.Currency = currency;
        cart.MarketId = market.MarketId;
        cart.Properties["UcpStatus"] = "CREATED";

        var shipment = cart.GetFirstShipment();
        if (shipment == null)
        {
            shipment = _orderGroupFactory.CreateShipment(cart);
            cart.GetFirstForm().Shipments.Add(shipment);
        }

        foreach (var item in request.LineItems)
        {
            var code = item.Item.Id;
            var contentLink = _referenceConverter.GetContentLink(code);
            var entry = _contentLoader.Get<EntryContentBase>(contentLink);

            var lineItem = cart.CreateLineItem(code, _orderGroupFactory);
            lineItem.DisplayName = !string.IsNullOrEmpty(item.Item.Title)
                ? item.Item.Title
                : entry.DisplayName;
            lineItem.Quantity = item.Quantity;

            var price = _pricingService.GetPrice(code, market.MarketId, currency);
            if (price.HasValue)
            {
                lineItem.PlacedPrice = price.Value.Amount;
            }

            cart.AddLineItem(shipment, lineItem);
        }

        cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
        _orderRepository.Save(cart);

        return Task.FromResult(BuildResponse(cart, sessionId));
    }

    public Task<UcpCheckoutSessionResponse> GetSessionAsync(string sessionId)
    {
        var cart = LoadCart(sessionId);
        if (cart == null)
        {
            return Task.FromResult<UcpCheckoutSessionResponse>(null);
        }

        return Task.FromResult(BuildResponse(cart, sessionId));
    }

    public Task<UcpCheckoutSessionResponse> UpdateSessionAsync(string sessionId, UpdateCheckoutSessionRequest request)
    {
        var cart = LoadCart(sessionId);
        if (cart == null)
        {
            return Task.FromResult<UcpCheckoutSessionResponse>(null);
        }

        var market = _marketService.GetMarket(new MarketId(_config.DefaultMarketId));

        // Update buyer info
        if (request.Buyer != null)
        {
            cart.Properties["UcpBuyerEmail"] = request.Buyer.Email;
            cart.Properties["UcpBuyerFirstName"] = request.Buyer.FirstName;
            cart.Properties["UcpBuyerLastName"] = request.Buyer.LastName;
            cart.Properties["UcpBuyerPhone"] = request.Buyer.Phone;
        }

        // Update shipping address
        if (request.Fulfillment?.ShippingAddress != null)
        {
            var addr = request.Fulfillment.ShippingAddress;
            var shipment = cart.GetFirstShipment();
            if (shipment != null)
            {
                var orderAddress = _orderGroupFactory.CreateOrderAddress(cart);
                orderAddress.Line1 = addr.AddressLine1;
                orderAddress.Line2 = addr.AddressLine2;
                orderAddress.City = addr.City;
                orderAddress.RegionCode = addr.State;
                orderAddress.PostalCode = addr.PostalCode;
                orderAddress.CountryCode = addr.CountryCode;
                orderAddress.Id = "UcpShipping";

                if (request.Buyer != null)
                {
                    orderAddress.FirstName = request.Buyer.FirstName;
                    orderAddress.LastName = request.Buyer.LastName;
                    orderAddress.Email = request.Buyer.Email;
                }

                shipment.ShippingAddress = orderAddress;
            }

            cart.Properties["UcpStatus"] = "SHIPPING_REQUIRED";
        }

        // Update selected shipping option
        if (!string.IsNullOrEmpty(request.SelectedShippingOptionId))
        {
            var shipment = cart.GetFirstShipment();
            if (shipment != null && Guid.TryParse(request.SelectedShippingOptionId, out var methodId))
            {
                shipment.ShippingMethodId = methodId;
            }
        }

        cart.ApplyDiscounts(_promotionEngine, new PromotionEngineSettings());
        _orderRepository.Save(cart);

        return Task.FromResult(BuildResponse(cart, sessionId));
    }

    public Task<UcpCheckoutSessionResponse> CompleteSessionAsync(string sessionId, CompleteCheckoutSessionRequest request)
    {
        var cart = LoadCart(sessionId);
        if (cart == null)
        {
            return Task.FromResult<UcpCheckoutSessionResponse>(null);
        }

        var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
        var payment = _orderGroupFactory.CreatePayment(cart);
        payment.Amount = totals.Total.Amount;
        payment.PaymentMethodName = "GooglePay";
        payment.Status = nameof(PaymentStatus.Pending);

        if (request.PaymentData?.Credential?.GooglePayToken != null)
        {
            payment.Properties["GooglePayToken"] = request.PaymentData.Credential.GooglePayToken;
        }

        cart.AddPayment(payment, _orderGroupFactory);

        var processResults = cart.ProcessPayments(_paymentProcessor, _orderGroupCalculator);
        var failures = processResults.Where(r => !r.IsSuccessful).ToList();
        if (failures.Count != 0)
        {
            var errorMsg = string.Join("; ", failures.Select(f => f.Message));
            _log.Error($"UCP payment processing failed for session {sessionId}: {errorMsg}");
            throw new InvalidOperationException($"Payment processing failed: {errorMsg}");
        }

        var processedPayments = cart.GetFirstForm().Payments
            .Where(p => p.Status == nameof(PaymentStatus.Processed));
        if (!processedPayments.Any())
        {
            throw new InvalidOperationException("No payment was processed successfully.");
        }

        var orderReference = _orderRepository.SaveAsPurchaseOrder(cart);
        var purchaseOrder = _orderRepository.Load<IPurchaseOrder>(orderReference.OrderGroupId);
        _orderRepository.Delete(cart.OrderLink);
        cart.AdjustInventoryOrRemoveLineItems((item, issue) => { });

        var response = new UcpCheckoutSessionResponse
        {
            GlobalId = BuildGlobalId(sessionId),
            SessionId = sessionId,
            Status = "COMPLETED",
            OrderConfirmation = new UcpOrderConfirmation
            {
                OrderId = purchaseOrder.OrderLink.OrderGroupId.ToString(),
            },
        };

        return Task.FromResult(response);
    }

    public Task<UcpCheckoutSessionResponse> CancelSessionAsync(string sessionId)
    {
        var cart = LoadCart(sessionId);
        if (cart == null)
        {
            return Task.FromResult<UcpCheckoutSessionResponse>(null);
        }

        _orderRepository.Delete(cart.OrderLink);

        var response = new UcpCheckoutSessionResponse
        {
            GlobalId = BuildGlobalId(sessionId),
            SessionId = sessionId,
            Status = "CANCELLED",
        };

        return Task.FromResult(response);
    }

    private ICart LoadCart(string sessionId)
    {
        var cartName = $"UcpCheckout-{sessionId}";
        return _orderRepository.Load<ICart>(_config.UcpCustomerId, cartName)
            .FirstOrDefault();
    }

    private string BuildGlobalId(string sessionId)
    {
        return $"gid://{_config.MerchantHostname}/Checkout/{sessionId}";
    }

    private UcpCheckoutSessionResponse BuildResponse(ICart cart, string sessionId)
    {
        var market = _marketService.GetMarket(new MarketId(_config.DefaultMarketId));
        var totals = _orderGroupCalculator.GetOrderGroupTotals(cart);
        var discountTotal = _orderGroupCalculator.GetOrderDiscountTotal(cart);
        var status = cart.Properties["UcpStatus"]?.ToString() ?? "CREATED";

        var lineItems = cart.GetAllLineItems().Select(li => new UcpLineItemResponse
        {
            ItemId = li.Code,
            Title = li.DisplayName,
            Quantity = (int)li.Quantity,
            UnitPrice = ToMinorUnits(li.PlacedPrice, cart.Currency),
        }).ToList();

        var response = new UcpCheckoutSessionResponse
        {
            GlobalId = BuildGlobalId(sessionId),
            SessionId = sessionId,
            Status = status,
            LineItems = lineItems,
            Totals = new UcpTotals
            {
                Subtotal = ToMinorUnits(totals.SubTotal.Amount, cart.Currency),
                Tax = ToMinorUnits(totals.TaxTotal.Amount, cart.Currency),
                Shipping = ToMinorUnits(totals.ShippingTotal.Amount, cart.Currency),
                Discount = ToMinorUnits(discountTotal.Amount, cart.Currency),
                Total = ToMinorUnits(totals.Total.Amount, cart.Currency),
            },
            PaymentHandlerConfig = BuildPaymentHandlerConfig(),
        };

        // Include shipping options when address is provided
        if (cart.GetFirstShipment()?.ShippingAddress != null)
        {
            response.ShippingOptions = GetShippingOptions(cart, market);
        }

        return response;
    }

    private List<UcpShippingOption> GetShippingOptions(ICart cart, IMarket market)
    {
        var methods = _shippingService.GetShippingMethodsByMarket(
            market.MarketId.Value, false);
        var shipment = cart.GetFirstShipment();
        var options = new List<UcpShippingOption>();

        foreach (var method in methods)
        {
            try
            {
                var rate = _shippingService.GetRate(shipment, method, market);
                if (rate != null)
                {
                    options.Add(new UcpShippingOption
                    {
                        Id = method.MethodId.ToString(),
                        Title = method.Name,
                        Price = ToMinorUnits(rate.Money.Amount, cart.Currency),
                    });
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to get rate for shipping method {method.Name}: {ex.Message}");
            }
        }

        return options;
    }

    private UcpPaymentHandlerConfig BuildPaymentHandlerConfig()
    {
        return new UcpPaymentHandlerConfig
        {
            GooglePay = new GooglePayConfig
            {
                MerchantInfo = new GooglePayMerchantInfo
                {
                    MerchantId = _config.GooglePayMerchantId,
                    MerchantName = _config.GooglePayMerchantName,
                },
                AllowedCardNetworks = _config.AllowedCardNetworks,
                AllowedAuthMethods = _config.AllowedAuthMethods,
                TokenizationSpec = new GooglePayTokenizationSpec
                {
                    Type = "PAYMENT_GATEWAY",
                    Parameters = new Dictionary<string, string>
                    {
                        ["gateway"] = _config.PaymentGateway,
                        ["gatewayMerchantId"] = _config.PaymentGatewayMerchantId,
                    },
                },
            },
        };
    }

    private static long ToMinorUnits(decimal amount, Currency currency)
    {
        var currencyDecimalDigits = currency.Format.CurrencyDecimalDigits;
        return (long)Math.Round(amount * (decimal)Math.Pow(10, currencyDecimalDigits));
    }
}