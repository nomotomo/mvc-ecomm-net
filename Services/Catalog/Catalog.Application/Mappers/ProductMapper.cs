using AutoMapper;
using Microsoft.Extensions.Logging;

// namespace Catalog.Application.Mappers;
//
// public static class ProductMapper
// {
//     private static readonly Lazy<IMapper> Lazy = new(() =>
//     {
//         var config = new MapperConfiguration(cfg =>
//         {
//             cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
//             cfg.AddProfile<ProductMappingProfiles>();
//         }, null);
//         var mapper = config.CreateMapper();
//         return config.CreateMapper();
//     });
//
//     public static IMapper Mapper => Lazy.Value;
// }