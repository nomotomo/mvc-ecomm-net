using Discount.Grpc.Protos;
using Grpc.Core;

namespace Basket.Application.GrpcService;

public class DiscountGrpcService
{
    private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoServiceClient;

    public DiscountGrpcService(DiscountProtoService.DiscountProtoServiceClient discountProtoServiceClient)
    {
        _discountProtoServiceClient = discountProtoServiceClient;
    }
    
    public async Task<CouponModel> GetDiscount(string productName)
    {
        var request = new GetDiscountRequest { ProductName = productName };
        return await _discountProtoServiceClient.GetDiscountAsync(request);
    }
    
    public async Task<IList<CouponModel>> GetAllDiscounts()
    {
        var request = new Google.Protobuf.WellKnownTypes.Empty();
        using var call = _discountProtoServiceClient.GetAllDiscounts(request);
        
        var coupons = new List<CouponModel>();
        await foreach (var couponModel in call.ResponseStream.ReadAllAsync())
        {
            coupons.Add(couponModel);
        }
        return coupons;
    }
}