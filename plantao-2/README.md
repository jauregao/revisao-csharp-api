# Parte 2: banco de dados e organização em camadas

### Usar EF Core e SQLite, aplicar migrations, separar Controller/Service/Repository e completar PUT/DELETE

[Voltar à apresentação do projeto](../README.md)

Vamos continuar juntas a API de produtos: salvar os dados em SQLite, organizar as responsabilidades em camadas e completar as operações de atualização e exclusão. Testaremos cada etapa no Swagger e conferiremos o banco.

## 1. Arquivos da API

O exemplo completo está em [base/back](../base/back/README.md). Os pacotes e a migration inicial já estão incluídos.

| Arquivo                                                                  | Conteúdo                                        |
| ------------------------------------------------------------------------ | ----------------------------------------------- |
| [Produto.cs](../base/back/Models/Produto.cs)                             | Model e validação da parte 1                    |
| [Program.cs](../base/back/Program.cs)                                    | Banco, dependências, CORS e tratamento de erros |
| [AppDbContext.cs](../base/back/Data/AppDbContext.cs)                     | Contexto e DbSet de produtos                    |
| [IProdutoRepository.cs](../base/back/Repositories/IProdutoRepository.cs) | Contrato do Repository                          |
| [ProdutoRepository.cs](../base/back/Repositories/ProdutoRepository.cs)   | Consultas e gravações                           |
| [IProdutoService.cs](../base/back/Services/IProdutoService.cs)           | Contrato do Service                             |
| [ProdutoService.cs](../base/back/Services/ProdutoService.cs)             | Operações sobre os produtos                     |
| [ProdutosController.cs](../base/back/Controllers/ProdutosController.cs)  | Endpoints do CRUD                               |
| [Migrations](../base/back/Migrations)                                    | Esquema inicial do banco                        |

O DbContext coordena consultas e gravações. `DbSet<Produto>` representa as entidades consultáveis, não um array já carregado. As interfaces definem os contratos entre as camadas.

## 2. Configure o banco, as dependências e o tratamento de erros

Arquivo: [Program.cs](../base/back/Program.cs).

```csharp
using Microsoft.EntityFrameworkCore;
using RevisaoProdutos.Data;
using RevisaoProdutos.Repositories;
using RevisaoProdutos.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PaginaLocal", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=revisao.db"));
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IProdutoService, ProdutoService>();

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("PaginaLocal");
app.MapControllers();
app.Run();
```

`AddScoped` informa qual implementação fornecer para cada interface e mantém uma instância por escopo de requisição. Os construtores recebem essas dependências. O DbContext também é scoped por padrão.

`AddProblemDetails` e `UseExceptionHandler` oferecem tratamento geral de exceções inesperadas com uma resposta estruturada. Para 500, confira o log da API. Isso é diferente de devolver 404 porque um produto não existe ou 400 porque a entrada é inválida.

Dentro de `base/back`, execute:

```powershell
dotnet build
dotnet ef database update
```

O build compila o código. A migration descreve o esquema. `database update` cria/aplica o esquema ao SQLite; não é o comando que gera a migration. Abra `base/back/revisao.db` no DBeaver e localize `Produtos`.

Os produtos da lista em memória não são transferidos automaticamente para o banco. Cadastre produtos novos para comparar a persistência.

### 2.1. CORS: chamadas de outra origem

Uma origem combina **protocolo, host e porta**. CORS permite que a API informe ao navegador quais origens podem acessar suas respostas.

O protocolo aparece no início da URL:

| Protocolo | Como a comunicação acontece                                                                                                                      |
| --------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| HTTP      | Envia requisições e respostas sem criptografia de transporte; um intermediário na rede pode ler ou alterar os dados                              |
| HTTPS     | Usa HTTP sobre TLS, que criptografa a comunicação, protege sua integridade e verifica a identidade do servidor por meio de um certificado válido |

Os métodos, como GET e POST, continuam os mesmos. A diferença está na proteção da conexão. Usamos HTTP neste exemplo local; em produção, usamos HTTPS para proteger os dados em trânsito.

Para o navegador, `http://localhost:5027` e `https://localhost:5027` são origens diferentes, mesmo com host e porta iguais. Trocar o texto da URL para HTTPS só funciona se o servidor estiver configurado para receber conexões HTTPS nessa porta.

| Endereços comparados                                                   | Mesma origem?              |
| ---------------------------------------------------------------------- | -------------------------- |
| `http://localhost:5027/swagger` e `http://localhost:5027/api/produtos` | Sim: só o caminho mudou    |
| `http://localhost:4200` e `http://localhost:5027`                      | Não: portas diferentes     |
| `https://localhost:5027` e `http://localhost:5027`                     | Não: protocolos diferentes |

