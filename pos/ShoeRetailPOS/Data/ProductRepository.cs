using MySql.Data.MySqlClient;
using ShoeRetailPOS.Models;
using System.Collections.Generic;

namespace ShoeRetailPOS.Data
{
    public class ProductRepository
    {
        public List<Product> GetActiveProducts()
        {
            var products = new List<Product>();

            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            string query = @"
                SELECT
                    product_id,
                    name,
                    description,
                    main_category,
                    price,
                    color,
                    images,
                    isActive
                FROM products
                WHERE isActive = 'active'
            ";

            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                products.Add(new Product
                {
                    ProductId = reader.GetInt32("product_id"),
                    Name = reader.GetString("name"),
                    Description = reader["description"]?.ToString(),
                    MainCategory = reader.GetString("main_category"),
                    Price = reader.GetDecimal("price"),
                    Color = reader["color"]?.ToString(),
                    Images = reader["images"]?.ToString(),
                    IsActive = reader.GetString("isActive")
                });
            }

            return products;
        }

        public (Product product, ProductSize size)? GetProductBySku(string sku)
        {
            using var conn = new MySqlConnection(DbConfig.ConnectionString);
            conn.Open();

            string query = @"
        SELECT 
            p.product_id, p.name, p.description, p.main_category, 
            p.price, p.color, p.images, p.isActive,
            ps.size_id, ps.size_value, ps.stock, ps.sku
        FROM product_sizes ps
        JOIN products p ON p.product_id = ps.product_id
        WHERE ps.sku = @sku
        LIMIT 1
    ";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@sku", sku);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            var product = new Product
            {
                ProductId = reader.GetInt32("product_id"),
                Name = reader.GetString("name"),
                Description = reader["description"]?.ToString(),
                MainCategory = reader.GetString("main_category"),
                Price = reader.GetDecimal("price"),
                Color = reader["color"]?.ToString(),
                Images = reader["images"]?.ToString(),
                IsActive = reader.GetString("isActive")
            };

            var size = new ProductSize
            {
                SizeId = reader.GetInt32("size_id"),
                SizeValue = reader.GetString("size_value"),
                Stock = reader.GetInt32("stock"),
                Sku = reader.GetString("sku"),
                ProductId = product.ProductId
            };

            return (product, size);
        }

        public int GetAvailableStock(int productId, int sizeId)
        {
            using var con = new MySqlConnection(DbConfig.ConnectionString);

            con.Open();

            string sql = @"SELECT stock 
                   FROM product_sizes 
                   WHERE product_id = @productId AND size_id = @sizeId";

            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@productId", productId);
            cmd.Parameters.AddWithValue("@sizeId", sizeId);

            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }


    }
}
