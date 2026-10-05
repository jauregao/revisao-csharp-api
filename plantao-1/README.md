# Parte 1: C#, HTTP e produtos em memória

### Ler o projeto, modelar produtos, validar os campos, cadastrar e consultar com GET/POST

Vamos construir os primeiros endpoints da nossa API, retomando C#, HTTP e modelagem. O exemplo completo está em [base/plantao-1](../base/plantao-1/README.md). Vamos criar o model Produto, suas validações e os endpoints de listagem, busca por ID e cadastro.

## Arquivos do exemplo

| Arquivo                                                                      | Conteúdo                               |
| ---------------------------------------------------------------------------- | -------------------------------------- |
| [Produto.cs](../base/plantao-1/Models/Produto.cs)                            | Model e validação                      |
| [ProdutosController.cs](../base/plantao-1/Controllers/ProdutosController.cs) | Lista em memória, consultas e cadastro |
| [Program.cs](../base/plantao-1/Program.cs)                                   | Configuração da API                    |

## 1. Base da API e HTTP

Pasta: `base/back`

```powershell
dotnet restore
dotnet build
dotnet run --launch-profile http
```

Abra `http://localhost:5026/swagger`. O exemplo já inclui listagem, busca por ID e cadastro.

O Swagger envia requisições para nossa API e mostra as respostas. Usaremos HTTP para essa comunicação e JSON para representar os dados.

Arquivos iniciais:

- [Program.cs](../base/back/Program.cs): configura e inicia a API.
- [RevisaoProdutos.csproj](../base/back/RevisaoProdutos.csproj): define a versão do .NET e os pacotes.
- [appsettings.json](../base/back/appsettings.json): guarda configurações, como os logs.

Em `Program.cs`, `AddControllers()` registra os Controllers, `MapControllers()` conecta suas rotas e `Run()` inicia o servidor. `AddSwaggerGen()`, `UseSwagger()` e `UseSwaggerUI()` preparam e disponibilizam o Swagger.

`restore` prepara os pacotes; `build` compila o código; `run` inicia a API. `Ctrl+C` encerra a aplicação.

### 1.1. O que vai em uma requisição HTTP?

Uma requisição solicita uma operação à API. Ela contém método, endereço, headers e, quando necessário, body.

Exemplo simplificado do cadastro em HTTP/1.1:

```http
POST /api/produtos HTTP/1.1
Host: localhost:5027
Content-Type: application/json
Accept: application/json

{"nome":"Caderno","preco":25.9}
```

| Parte   | Neste exemplo                                                           |
| ------- | ----------------------------------------------------------------------- |
| Método  | `POST`: cadastrar um produto                                            |
| Caminho | `/api/produtos`: recurso acessado                                       |
| Headers | `Content-Type`: formato enviado; `Accept`: formato desejado na resposta |
| Body    | JSON com nome e preço                                                   |

A resposta tem status, headers e, quando houver conteúdo, body. Um cadastro válido retorna 201, o produto em JSON e o header `Location` com seu endereço de consulta. Já nosso GET envia uma requisição sem body e recebe uma lista na resposta.

### 1.2. URL e as partes do endereço

URL é o endereço usado para acessar um recurso:

```text
http://localhost:5027/api/produtos?nome=caderno
```

| Parte           | Exemplo         | Significado                                   |
| --------------- | --------------- | --------------------------------------------- |
| Protocolo       | `http`          | Como acessamos a API; HTTPS protege a conexão |
| Host ou domínio | `localhost`     | Servidor acessado; aqui, o próprio computador |
| Porta           | `5027`          | Porta em que a API está executando            |
| Caminho         | `/api/produtos` | Recurso acessado                              |
| Query           | `?nome=caderno` | Parâmetro usado para filtrar a consulta       |

Em uma API publicada, o host pode ser um domínio, como `api.exemplo.com`. `localhost` é um nome reservado para o próprio computador.

Uma rota define o caminho aceito pela aplicação. O endpoint combina método e rota: `GET /api/produtos` consulta, enquanto `POST /api/produtos` cadastra no mesmo endereço.

### 1.3. Métodos e status que vamos encontrar

| Método | Para que serve                       | Exemplo                                            |
| ------ | ------------------------------------ | -------------------------------------------------- |
| GET    | Consultar dados                      | `GET /api/produtos/1`                              |
| POST   | Criar um recurso                     | `POST /api/produtos` com nome e preço no body      |
| PUT    | Atualizar a representação do recurso | `PUT /api/produtos/1` com nome e preço no body     |
| PATCH  | Atualizar parte do recurso           | Alterar apenas o preço; não será implementado aqui |
| DELETE | Remover um recurso                   | `DELETE /api/produtos/1`                           |

Nesta parte, implementaremos GET e POST. PUT e DELETE ficam para a Parte 2.

O status informa o resultado: 2xx indica sucesso, 4xx indica erro na requisição e 5xx indica erro no servidor.

