using Catalog.API.Controllers;
using Catalog.Application.Commands;
using Catalog.Application.Queries;
using Catalog.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controller;

public class CatalogController(IMediator mediator) : ApiController
{
    [HttpGet]
    [Route("[action]/{id}", Name = "GetProductById")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProductById(string id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet]
    [Route("[action]/{productName}", Name = "GetProductsByProductName")]
    [ProducesResponseType(typeof(IList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> GetProductByName(string productName)
    {
        var query = new GetProductByNameQuery(productName);
        var result = await mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet]
    [Route("GetAllProducts")]
    [ProducesResponseType(typeof(IList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> GetAllProducts()
    {
        var query = new GetAllProductQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet]
    [Route("GetAllBrands")]
    [ProducesResponseType(typeof(IList<BrandResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<BrandResponse>> GetAllBrands()
    {
        var query = new GetAllBrandsQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet]
    [Route("GetAllTypes")]
    [ProducesResponseType(typeof(IList<TypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<TypeResponse>> GetAllTypes()
    {
        var query = new GetAllTypesQuery();
        var result = await mediator.Send(query);
        return Ok(result);
    }
    
    [HttpGet]
    [Route("[action]/{brandName}", Name = "GetProductsByBrandName")]
    [ProducesResponseType(typeof(IList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> GetProductByBrandName(string brandName)
    {
        var query = new GetProductByBrandQuery(brandName);
        var result = await mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Route("CreateProduct")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductCommand command)
    {
        var result = await mediator.Send<ProductResponse>(command);
        return Ok(result);
    }
    
    [HttpPut]
    [Route("UpdateProduct")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct([FromBody] UpdateProductCommand command)
    {
        var result = await mediator.Send<ProductResponse>(command);
        return Ok(result);
    }
    
    [HttpDelete]
    [Route("{id}", Name = "DeleteProduct")]
    [ProducesResponseType(typeof(Boolean), StatusCodes.Status200OK)]
    public async Task<ActionResult<Boolean>> DeleteProduct(string Id)
    {
        var query = new DeleteProductByIdCommand(Id);
        var result = await mediator.Send(query);
        return Ok(result);
    }
    
}

