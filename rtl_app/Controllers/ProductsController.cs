using Microsoft.AspNetCore.Mvc;
namespace rtl_app.Controllers;
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductStorage storage;
    public ProductsController(IProductStorage storage)
    {
        this.storage = storage;
    }

    [HttpGet("test", Name = "test")]
    public async Task<IActionResult> Test(){
        return Ok("ok");
    }

    [HttpGet(Name = "GetAllProducts")]
    public async Task<IActionResult> Get([FromQuery]string? keyword)
    {
        if(String.IsNullOrEmpty(keyword)) keyword = "";
       var result = await storage.GetAllProducts(keyword);
       return Ok(result);
    }

    [HttpPost(Name = "AddProduct")]
    public async Task<IActionResult> Post([FromBody]Product product)
    {
        await storage.AddProduct(product);
       return Ok();
    }
}