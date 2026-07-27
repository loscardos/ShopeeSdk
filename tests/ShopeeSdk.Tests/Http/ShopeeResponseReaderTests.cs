using FluentAssertions;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Generated.Ads;
using ShopeeSdk.Generated.Payment;
using ShopeeSdk.Generated.Product;

namespace ShopeeSdk.Tests.Http;

public sealed class ShopeeResponseReaderTests
{
    [Fact]
    public void ReadJsonEnvelope_HandlesResponseWrappedPayload()
    {
        var envelope = ShopeeResponseReader.ReadJsonEnvelope<TestPayload>("""
        { "request_id": "req-1", "error": "", "message": "", "response": { "value": "ok" } }
        """);

        envelope.RequestId.Should().Be("req-1");
        envelope.Response!.Value.Should().Be("ok");
    }

    [Fact]
    public void ReadJsonEnvelope_HandlesTopLevelAuthPayload()
    {
        var envelope = ShopeeResponseReader.ReadJsonEnvelope<TestPayload>("""
        { "request_id": "req-2", "error": "", "message": "", "value": "ok" }
        """);

        envelope.RequestId.Should().Be("req-2");
        envelope.Response!.Value.Should().Be("ok");
    }

    [Fact]
    public void ReadJsonEnvelope_ThrowsSdkExceptionForEmptyPayload()
    {
        var act = () => ShopeeResponseReader.ReadJsonEnvelope<TestPayload>("");
        act.Should().Throw<Exception>().WithMessage("*empty*");
    }

    [Fact]
    public void ReadJsonEnvelope_HandlesAdsRecommendedItemListArrayPayload()
    {
        var envelope = ShopeeResponseReader.ReadJsonEnvelope<AdsGetRecommendedItemListResponse>("""
        {
          "request_id": "req-ads",
          "error": "",
          "message": "",
          "warning": "",
          "response": [
            {
              "item_id": 123,
              "item_status_list": ["NORMAL"],
              "sku_tag_list": ["ADS_ELIGIBLE"],
              "ongoing_ad_type_list": ["product_manual_bidding"]
            }
          ]
        }
        """);

        envelope.RequestId.Should().Be("req-ads");
        envelope.Response!.Items.Should().ContainSingle()
            .Which.ItemId.Should().Be(123);
    }

    [Fact]
    public void ReadJsonEnvelope_HandlesPaymentMethodListArrayPayload()
    {
        var envelope = ShopeeResponseReader.ReadJsonEnvelope<PaymentGetPaymentMethodListResponse>("""
        {
          "code": "",
          "message": "",
          "request_id": "req-payment",
          "response": [
            {
              "region": "ID",
              "payment_method": ["COD", "Credit Card"]
            }
          ]
        }
        """);

        envelope.RequestId.Should().Be("req-payment");
        envelope.Response!.Items.Should().ContainSingle()
            .Which.PaymentMethod.Should().Contain("COD");
    }

    [Fact]
    public void ReadJsonEnvelope_MapsItemLevelStockInfoV2()
    {
        var envelope =
            ShopeeResponseReader
                .ReadJsonEnvelope<ProductGetItemBaseInfoResponse>("""
        {
          "request_id": "req-stock",
          "error": "",
          "message": "",
          "response": {
            "item_list": [
              {
                "item_id": 56414886051,
                "item_sku": "SPE1010583390",
                "has_model": false,
                "stock_info_v2": {
                  "summary_info": {
                    "total_available_stock": 12
                  },
                  "seller_stock": [
                    {
                      "location_id": "IDZ",
                      "stock": 12,
                      "if_saleable": true
                    }
                  ]
                }
              }
            ]
          }
        }
        """);

        var item = envelope.Response!.ItemList.Should()
            .ContainSingle()
            .Which;
        item.StockInfoV2!.SummaryInfo!.TotalAvailableStock.Should().Be(12);
        item.StockInfoV2.SellerStock.Should().ContainSingle()
            .Which.LocationId.Should().Be("IDZ");
    }

    private sealed record TestPayload(string Value);
}
