using RevisaoProdutos.Models;

namespace RevisaoProdutos.Services;

public interface IProdutoService
{
    List<Produto> Listar();
    Produto? BuscarPorId(int id);
    Produto Cadastrar(Produto produto);
    Produto? Atualizar(int id, Produto dados);
    bool Remover(int id);
}
