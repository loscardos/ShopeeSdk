namespace ShopeeSdk.Abstractions.Http;

public enum ShopeeErrorCategory
{
    Unknown,
    Authentication,
    Signature,
    Validation,
    Permission,
    RateLimit,
    Transient
}
