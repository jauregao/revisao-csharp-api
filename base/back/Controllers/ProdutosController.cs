using Microsoft.AspNetCore.Mvc;
using RevisaoProdutos.Models;

namespace RevisaoProdutos.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    

    private static List<Produto> Produtos = new List<Produto>
{
    new Produto { Id = 1, Nome = "Notebook", Preco = 3500.00m },
    new Produto { Id = 2, Nome = "Smartphone", Preco = 1500.00m },
    new Produto { Id = 3, Nome = "Tablet", Preco = 1200.00m }
};
    private static int _proximoId = 4;



    [HttpGet]
    public ActionResult<List<Produto>> Listar([FromQuery] string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return Ok(Produtos);
        }

        var filtrados = Produtos
            .Where(produto => produto.Nome.Contains(
                nome.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Ok(filtrados);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Produto> BuscarPorId([FromRoute] int id)
    {
        var produto = Produtos.FirstOrDefault(item => item.Id == id);
        if (produto is null)
        {
            return NotFound();
        }
        return Ok(produto);
    }

    [HttpPost]
    public ActionResult<Produto> Cadastrar([FromBody] Produto produto)
    {
        produto.Id = _proximoId++;
        produto.Nome = produto.Nome.Trim();
        Produtos.Add(produto);

        return CreatedAtAction(
            nameof(BuscarPorId), new { id = produto.Id }, produto);
    }
}