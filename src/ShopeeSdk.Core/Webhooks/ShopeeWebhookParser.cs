using System.Text.Json;
using ShopeeSdk.Abstractions.Webhooks;
using ShopeeSdk.Generated.Webhooks;

namespace ShopeeSdk.Core.Webhooks;

public sealed class ShopeeWebhookParser(
    IShopeeWebhookVerifier verifier,
    ShopeeWebhookTimestampValidator timestampValidator,
    IShopeeWebhookIdempotencyKeyFactory idempotencyKeyFactory) : IShopeeWebhookParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ShopeeWebhookReceiveResult TryReceive(string rawBody, string signature, DateTimeOffset receivedAt)
    {
        if (!verifier.Verify(rawBody, signature))
        {
            return ShopeeWebhookReceiveResult.Reject("invalid_signature");
        }

        ShopeeWebhookEvent? webhookEvent;
        try
        {
            webhookEvent = JsonSerializer.Deserialize<ShopeeWebhookEvent>(rawBody, JsonOptions);
        }
        catch (JsonException)
        {
            return ShopeeWebhookReceiveResult.Reject("invalid_payload");
        }

        if (webhookEvent is null)
        {
            return ShopeeWebhookReceiveResult.Reject("empty_payload");
        }

        if (!timestampValidator.IsFresh(webhookEvent, receivedAt))
        {
            return ShopeeWebhookReceiveResult.Reject("stale_timestamp");
        }

        var idempotencyKey = idempotencyKeyFactory.Create(rawBody, webhookEvent);
        var envelope = new ShopeeWebhookEnvelope("shopee", rawBody, signature, idempotencyKey, receivedAt, webhookEvent);
        return ShopeeWebhookReceiveResult.Accept(envelope);
    }

    public object ParseData(ShopeeWebhookEvent webhookEvent)
        => webhookEvent.Code switch
        {
            ShopeeWebhookEventCode.ReservedStockChangePush => ParseData<ReservedStockChangePushData>(webhookEvent),
            ShopeeWebhookEventCode.VideoUploadPush => ParseData<VideoUploadPushData>(webhookEvent),
            ShopeeWebhookEventCode.BrandRegisterResult => ParseData<BrandRegisterResultData>(webhookEvent),
            ShopeeWebhookEventCode.ViolationItemPush => ParseData<ViolationItemPushData>(webhookEvent),
            ShopeeWebhookEventCode.ItemPriceUpdatePush => ParseData<ItemPriceUpdatePushData>(webhookEvent),
            ShopeeWebhookEventCode.ItemScheduledPublishFailedPush => ParseData<ItemScheduledPublishFailedPushData>(webhookEvent),
            ShopeeWebhookEventCode.OrderStatusPush => ParseData<OrderStatusPushData>(webhookEvent),
            ShopeeWebhookEventCode.OrderTrackingNoPush => ParseData<OrderTrackingnoPushData>(webhookEvent),
            ShopeeWebhookEventCode.ShippingDocumentStatusPush => ParseData<ShippingDocumentStatusPushData>(webhookEvent),
            ShopeeWebhookEventCode.BookingStatusPush => ParseData<BookingStatusPushData>(webhookEvent),
            ShopeeWebhookEventCode.BookingTrackingNoPush => ParseData<BookingTrackingnoPushData>(webhookEvent),
            ShopeeWebhookEventCode.BookingShippingDocumentStatusPush => ParseData<BookingShippingDocumentStatusPushData>(webhookEvent),
            ShopeeWebhookEventCode.PackageFulfillmentStatusPush => ParseData<PackageFulfillmentStatusPushData>(webhookEvent),
            ShopeeWebhookEventCode.CourierDeliveryBindingStatusPush => ParseData<CourierDeliveryBindingStatusPushData>(webhookEvent),
            ShopeeWebhookEventCode.PackageInfoPush => ParseData<PackageInfoPushData>(webhookEvent),
            ShopeeWebhookEventCode.ReturnUpdatesPush => ParseData<ReturnUpdatesPushData>(webhookEvent),
            ShopeeWebhookEventCode.ItemPromotionPush => ParseData<ItemPromotionPushData>(webhookEvent),
            ShopeeWebhookEventCode.PromotionUpdatePush => ParseData<PromotionUpdatePushData>(webhookEvent),
            ShopeeWebhookEventCode.ShopeeUpdates => ParseData<ShopeeUpdatesData>(webhookEvent),
            ShopeeWebhookEventCode.OpenApiAuthorizationExpiry => ParseData<OpenApiAuthorizationExpiryData>(webhookEvent),
            ShopeeWebhookEventCode.ShopAuthorizationPush => ParseData<ShopAuthorizationPushData>(webhookEvent),
            ShopeeWebhookEventCode.ShopAuthorizationCanceledPush => ParseData<ShopAuthorizationCanceledPushData>(webhookEvent),
            ShopeeWebhookEventCode.ShopPenaltyUpdatePush => ParseData<ShopPenaltyUpdatePushData>(webhookEvent),
            ShopeeWebhookEventCode.VideoUploadResultPush => ParseData<VideoUploadResultPushData>(webhookEvent),
            ShopeeWebhookEventCode.WebchatPush => ParseData<WebchatPushData>(webhookEvent),
            ShopeeWebhookEventCode.InboundStatusPush => ParseData<InboundStatusPushData>(webhookEvent),
            ShopeeWebhookEventCode.SupplierCreateProductPush => ParseData<SupplierCreateProductPushData>(webhookEvent),
            ShopeeWebhookEventCode.SupplierProductReviewResultPush => ParseData<SupplierProductReviewResultPushData>(webhookEvent),
            ShopeeWebhookEventCode.PurchaseOrderPush => ParseData<PurchaseOrderPushData>(webhookEvent),
            ShopeeWebhookEventCode.FbsSellableStock => ParseData<FbsSellableStockData>(webhookEvent),
            ShopeeWebhookEventCode.FbsBrInvoiceErrorPush => ParseData<FbsBrInvoiceErrorPushData>(webhookEvent),
            ShopeeWebhookEventCode.FbsBrBlockShopPush => ParseData<FbsBrBlockShopPushData>(webhookEvent),
            ShopeeWebhookEventCode.FbsBrBlockSkuPush => ParseData<FbsBrBlockSkuPushData>(webhookEvent),
            ShopeeWebhookEventCode.FbsBrInvoiceIssuedPush => ParseData<FbsBrInvoiceIssuedPushData>(webhookEvent),
            _ => ParseData<ShopeeWebhookUnknownData>(webhookEvent)
        };

    public TData ParseData<TData>(ShopeeWebhookEvent webhookEvent)
        => webhookEvent.Data.Deserialize<TData>(JsonOptions)
            ?? throw new JsonException($"Shopee webhook data could not be deserialized as {typeof(TData).Name}.");
}
