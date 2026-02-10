using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Foundation.Features.Ucp.Models;

public class CreateCheckoutSessionRequest
{
    [JsonPropertyName("line_items")]
    public List<UcpLineItemRequest> LineItems { get; set; } = new();

    [JsonPropertyName("currency")]
    public string Currency { get; set; }
}

public class UcpLineItemRequest
{
    [JsonPropertyName("item")]
    public UcpItemRef Item { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}

public class UcpItemRef
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
}

public class UpdateCheckoutSessionRequest
{
    [JsonPropertyName("buyer")]
    public UcpBuyer Buyer { get; set; }

    [JsonPropertyName("fulfillment")]
    public UcpFulfillment Fulfillment { get; set; }

    [JsonPropertyName("selected_shipping_option_id")]
    public string SelectedShippingOptionId { get; set; }

    [JsonPropertyName("payment_data")]
    public UcpPaymentData PaymentData { get; set; }
}

public class UcpBuyer
{
    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }
}

public class UcpFulfillment
{
    [JsonPropertyName("shipping_address")]
    public UcpAddress ShippingAddress { get; set; }
}

public class UcpAddress
{
    [JsonPropertyName("address_line_1")]
    public string AddressLine1 { get; set; }

    [JsonPropertyName("address_line_2")]
    public string AddressLine2 { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }
}

public class UcpPaymentData
{
    [JsonPropertyName("credential")]
    public UcpPaymentCredential Credential { get; set; }
}

public class UcpPaymentCredential
{
    [JsonPropertyName("google_pay_token")]
    public string GooglePayToken { get; set; }
}

public class CompleteCheckoutSessionRequest
{
    [JsonPropertyName("payment_data")]
    public UcpPaymentData PaymentData { get; set; }
}