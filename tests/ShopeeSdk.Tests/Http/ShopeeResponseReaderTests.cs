using FluentAssertions;
using ShopeeSdk.Core.Http;
using ShopeeSdk.Generated.Ads;
using ShopeeSdk.Generated.Order;
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
                "deboost": "FALSE",
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
        item.Deboost.Should().BeFalse();
    }

    [Fact]
    public void ReadJsonEnvelope_MapsModelShopeeStockWhenProviderReturnsNumber()
    {
        var envelope =
            ShopeeResponseReader
                .ReadJsonEnvelope<ProductGetModelListResponse>("""
        {
          "request_id": "req-model-stock",
          "error": "",
          "message": "",
          "response": {
            "model": [
              {
                "model_id": 1,
                "stock_info_v2": {
                  "shopee_stock": [
                    {
                      "location_id": "IDZ",
                      "stock": 12
                    }
                  ]
                }
              }
            ]
          }
        }
        """);

        var stock = envelope.Response!.Model.Should().ContainSingle()
            .Which.StockInfoV2!.ShopeeStock.Should().ContainSingle()
            .Which.Stock;
        ((object?)stock).Should().Be(12);
    }

    [Fact]
    public void ReadJsonEnvelope_MapsPromotionItemIdBeyondInt32()
    {
        var envelope =
            ShopeeResponseReader
                .ReadJsonEnvelope<ProductGetItemPromotionResponse>("""
        {
          "request_id": "req-promotion",
          "error": "",
          "message": "",
          "response": {
            "success_list": [
              {
                "item_id": 24261332641,
                "promotion": []
              }
            ]
          }
        }
        """);

        envelope.Response!.SuccessList.Should().ContainSingle()
            .Which.ItemId.Should().Be(24261332641);
    }

    [Fact]
    public void ReadJsonEnvelope_MapsOrderProductLocationIdWhenProviderReturnsNumber()
    {
        var envelope =
            ShopeeResponseReader
                .ReadJsonEnvelope<OrderGetOrderDetailResponse>("""
        {
          "request_id": "req-order-location",
          "error": "",
          "message": "",
          "response": {
            "order_list": [
              {
                "order_sn": "SHP-1",
                "item_list": [
                  {
                    "product_location_id": 123456789
                  }
                ]
              }
            ]
          }
        }
        """);

        envelope.Response!.OrderList.Should().ContainSingle()
            .Which.ItemList.Should().ContainSingle()
            .Which.ProductLocationId.Should().Be("123456789");
    }

    [Fact]
    public void ReadJsonEnvelope_MapsOrderProductLocationIdWhenProviderReturnsSingletonArray()
    {
        var envelope =
            ShopeeResponseReader
                .ReadJsonEnvelope<OrderGetOrderDetailResponse>("""
        {
          "response": {
            "order_list": [
              {
                "order_sn": "SHP-1",
                "item_list": [
                  {
                    "product_location_id": [123456789]
                  }
                ]
              }
            ]
          }
        }
        """);

        envelope.Response!.OrderList.Should().ContainSingle()
            .Which.ItemList.Should().ContainSingle()
            .Which.ProductLocationId.Should().Be("123456789");
    }

    private sealed record TestPayload(string Value);
}
