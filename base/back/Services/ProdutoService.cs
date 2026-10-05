using RevisaoProdutos.Models;
using RevisaoProdutos.Repositories;

namespace RevisaoProdutos.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;
    public ProdutoService(IProdutoRepository repository) { _repository = repository; }

    public List<Produto> Listar()
    {
        return _repository.Listar();
    }

    public Produto? BuscarPorId(int id)
    {
        return _repository.BuscarPorId(id);
    }

    public Produto Cadastrar(Produto produto)
    {
        produto.Id = 0;
        produto.Nome = produto.Nome.Trim();
        return _repository.Adicionar(produto);
    }

    public Produto? Atualizar(int id, Produto dados)
    {
        var produto = _repository.BuscarPorId(id);
        if (produto is null) return null;

        produto.Nome = dados.Nome.Trim();
        produto.Preco = dados.Preco;
        _repository.Salvar();
        return produto;
    }

    public bool Remover(int id)
    {
        var produto = _repository.BuscarPorId(id);
        if (produto is null) return false;

        _repository.Remover(produto);
        return true;
    }
}
