using System;
using System.Collections.Generic;

namespace Foundation.Features.Ucp;

public class UcpConfiguration
{
    public string MerchantHostname { get; set; }
    public string GooglePayMerchantId { get; set; }
    public string GooglePayMerchantName { get; set; }
    public string DefaultMarketId { get; set; } = "DEFAULT";
    public string DefaultLanguageCode { get; set; } = "en";
    public Guid UcpCustomerId { get; set; }
    public List<string> AllowedCardNetworks { get; set; } = new() { "VISA", "MASTERCARD", "AMEX" };
    public List<string> AllowedAuthMethods { get; set; } = new() { "PAN_ONLY", "CRYPTOGRAM_3DS" };
    public string PaymentGateway { get; set; } = "example";
    public string PaymentGatewayMerchantId { get; set; }
}