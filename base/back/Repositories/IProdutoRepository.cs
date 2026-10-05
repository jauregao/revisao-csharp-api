using RevisaoProdutos.Models;

namespace RevisaoProdutos.Repositories;

public interface IProdutoRepository
{
    List<Produto> Listar();
    Produto? BuscarPorId(int id);
    Produto Adicionar(Produto produto);
    void Salvar();
    void Remover(Produto produto);
}
