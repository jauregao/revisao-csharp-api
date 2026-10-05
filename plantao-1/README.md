# Parte 1: C#, HTTP e produtos em memória

### Ler o projeto, modelar produtos, validar os campos, cadastrar e consultar com GET/POST

Vamos construir os primeiros endpoints da nossa API, retomando C#, HTTP e modelagem. O exemplo completo está em [base/plantao-1](../base/plantao-1/README.md). Vamos criar o model Produto, suas validações e os endpoints de listagem, busca por ID e cadastro.

## Arquivos do exemplo

| Arquivo                                                                      | Conteúdo                               |
| ---------------------------------------------------------------------------- | -------------------------------------- |
| [Produto.cs](../base/plantao-1/Models/Produto.cs)                            | Model e validação                      |
| [ProdutosController.cs](../base/plantao-1/Controllers/ProdutosController.cs) | Lista em memória, consultas e cadastro |
| [Program.cs](../base/plantao-1/Program.cs)                                   | Configuração da API                    |

## 1. Abra a base e observe a resposta

No terminal em `base/plantao-1`, execute:

```powershell
dotnet restore
dotnet build
dotnet run --launch-profile http
```

Abra `http://localhost:5026/swagger`. O exemplo já inclui listagem, busca por ID e cadastro.

O Swagger faz o papel de cliente; a API é o servidor. HTTP define a troca de requisições e respostas; JSON é o formato dos dados. Na organização REST, `/api/produtos` identifica o recurso e os métodos indicam operações: GET consulta, POST cria, PUT atualiza e DELETE remove. Nesta parte, implementaremos GET e POST; a Parte 2 completa o CRUD.

Os arquivos iniciais têm responsabilidades diferentes: [Program.cs](../base/plantao-1/Program.cs) configura a aplicação; [RevisaoProdutos.csproj](../base/plantao-1/RevisaoProdutos.csproj) declara a versão do .NET e os pacotes usados; [appsettings.json](../base/plantao-1/appsettings.json) contém configurações, como o nível de detalhe dos logs.

Em `Program.cs`, `AddControllers()` registra os recursos usados pelos Controllers; `AddSwaggerGen()` prepara a descrição da API; `UseSwagger()` e `UseSwaggerUI()` disponibilizam essa descrição e sua interface no ambiente de desenvolvimento. `MapControllers()` conecta as rotas dos Controllers à aplicação e `Run()` inicia o servidor.

Pare a API com `Ctrl+C` antes de editar. Após cada etapa, use `dotnet build` para conferir a compilação e `dotnet run --launch-profile http` para testar a versão atualizada.

## 2. Crie o model e suas validações

Dentro de `base/plantao-1`, crie a pasta `Models`. Dentro dela, crie o arquivo `Produto.cs` com o código abaixo. O model representa os dados que receberemos e devolveremos na API:

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

`class Produto` define a estrutura de um produto. Cada objeto dessa classe terá suas próprias propriedades `Id`, `Nome` e `Preco`. `public` permite que outras partes da aplicação acessem a classe e suas propriedades.

Em C#, cada propriedade tem um tipo: `int` guarda o identificador inteiro, `string` guarda o nome e `decimal` guarda o preço. `decimal` é adequado para valores monetários porque representa números decimais com precisão apropriada para esses cálculos. `{ get; set; }` permite ler e atribuir o valor da propriedade. `= string.Empty` inicia o nome com uma string vazia.

`namespace RevisaoProdutos.Models` organiza a classe dentro de um grupo de tipos. `using System.ComponentModel.DataAnnotations` permite usar os atributos de validação desse namespace sem escrever seus nomes completos.

Os trechos entre colchetes são atributos: acrescentam informações que o framework pode interpretar. `[Required]` exige um nome preenchido; `[MaxLength(100)]` limita seu tamanho; `[Range(0.01, 10000)]` define o intervalo permitido para o preço. Declarar `string` define o tipo do nome, mas não garante, sozinho, que ele contenha texto. Essas regras são as Data Annotations vistas na modelagem.