| Status                     | Exemplo nesta API                                                 |
| -------------------------- | ----------------------------------------------------------------- |
| 200 OK                     | Consulta concluída, inclusive uma lista vazia                     |
| 201 Created                | Produto cadastrado                                                |
| 204 No Content             | Exclusão concluída, sem corpo de resposta, na Parte 2             |
| 400 Bad Request            | Nome vazio, preço inválido ou dados que não podem ser convertidos |
| 404 Not Found              | Produto ou rota não encontrado                                    |
| 405 Method Not Allowed     | Método não disponível para uma rota existente                     |
| 415 Unsupported Media Type | Formato do body não aceito pelo endpoint                          |
| 500 Internal Server Error  | Falha inesperada na aplicação                                     |

### 1.4. Onde enviamos cada dado?

Os dados podem chegar em lugares diferentes da requisição:

| Origem            | Exemplo                           | Uso neste projeto                |
| ----------------- | --------------------------------- | -------------------------------- |
| Parâmetro de rota | `/api/produtos/1`                 | Identificar o produto consultado |
| Query string      | `/api/produtos?nome=caderno`      | Filtrar a coleção por nome       |
| Body              | `{"nome":"Caderno","preco":25.9}` | Enviar os dados do cadastro      |

- A query começa com `?`. Mais parâmetros são separados por `&`: `?nome=caderno&pagina=2`.
- A API precisa implementar cada parâmetro. Aqui, teremos apenas o filtro `nome`.
- `?id=1` e `/api/produtos/1` são entradas diferentes: query e parâmetro de rota.
- Os dados enviados na query não preenchem automaticamente o body do cadastro.

## 2. Crie o model e suas validações

Arquivo: `base/back/Models/Produto.cs`

Essa classe define os dados de um produto:

```csharp
using System.ComponentModel.DataAnnotations;

namespace RevisaoProdutos.Models;

public class Produto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Range(0.01, 10000)]
    public decimal Preco { get; set; }
}
```

O que cada trecho faz:

- `class Produto` define os dados de cada produto. `public` permite o acesso por outras partes da aplicação.
- `int`, `string` e `decimal` representam ID inteiro, nome e preço. `decimal` é adequado para valores monetários.
- `{ get; set; }` permite ler e atribuir valores. `string.Empty` inicia o nome vazio.
- `namespace` organiza a classe; `using` disponibiliza os atributos de validação.
- `[Required]` exige nome preenchido; `[MaxLength(100)]` limita o nome; `[Range(0.01, 10000)]` limita o preço.

Esses atributos são as Data Annotations. Com `[ApiController]`, a API responde 400 quando os dados são inválidos. O `ModelState` reúne os resultados da conversão e da validação; nesse caso, o framework o verifica automaticamente.

## 3. Controller e endpoints

Arquivo: `base/back/Controllers/ProdutosController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using RevisaoProdutos.Models;

namespace RevisaoProdutos.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private static readonly List<Produto> Produtos = new();
    private static int _proximoId = 1;

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
```

### 3.1. Estrutura do Controller

- `using RevisaoProdutos.Models` permite usar `Produto`.
- `ControllerBase` fornece respostas como `Ok()` e `NotFound()`.
- `[Route("api/produtos")]` define o caminho; os atributos `[HttpGet]` e `[HttpPost]` definem os métodos HTTP.
- `ActionResult<T>` permite retornar dados do tipo `T` ou outro resultado HTTP.
- `List<Produto>` guarda os produtos; `new()` cria a lista vazia.
- `private` restringe o acesso ao campo à classe. `readonly` impede trocar a referência da lista, mas permite adicionar itens.
- `static` compartilha a lista e o contador entre as instâncias do Controller. Os dados ficam na memória e somem quando a API encerra.

### 3.2. Listagem e filtro por query

`GET /api/produtos` retorna todos os produtos. `GET /api/produtos?nome=caderno` filtra pelo nome.

- `[FromQuery]` lê `nome` da query string. `string?` permite `null` quando o parâmetro não é enviado.
- `IsNullOrWhiteSpace` identifica nome ausente, vazio ou só com espaços. Nesses casos, a resposta inclui a lista completa.
- `Where` seleciona os produtos; `Contains` procura o texto no nome; `OrdinalIgnoreCase` ignora maiúsculas e minúsculas.
- `ToList()` reúne os resultados sem alterar os cadastros.
- `Ok()` retorna 200, e o ASP.NET Core converte a lista para JSON. Sem resultados, a resposta é 200 com `[]`.

### 3.3. Busca por parâmetro de rota

`GET /api/produtos/1` consulta o produto de ID 1.

- `[FromRoute]` lê o ID do caminho. O framework também reconhece essa origem quando o nome corresponde ao parâmetro da rota.
- `{id:int}` exige um inteiro. `/api/produtos/abc` retorna 404 porque não corresponde à rota.
- `FirstOrDefault` retorna o primeiro produto encontrado ou `null`. A lambda `item => item.Id == id` compara os IDs.
- `var` faz o compilador deduzir o tipo da variável; ela continua tendo um tipo definido.
- `NotFound()` retorna 404 quando o produto não existe; `Ok(produto)` retorna 200 com seus dados.

