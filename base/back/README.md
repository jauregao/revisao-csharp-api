# API completa: parte 2

CRUD de produtos com validação, SQLite, Entity Framework Core, camadas e CORS para `http://localhost:4200`.

## Arquivos

| Arquivo                                                     | Responsabilidade                                                 |
| ----------------------------------------------------------- | ---------------------------------------------------------------- |
| [Program.cs](Program.cs)                                    | Inicialização, dependências, Swagger, CORS e tratamento de erros |
| [Produto.cs](Models/Produto.cs)                             | Dados e validações do produto                                    |
| [ProdutosController.cs](Controllers/ProdutosController.cs)  | Endpoints GET, POST, PUT e DELETE                                |
| [IProdutoService.cs](Services/IProdutoService.cs)           | Contrato do Service                                              |
| [ProdutoService.cs](Services/ProdutoService.cs)             | Cadastro, atualização e exclusão                                 |
| [IProdutoRepository.cs](Repositories/IProdutoRepository.cs) | Contrato do Repository                                           |
| [ProdutoRepository.cs](Repositories/ProdutoRepository.cs)   | Consultas e gravações no banco                                   |
| [AppDbContext.cs](Data/AppDbContext.cs)                     | Contexto do Entity Framework Core                                |
| [Migrations](Migrations)                                    | Criação da tabela de produtos                                    |
| [RevisaoProdutos.csproj](RevisaoProdutos.csproj)            | Versão do .NET e pacotes                                         |

## Executar

Na pasta `base/back`, com .NET SDK 8 e `dotnet-ef` 8.0.20:

```powershell
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

Se necessário, instale a ferramenta com `dotnet tool install --global dotnet-ef --version 8.0.20`.

Swagger: `http://localhost:5027/swagger`. A atualização cria `revisao.db` usando a migration incluída. Execute os comandos nesta pasta para usar o mesmo arquivo SQLite.

[Conteúdo da parte 2](../../plantao-2/README.md) · [Versão em memória da parte 1](../plantao-1/README.md)

[Angular conectado a esta API](../front/README.md)
