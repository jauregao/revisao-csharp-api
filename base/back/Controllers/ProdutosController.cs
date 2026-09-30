using Microsoft.AspNetCore.Mvc;
using RevisaoProdutos.Models;

namespace RevisaoProdutos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
  private readonly List<Produto> _produtos;

  public ProdutosController()
  {
    _produtos = new List<Produto>
    {
      new Produto { Id = 1, Nome = "Mouse Gamer", Preco = 89.90m },
      new Produto { Id = 2, Nome = "Mouse Sem Fio", Preco = 59.90m },
      new Produto { Id = 3, Nome = "Teclado Gamer", Preco = 149.90m },
      new Produto { Id = 4, Nome = "Teclado Sem Fio", Preco = 119.90m },
      new Produto { Id = 5, Nome = "Headset Gamer", Preco = 199.90m }
    };
  }

  [HttpGet]
  public ActionResult<IEnumerable<Produto>> GetProdutos([FromQuery] string? nome = null)
  {
    if (string.IsNullOrWhiteSpace(nome))
    {
      return Ok(_produtos);
    }

    var produtosFiltrados = _produtos.Where(p =>
      p.Nome.Contains(nome.Trim(), StringComparison.OrdinalIgnoreCase));

    return Ok(produtosFiltrados);
  }

  [HttpGet("{id}")]
  public ActionResult<Produto> GetProduto(int id)
  {
    var produto = _produtos.FirstOrDefault(p => p.Id == id);
    if (produto == null)
    {
      return NotFound();
    }
    return Ok(produto);
  }
}