Na próxima etapa, o atributo `[ApiController]` ativará a resposta automática 400 para entradas que descumprirem essas regras. O `ModelState` reúne os resultados da conversão dos dados recebidos e da validação; nesse comportamento padrão, não precisamos verificá-lo manualmente em cada ação.

## 3. Crie o Controller e implemente os endpoints

### 3.1. Crie a listagem de produtos

Dentro de `base/plantao-1`, crie a pasta `Controllers` e, dentro dela, o arquivo `ProdutosController.cs`. Comece com:

```csharp
using Microsoft.AspNetCore.Mvc;
using RevisaoProdutos.Models;

namespace RevisaoProdutos.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private static readonly List<Produto> Produtos = new();

    [HttpGet]
    public ActionResult<List<Produto>> Listar()
    {
        return Ok(Produtos);
    }
}
```

`using RevisaoProdutos.Models` permite usar a classe que criamos. O Controller herda de `ControllerBase`, que oferece respostas como `Ok()` e `NotFound()`. `[Route("api/produtos")]` define o endereço do recurso e `[HttpGet]` associa `Listar()` às requisições GET nesse endereço.

`List<Produto>` é uma lista tipada: seus elementos são objetos `Produto`. `new()` cria uma lista vazia do tipo declarado. `private` restringe o acesso ao campo à própria classe; `readonly` impede trocar a referência da lista, mas permite adicionar produtos.

`static` faz a lista ser compartilhada entre as instâncias do Controller. Assim, um produto cadastrado pode ser consultado em outra requisição. Os dados continuam na memória do processo: encerrar a API apaga a lista. Na Parte 2, usaremos o banco para persistir os produtos.

`Listar()` é um método público. Seu retorno `ActionResult<List<Produto>>` permite devolver uma lista de produtos ou uma resposta HTTP, como um erro. `Ok(Produtos)` devolve status 200, e o ASP.NET Core serializa a lista para JSON.

Uma lista vazia aparece como `[]`: a consulta funcionou, apenas não há produtos. Por isso, a listagem retorna 200. O status 404 será usado na consulta de um produto específico quando ele não existir.

Compile, execute a API e teste `GET /api/produtos` no Swagger. Agora o resultado será **200** com `[]`.

### 3.2. Acrescente a busca por ID

Pare a API. No mesmo arquivo, adicione este método **dentro da classe**, depois de `Listar()` e antes da última chave `}`:

```csharp
    [HttpGet("{id:int}")]
    public ActionResult<Produto> BuscarPorId(int id)
    {
        var produto = Produtos.FirstOrDefault(item => item.Id == id);
        if (produto is null)
        {
            return NotFound();
        }
        return Ok(produto);
    }
```

`[HttpGet("{id:int}")]` acrescenta um ID inteiro à rota, como `/api/produtos/1`. O parâmetro `int id` recebe o valor da URL. A busca retorna 200 quando encontra o produto e 404 quando não encontra.

`FirstOrDefault` devolve o primeiro produto que atende ao filtro, ou `null` se nenhum for encontrado. A expressão `item => item.Id == id` é uma função lambda: para cada item, verifica se seu ID é igual ao recebido na URL.

`var` deixa o compilador inferir o tipo da variável a partir da expressão atribuída; a variável continua tendo um tipo definido. `if (produto is null)` trata a ausência antes de devolver os dados. `return NotFound()` encerra o método com status 404; se houver um produto, `return Ok(produto)` devolve 200 com seu JSON.

Compile, execute e teste `GET /api/produtos/1`. Como ainda não cadastramos produtos, espere **404**.

### 3.3. Acrescente o cadastro

Pare a API. Dentro da classe, logo abaixo da declaração da lista `Produtos`, adicione o contador:

```csharp
    private static int _proximoId = 1;
```

Depois de `BuscarPorId()` e antes da última chave da classe, acrescente:

```csharp
    [HttpPost]
    public ActionResult<Produto> Cadastrar([FromBody] Produto produto)
    {
        produto.Id = _proximoId++;
        produto.Nome = produto.Nome.Trim();
        Produtos.Add(produto);

        return CreatedAtAction(
            nameof(BuscarPorId), new { id = produto.Id }, produto);
    }
```

