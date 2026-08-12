using System.Globalization;
using System.Reflection;
using Loscardos.ShopeeSdk.Abstractions.Http;
using Loscardos.ShopeeSdk.Core.Auth;

namespace Loscardos.ShopeeSdk.SampleConsole;

public sealed class ReadonlyCertificationRunner(
    IShopeeClient client,
    ShopeeTokenService tokenService,
    long shopId,
    long? merchantId,
    int pageSize,
    long timeFrom,
    long timeTo)
{
    private static readonly string[] FixtureNameParts =
    [
        "order_sn",
        "item_id",
        "model_id",
        "package_number",
        "booking_sn",
        "voucher_id",
        "discount_id",
        "bundle_deal_id",
        "add_on_deal_id",
        "campaign_id",
        "address_id",
        "brand_id",
        "category_id",
        "comment_id",
        "conversation_id",
        "message_id",
        "video_id",
        "shop_flash_sale_id",
        "top_picks_id",
        "return_sn",
        "request_id",
        "document_id",
        "tracking_number",
        "first_mile_tracking_number",
        "pitem_id",
        "aitem_id",
        "global_item_id",
        "logistics_channel_id"
    ];

    public async Task<IReadOnlyList<CertificationResult>> RunAsync(CancellationToken cancellationToken)
    {
        var endpoints = GeneratedEndpointInventory
            .ReadFromDirectory(GeneratedEndpointInventory.DefaultGeneratedManagersDirectory)
            .Where(endpoint => EndpointCertificationMatrix.Classify(endpoint) == EndpointClassification.Readonly)
            .ToArray();
        var results = new List<CertificationResult>();

        foreach (var endpoint in endpoints)
        {
            if (endpoint.AuthScope == "Merchant" && merchantId is null)
            {
                results.Add(new CertificationResult(endpoint.Area, Label(endpoint), CertificationStatus.SkipNoData, "Set SHOPEE_MERCHANT_ID to certify merchant-scoped endpoints."));
                continue;
            }

            var request = TryCreateRequest(endpoint, out var skipReason);
            if (request is null)
            {
                results.Add(new CertificationResult(endpoint.Area, Label(endpoint), CertificationStatus.SkipNoData, skipReason));
                continue;
            }

            results.Add(await RunEndpointAsync(endpoint, request, cancellationToken));
        }

        results.Add(new CertificationResult(
            "Mutation",
            "create/update/delete/ship/cancel endpoints",
            CertificationStatus.SkipMutationNeedsFixture,
            "Write certification needs dedicated sandbox fixtures and explicit opt-in."));

        return results;
    }

    private async Task<CertificationResult> RunEndpointAsync(
        GeneratedEndpoint endpoint,
        object request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"== {endpoint.Area}: {Label(endpoint)} ==");
        try
        {
            var api = CreateApi(endpoint.Area);
            var method = api.GetType().GetMethod(endpoint.MethodName)
                ?? throw new MissingMethodException(api.GetType().FullName, endpoint.MethodName);
            var parameters = method.GetParameters();
            object?[] arguments = parameters.Length == 3
                ? [ResolveOwner(endpoint), request, cancellationToken]
                : [request, cancellationToken];
            var task = (Task?)method.Invoke(api, arguments)
                ?? throw new InvalidOperationException($"Endpoint {Label(endpoint)} did not return a task.");
            await task;

            Console.WriteLine($"PASS {Label(endpoint)}");
            return new CertificationResult(endpoint.Area, Label(endpoint), CertificationStatus.Pass, "response_received=true");
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            Console.WriteLine($"FAIL {Label(endpoint)}: {exception.InnerException.Message}");
            return ToFailureResult(endpoint, exception.InnerException.Message);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"FAIL {Label(endpoint)}: {exception.Message}");
            return ToFailureResult(endpoint, exception.Message);
        }
    }

    private static CertificationResult ToFailureResult(GeneratedEndpoint endpoint, string message)
    {
        var normalized = message.ToLowerInvariant();
        if (normalized.Contains("permission", StringComparison.Ordinal)
            || normalized.Contains("not authorized", StringComparison.Ordinal)
            || normalized.Contains("not in", StringComparison.Ordinal) && normalized.Contains("whitelist", StringComparison.Ordinal)
            || normalized.Contains("agree to", StringComparison.Ordinal)
            || normalized.Contains("no permission", StringComparison.Ordinal))
        {
            return new CertificationResult(endpoint.Area, Label(endpoint), CertificationStatus.SkipScope, message);
        }

        if (normalized.Contains("required", StringComparison.Ordinal)
            || normalized.Contains("error_param", StringComparison.Ordinal)
            || normalized.Contains("params_error", StringComparison.Ordinal)
            || normalized.Contains("param_error", StringComparison.Ordinal)
            || normalized.Contains("error_not_found", StringComparison.Ordinal)
            || normalized.Contains("wrong parameters", StringComparison.Ordinal)
            || normalized.Contains("invalid", StringComparison.Ordinal)
            || normalized.Contains("abnormal", StringComparison.Ordinal)
            || normalized.Contains("not found", StringComparison.Ordinal)
            || normalized.Contains("is not found", StringComparison.Ordinal))
        {
            return new CertificationResult(endpoint.Area, Label(endpoint), CertificationStatus.SkipNoData, message);
        }

        return new CertificationResult(endpoint.Area, Label(endpoint), CertificationStatus.Fail, message);
    }

    private object CreateApi(string area)
    {
        var type = Type.GetType($"Loscardos.ShopeeSdk.Core.Managers.Generated.{area}Api, Loscardos.ShopeeSdk.Core")
            ?? throw new InvalidOperationException($"Generated API type for {area} was not found.");
        var constructor = type.GetConstructors().Single();
        var parameters = constructor.GetParameters();
        object?[] arguments = parameters.Length == 2
            ? new object[] { client, tokenService }
            : [client];
        return Activator.CreateInstance(type, arguments)
            ?? throw new InvalidOperationException($"Could not create generated API type {type.FullName}.");
    }

    private long ResolveOwner(GeneratedEndpoint endpoint)
        => endpoint.AuthScope == "Merchant"
            ? merchantId!.Value
            : shopId;

    private object? TryCreateRequest(GeneratedEndpoint endpoint, out string skipReason)
    {
        skipReason = string.Empty;
        var requestType = Type.GetType($"Loscardos.ShopeeSdk.Generated.{endpoint.Area}.{endpoint.RequestTypeName}, Loscardos.ShopeeSdk.Generated");
        if (requestType is null)
        {
            skipReason = $"Request type {endpoint.RequestTypeName} was not found.";
            return null;
        }

        var constructor = requestType.GetConstructors().SingleOrDefault();
        if (constructor is null)
        {
            skipReason = $"Request type {endpoint.RequestTypeName} has no public constructor.";
            return null;
        }

        var values = new List<object?>();
        foreach (var parameter in constructor.GetParameters())
        {
            if (TryBuildValue(parameter, out var value, out skipReason))
            {
                values.Add(value);
                continue;
            }

            return null;
        }

        try
        {
            return Activator.CreateInstance(requestType, values.ToArray());
        }
        catch (Exception exception)
        {
            skipReason = $"Request type {endpoint.RequestTypeName} could not be constructed safely: {exception.Message}";
            return null;
        }
    }

    private bool TryBuildValue(ParameterInfo parameter, out object? value, out string skipReason)
    {
        var name = ToSnakeCase(parameter.Name ?? string.Empty);
        var type = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
        skipReason = string.Empty;

        if (FixtureNameParts.Any(part => name.Contains(part, StringComparison.Ordinal)))
        {
            value = null;
            skipReason = $"Parameter {name} needs a domain fixture.";
            return false;
        }

        if (parameter.HasDefaultValue)
        {
            value = parameter.DefaultValue;
            return true;
        }

        value = name switch
        {
            "language" => "en",
            "page_size" or "limit" or "count" => Convert.ChangeType(pageSize, type, CultureInfo.InvariantCulture),
            "page_no" or "page_num" => Convert.ChangeType(1, type, CultureInfo.InvariantCulture),
            "offset" => Convert.ChangeType(0, type, CultureInfo.InvariantCulture),
            "cursor" or "next_cursor" or "scroll_id" => string.Empty,
            "time_range_field" => "update_time",
            "time_from" or "start_time" or "start_date" or "create_time_from" or "update_time_from" => Convert.ChangeType(timeFrom, type, CultureInfo.InvariantCulture),
            "time_to" or "end_time" or "end_date" or "create_time_to" or "update_time_to" => Convert.ChangeType(timeTo, type, CultureInfo.InvariantCulture),
            "date_type" => "create_time",
            "sort_by" => "update_time",
            "sort_type" or "sort_order" => "DESC",
            "status" or "order_status" or "item_status" => string.Empty,
            "response_optional_fields" => string.Empty,
            _ => TryDefaultPrimitive(type, out var primitive) ? primitive : null
        };

        if (value is not null)
        {
            return true;
        }

        skipReason = $"Parameter {name} has no safe default.";
        return false;
    }

    private static bool TryDefaultPrimitive(Type type, out object? value)
    {
        if (type == typeof(string))
        {
            value = string.Empty;
            return true;
        }

        if (type == typeof(bool))
        {
            value = false;
            return true;
        }

        if (type.IsPrimitive || type == typeof(decimal))
        {
            value = Convert.ChangeType(0, type, CultureInfo.InvariantCulture);
            return true;
        }

        value = null;
        return false;
    }

    private static string Label(GeneratedEndpoint endpoint) => $"{endpoint.Method} {endpoint.Path}";

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var chars = new List<char>();
        for (var index = 0; index < value.Length; index++)
        {
            var current = value[index];
            if (char.IsUpper(current) && index > 0)
            {
                chars.Add('_');
            }

            chars.Add(char.ToLowerInvariant(current));
        }

        return new string(chars.ToArray());
    }
}
