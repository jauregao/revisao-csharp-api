using System.ComponentModel.DataAnnotations;

namespace RevisaoProdutos.Models;

public class Produto
{
  [Key]
  public int Id { get; set; }

  [Required(ErrorMessage = "O campo Nome é obrigatório.")]
  [StringLength(100, ErrorMessage = "O campo Nome deve ter no máximo 100 caracteres.")]
  public string Nome { get; set; } = string.Empty;

  [Required(ErrorMessage = "O campo Preço é obrigatório.")]
  [Range(0.01, double.MaxValue, ErrorMessage = "O campo Preço deve ser maior que zero.")]
  public decimal Preco { get; set; }
}