Uma página em `http://localhost:4200` pode chamar nossa API, mas o navegador exige a permissão CORS para disponibilizar a resposta ao JavaScript. O Swagger desta API usa a mesma origem; por isso, funcionar nele não comprova que CORS está configurado.

A política abaixo está incluída no exemplo de `Program.cs` desta parte.

- `AddCors` registra a política; `WithOrigins` permite a origem indicada, sem caminho ou barra final.
- `AllowAnyHeader` e `AllowAnyMethod` permitem headers e métodos nessa política; não criam endpoints.
- `UseCors` aplica a política depois do roteamento.

O navegador envia `Origin`; para a origem permitida, a API responde com `Access-Control-Allow-Origin`. Antes de chamadas como POST com JSON, o navegador pode enviar **OPTIONS**, chamado preflight, para conferir a permissão.

O navegador aplica a política de CORS; Postman e chamadas entre servidores seguem seus próprios controles de acesso.

#### Para que serve CORS?

CORS controla o compartilhamento de respostas com JavaScript de outras origens. O navegador aplica essa regra com base nos headers enviados pela API.

No nosso projeto:

| Origem da página                | Resultado                                                       |
| ------------------------------- | --------------------------------------------------------------- |
| `http://localhost:4200`         | O Angular pode ler as respostas, pois a API permite essa origem |
| Outra origem sem permissão      | O navegador impede que o JavaScript leia as respostas           |
| `http://localhost:5027/swagger` | O Swagger está na mesma origem da API                           |

Esse controle evita compartilhar respostas com páginas indevidas. Por exemplo, em um sistema com sessão e dados privados, uma página maliciosa poderia tentar consultar a API usando o navegador da pessoa. A política de mesma origem restringe essa leitura; a configuração de CORS deve permitir apenas os compartilhamentos necessários. Cookies e credenciais têm regras próprias de envio.

O bloqueio da leitura não garante que a requisição deixou de chegar à API. Quando um preflight falha, o navegador não envia a operação seguinte. A autenticação e as permissões de acesso continuam sendo verificadas pelo servidor.

O [Angular de exemplo](../base/front/README.md) permite observar as chamadas e o preflight na aba Network.

