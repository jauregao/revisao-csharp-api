using Microsoft.AspNetCore.Mvc;
using RevisaoProdutos.Models;
using RevisaoProdutos.Services;

namespace RevisaoProdutos.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;
    public ProdutosController(IProdutoService service) { _service = service; }

    [HttpGet]
    public ActionResult<List<Produto>> Listar()
    {
        return Ok(_service.Listar());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Produto> BuscarPorId(int id)
    {
        var produto = _service.BuscarPorId(id);
        if (produto is null) return NotFound();
        return Ok(produto);
    }

    [HttpPost]
    public ActionResult<Produto> Cadastrar([FromBody] Produto produto)
    {
        var criado = _service.Cadastrar(produto);
        return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Produto> Atualizar(int id, [FromBody] Produto dados)
    {
        var atualizado = _service.Atualizar(id, dados);
        if (atualizado is null) return NotFound();
        return Ok(atualizado);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Remover(int id)
    {
        if (!_service.Remover(id)) return NotFound();
        return NoContent();
    }
}
