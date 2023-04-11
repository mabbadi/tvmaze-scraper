public interface IProductStorage {
    public Task<List<Product>> GetAllProducts(string keyword);
    public Task AddProduct(Product product);
}