Referências: [CORS no navegador](https://developer.mozilla.org/en-US/docs/Web/HTTP/Guides/CORS) e [configuração no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0).

### 2.2. Outros ataques e como proteger a aplicação

A segurança também envolve os dados recebidos, o acesso ao banco, as permissões e a comunicação. Mesmo com validação no Angular, as regras precisam existir na API.

| Ataque                   | Exemplo                                                                                                                     | Proteção                                                                                         |
| ------------------------ | --------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| SQL injection            | Um texto enviado no formulário altera uma consulta construída por concatenação                                              | Consultas parametrizadas e permissões mínimas no banco                                           |
| DoS e DDoS               | Muitas chamadas ou operações caras esgotam os recursos e deixam a API indisponível; no DDoS, o tráfego vem de várias fontes | Limitação de requisições, limites de tamanho e tempo, monitoramento e proteção na infraestrutura |
| Interceptação de tráfego | Um intermediário lê ou altera credenciais e dados durante a comunicação                                                     | HTTPS com TLS e validação dos certificados                                                       |
| Acesso indevido          | Uma pessoa troca o ID na URL e consegue editar um registro de outra usuária                                                 | Autenticação e autorização por recurso e operação                                                |
| CSRF                     | Um site induz o navegador de uma pessoa autenticada a enviar uma ação usando seus cookies                                   | Proteção antiforgery, cookies SameSite e verificação de origem, conforme a autenticação          |
| XSS                      | Um conteúdo inserido por um atacante executa JavaScript na página                                                           | Codificação de saída, sanitização de HTML quando necessária e CSP como proteção adicional        |

No exemplo:

- [ProdutoRepository.cs](../base/back/Repositories/ProdutoRepository.cs) usa LINQ e EF Core, cujas consultas comuns parametrizam os valores. SQL bruto concatenado com entradas exige cuidado. A validação do model verifica os campos; a parametrização mantém os valores separados do comando SQL.
- Usamos HTTP localmente. Em produção, a comunicação deve usar HTTPS para proteger os dados em trânsito.
- O CRUD está aberto, sem login. Um sistema com dados por usuária precisa verificar a permissão de cada operação.
- Rate limiting pode responder 429 ao exceder um limite. Ataques que saturam a rede também exigem mitigação no provedor ou na infraestrutura.

Mantenha dependências atualizadas, guarde segredos fora do código e evite expor credenciais ou detalhes internos nas respostas e nos logs.

Referências da OWASP: [SQL injection](https://cheatsheetseries.owasp.org/cheatsheets/SQL_Injection_Prevention_Cheat_Sheet.html), [negação de serviço](https://cheatsheetseries.owasp.org/cheatsheets/Denial_of_Service_Cheat_Sheet.html), [TLS](https://cheatsheetseries.owasp.org/cheatsheets/Transport_Layer_Security_Cheat_Sheet.html), [autorização](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html), [CSRF](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html) e [XSS](https://cheatsheetseries.owasp.org/cheatsheets/Cross_Site_Scripting_Prevention_Cheat_Sheet.html).

## 3. Complete o Repository

Arquivo: [ProdutoRepository.cs](../base/back/Repositories/ProdutoRepository.cs).

```csharp
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
```

`ToList` executa a consulta. `FirstOrDefault` encontra a entidade ou null. Add/Remove registram a intenção no contexto; **SaveChanges grava as mudanças**. O produto buscado é rastreado pelo EF: alterar suas propriedades e chamar Salvar persiste a atualização.

Na primeira versão, `Add` alterava a lista em memória imediatamente. Agora, o Repository concentra o acesso ao banco: o contexto acompanha as alterações, e `SaveChanges()` as grava no SQLite. Sem essa chamada, um cadastro, uma edição ou uma exclusão não será persistido.

## 4. Complete o Service

Arquivo: [ProdutoService.cs](../base/back/Services/ProdutoService.cs).

```csharp
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
```

Na criação, o SQLite gera o ID. Na edição, buscamos pelo ID da URL e preservamos esse identificador; o body fornece nome e preço. A ausência retorna null ou false ao Controller, que escolherá o status HTTP.

O Service trabalha com comportamentos; o Repository executa a persistência. As interfaces representam os contratos entre essas camadas.

## 5. Troque o Controller e complete o CRUD

Arquivo: [ProdutosController.cs](../base/back/Controllers/ProdutosController.cs). Esta versão usa o Service no lugar da lista em memória.

```csharp
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
```

Execute `dotnet run --launch-profile http` e repita GET/POST no Swagger. Depois edite um ID existente com PUT e remova com DELETE.

| Operação                                  | Resultado esperado                    |
| ----------------------------------------- | ------------------------------------- |
| POST válido                               | 201, com ID gerado                    |
| PUT válido, ID existente                  | 200, mesmo ID e dados alterados       |
| DELETE de ID existente                    | 204, sem corpo                        |
| Buscar, editar ou remover ID inexistente  | 404                                   |
| POST/PUT com nome vazio ou preço negativo | 400, sem persistir a entrada inválida |

Reinicie a API: os dados válidos continuam. No DBeaver, consulte `SELECT * FROM Produtos;` e compare com os resultados. Atualize a consulta depois de cada alteração.

## 6. Observe as respostas e investigue os erros

Vamos testar três situações no Swagger: produto válido, entrada inválida e ID inexistente. Observe o status e o corpo da resposta em cada caso.

- Um cadastro válido retorna 201 e o produto criado.
- Um nome vazio ou preço negativo retorna 400 com os detalhes da validação.
- Buscar, editar ou remover um ID ausente retorna 404.
- Uma exclusão bem-sucedida retorna 204, sem corpo para interpretar.

Depois de um PUT inválido, consulte novamente o produto e confira que os dados anteriores foram preservados. Depois de um DELETE válido, tente buscar o mesmo ID e explique o 404.

Uma exceção inesperada pode produzir 500. Para investigar, leia o log no terminal da API e localize a operação que falhou. Não confunda esse caso com um produto que simplesmente não existe.

O caminho completo de uma requisição passa pelas seguintes partes:

```text
Swagger → Controller → Service → Repository → DbContext → SQLite
```

O Controller recebe o JSON e escolhe o status da resposta. O Service organiza o cadastro, a atualização e a exclusão. O Repository consulta e modifica as entidades por meio do DbContext. A chamada `SaveChanges()` grava as alterações no SQLite.

Faça um commit pelo Source Control do VS Code e acrescente ao seu README os comandos necessários para executar a API. Assim, outra pessoa poderá reproduzir os testes.

Referências: [EF Core](https://learn.microsoft.com/pt-br/ef/core/), [DI](https://learn.microsoft.com/pt-br/dotnet/core/extensions/dependency-injection), [erros em APIs](https://learn.microsoft.com/pt-br/aspnet/core/web-api/handle-errors?view=aspnetcore-8.0).