### 3.4. Cadastro pelo body

`POST /api/produtos` recebe nome e preço em JSON.

- `[FromBody]` converte o JSON em um objeto `Produto`. Com `[ApiController]`, dados inválidos recebem 400 antes de chamar `Cadastrar()`.
- `_proximoId++` usa o ID atual e aumenta o contador. A API define o ID do produto.
- `Trim()` remove espaços no início e no fim do nome; `Add()` guarda o produto na lista.
- `CreatedAtAction` retorna 201, o produto criado e seu endereço no header `Location`.
- `nameof(BuscarPorId)` fornece o nome do método de consulta; `new { id = produto.Id }` cria um objeto anônimo com o ID usado na URL.

Nesta versão, o Controller recebe a requisição, manipula a lista e escolhe a resposta. Na Parte 2, essas tarefas serão separadas em camadas.

## 4. Exemplos no Swagger

Body de um cadastro válido (`POST /api/produtos`):

```json
{
  "nome": "Caderno",
  "preco": 25.9
}
```

JSON usa ponto nas casas decimais. Não é necessário enviar ID.

No Swagger:

- `Request URL` mostra o endereço completo da requisição.
- Na busca por ID, preencha `id`; na listagem, preencha `nome` ou deixe-o vazio.
- No POST, envie o JSON em `Request body`.
- Confira o status, `Response headers` e `Response body`. No cadastro válido, `Location` aponta para a consulta do produto criado.

| Teste                                         | Resultado esperado                    |
| --------------------------------------------- | ------------------------------------- |
| GET da lista antes do cadastro                | 200 e lista vazia                     |
| POST válido                                   | 201, produto com ID e header Location |
| GET pelo ID retornado                         | 200 e produto cadastrado              |
| GET com ID inexistente, por exemplo 9999      | 404                                   |
| POST com nome vazio                           | 400                                   |
| POST com preço -5                             | 400                                   |
| GET após os dois cadastros inválidos          | Apenas os produtos válidos            |
| GET com `nome=caderno` após cadastrar Caderno | 200 e os produtos correspondentes     |
| GET com `nome=xyz-inexistente`                | 200 e lista vazia                     |

O corpo da resposta 400 informa quais campos falharam. Consulte a lista para confirmar que o produto inválido não foi cadastrado. Essa validação usa os atributos do model e `[ApiController]`.

Encerre e reinicie a API: a lista fica vazia porque seus dados estavam na memória do processo. Na Parte 2, compararemos esse comportamento com o SQLite.

## 5. O caminho de uma requisição e o registro das alterações

O cadastro segue este caminho:

```text
JSON → objeto Produto → validação → método Cadastrar → lista → resposta 201
```

Se os dados forem inválidos, a API responde 400 antes de chamar `Cadastrar`. Na busca por ID, retorna 200 quando encontra o produto e 404 quando a busca resulta em `null`.

Revise as alterações pelo Source Control do VS Code e faça um commit. O `.gitignore` exclui arquivos gerados, dependências e bancos.

Se a pasta já estiver em um repositório, use-o. Se estiver trabalhando com uma cópia independente, inicialize um repositório pelo VS Code. Ao concluir, você pode publicar o projeto no seu GitHub.

## Para praticar: requisições, parâmetros e validações

Cadastre dois produtos válidos com nomes diferentes. Prepare uma tabela com método, URL, body enviado quando houver, status recebido e explicação para cada caso:

1. Nome vazio.
2. Nome com mais de 100 caracteres.
3. Preço zero.
4. Preço acima de 10.000.
5. Busca de ID inexistente.
6. Listagem sem filtro e com filtro por um dos nomes cadastrados.
7. Filtro que não encontra nenhum produto.

Em uma busca por ID, identifique o dado enviado na rota. Em uma listagem filtrada, identifique o dado enviado na query. Em um cadastro válido, identifique os headers, o body e o `Location` retornado. Explique por que o filtro sem resultados recebe 200, enquanto a busca de um ID inexistente recebe 404.

Confirme que nenhum cadastro inválido aparece na lista. Depois, reinicie a API e explique por que os produtos sumiram. Você pode usar um arquivo Markdown para registrar os resultados.

Se uma validação estiver faltando, ajuste o model e faça um novo teste. Apresente um exemplo válido, um inválido e o trecho responsável pela resposta. Traga suas dúvidas para o segundo encontro.

Referências: [controllers e respostas](https://learn.microsoft.com/pt-br/aspnet/core/web-api/?view=aspnetcore-8.0) e [validação de modelos](https://learn.microsoft.com/pt-br/aspnet/core/mvc/models/validation?view=aspnetcore-8.0).
