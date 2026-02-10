using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Foundation.Features.Ucp.Models;

public class UcpCheckoutSessionResponse
{
    [JsonPropertyName("global_id")]
    public string GlobalId { get; set; }

    [JsonPropertyName("session_id")]
    public string SessionId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("line_items")]
    public List<UcpLineItemResponse> LineItems { get; set; } = new();

    [JsonPropertyName("totals")]
    public UcpTotals Totals { get; set; }

    [JsonPropertyName("shipping_options")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<UcpShippingOption> ShippingOptions { get; set; }

    [JsonPropertyName("payment_handler_config")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UcpPaymentHandlerConfig PaymentHandlerConfig { get; set; }

    [JsonPropertyName("messages")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<UcpMessage> Messages { get; set; }

    [JsonPropertyName("order_confirmation")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UcpOrderConfirmation OrderConfirmation { get; set; }
}

public class UcpLineItemResponse
{
    [JsonPropertyName("item_id")]
    public string ItemId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("unit_price")]
    public long UnitPrice { get; set; }
}

public class UcpTotals
{
    [JsonPropertyName("subtotal")]
    public long Subtotal { get; set; }

    [JsonPropertyName("tax")]
    public long Tax { get; set; }

    [JsonPropertyName("shipping")]
    public long Shipping { get; set; }

    [JsonPropertyName("discount")]
    public long Discount { get; set; }

    [JsonPropertyName("total")]
    public long Total { get; set; }
}

public class UcpShippingOption
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("price")]
    public long Price { get; set; }

    [JsonPropertyName("delivery_window")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string DeliveryWindow { get; set; }
}

public class UcpPaymentHandlerConfig
{
    [JsonPropertyName("google_pay")]
    public GooglePayConfig GooglePay { get; set; }
}

public class GooglePayConfig
{
    [JsonPropertyName("merchant_info")]
    public GooglePayMerchantInfo MerchantInfo { get; set; }

    [JsonPropertyName("allowed_card_networks")]
    public List<string> AllowedCardNetworks { get; set; }

    [JsonPropertyName("allowed_auth_methods")]
    public List<string> AllowedAuthMethods { get; set; }

    [JsonPropertyName("tokenization_spec")]
    public GooglePayTokenizationSpec TokenizationSpec { get; set; }
}

public class GooglePayMerchantInfo
{
    [JsonPropertyName("merchant_id")]
    public string MerchantId { get; set; }

    [JsonPropertyName("merchant_name")]
    public string MerchantName { get; set; }
}

public class GooglePayTokenizationSpec
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "PAYMENT_GATEWAY";

    [JsonPropertyName("parameters")]
    public Dictionary<string, string> Parameters { get; set; } = new();
}

public class UcpMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }
}

public class UcpOrderConfirmation
{
    [JsonPropertyName("order_id")]
    public string OrderId { get; set; }
}