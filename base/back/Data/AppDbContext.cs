using Microsoft.EntityFrameworkCore;
using RevisaoProdutos.Models;

namespace RevisaoProdutos.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos { get; set; }
}
