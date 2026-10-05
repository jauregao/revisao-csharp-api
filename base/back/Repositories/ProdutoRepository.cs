using RevisaoProdutos.Data;
using RevisaoProdutos.Models;

namespace RevisaoProdutos.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _context;
    public ProdutoRepository(AppDbContext context) { _context = context; }

    public List<Produto> Listar()
    {
        return _context.Produtos.ToList();
    }

    public Produto? BuscarPorId(int id)
    {
        return _context.Produtos.FirstOrDefault(produto => produto.Id == id);
    }

    public Produto Adicionar(Produto produto)
    {
        _context.Produtos.Add(produto);
        _context.SaveChanges();
        return produto;
    }

    public void Salvar()
    {
        _context.SaveChanges();
    }

    public void Remover(Produto produto)
    {
        _context.Produtos.Remove(produto);
        _context.SaveChanges();
    }
}