`[HttpPost]` associa o método ao cadastro. `[FromBody] Produto produto` indica que o corpo JSON da requisição deve ser convertido em um objeto `Produto`. Antes de executar o método, a API valida os atributos desse objeto: uma entrada inválida recebe 400 e não chega à lista.

`_proximoId++` entrega o ID atual e incrementa o contador para o próximo cadastro. Como o contador também é `static`, ele é compartilhado entre requisições. O cliente não decide o ID. `Trim()` remove espaços das pontas do nome e `Produtos.Add(produto)` adiciona o objeto à lista.

`CreatedAtAction` devolve 201, o produto criado e um header `Location` para sua consulta. `nameof(BuscarPorId)` obtém o nome do método de busca; `new { id = produto.Id }` cria um objeto anônimo com o parâmetro necessário para montar essa URL.

Nesta versão, o Controller recebe a requisição, manipula a lista e escolhe a resposta HTTP. Na Parte 2, vamos distribuir essas responsabilidades entre as camadas.

Compile e execute novamente. O Controller agora terá os métodos `Listar`, `BuscarPorId` e `Cadastrar`, além da lista e do contador. Confira se todos estão dentro das chaves da classe.

## 4. Teste no Swagger

Clique em **Try it out**, preencha a requisição e execute. Cadastre:

```json
{
  "nome": "Caderno",
  "preco": 25.9
}
```

JSON usa ponto nas casas decimais. Não é necessário enviar ID.

| Teste                                    | Resultado esperado                    |
| ---------------------------------------- | ------------------------------------- |
| GET da lista antes do cadastro           | 200 e lista vazia                     |
| POST válido                              | 201, produto com ID e header Location |
| GET pelo ID retornado                    | 200 e produto cadastrado              |
| GET com ID inexistente, por exemplo 9999 | 404                                   |
| POST com nome vazio                      | 400                                   |
| POST com preço -5                        | 400                                   |
| GET após os dois cadastros inválidos     | Apenas os produtos válidos            |

Observe o corpo do erro 400: ele informa quais campos falharam. Confirme que a requisição inválida não chegou a cadastrar. A validação depende dos atributos do model e do comportamento de `[ApiController]`.

Encerre e reinicie a API: a lista fica vazia porque seus dados estavam na memória do processo. Na Parte 2, compararemos esse comportamento com o SQLite.

## 5. O caminho de uma requisição e o registro das alterações

No cadastro, o caminho é: JSON → objeto Produto → validação → método Cadastrar → lista → resposta 201. Quando a entrada é inválida, a API responde 400 antes de executar `Cadastrar`. Na consulta por ID, a busca encontra um objeto e devolve 200, ou encontra `null` e devolve 404.

Pelo Source Control do VS Code, revise os arquivos alterados e faça um commit, para registrar a versão que construímos. Os arquivos gerados, dependências e bancos estão no `.gitignore`. Se esta pasta já pertence a um repositório, utilize esse repositório; caso seja uma cópia independente, inicialize-o pelo VS Code. A publicação no GitHub pode ser feita ao concluir o projeto em seu próprio repositório.

## Para praticar: testar e justificar as validações

Cadastre dois produtos válidos e prepare uma tabela com requisição enviada, status recebido e explicação para cada caso:

1. Nome vazio.
2. Nome com mais de 100 caracteres.
3. Preço zero.
4. Preço acima de 10.000.
5. Busca de ID inexistente.

Confirme que nenhum cadastro inválido aparece na lista. Depois, reinicie a API e explique por que os produtos sumiram. Você pode usar um arquivo Markdown para registrar os resultados.

Se uma validação estiver faltando, ajuste o model e faça um novo teste. Apresente um exemplo válido, um inválido e o trecho responsável pela resposta. Traga suas dúvidas para o segundo encontro.

Referências: [controllers e respostas](https://learn.microsoft.com/pt-br/aspnet/core/web-api/?view=aspnetcore-8.0) e [validação de modelos](https://learn.microsoft.com/pt-br/aspnet/core/mvc/models/validation?view=aspnetcore-8.0).
