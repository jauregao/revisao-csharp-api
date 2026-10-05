# Angular conectado à API

Uma tela com formulário e lista de produtos. Usa `ngModel`, `HttpClient` e `subscribe` para cadastrar, editar, excluir e atualizar a lista.

## Executar

API, em `base/back`:

```powershell
dotnet ef database update
dotnet run --launch-profile http
```

Angular, em outro terminal na pasta `base/front`:

```powershell
npm install
npm start
```

Abra `http://localhost:4200`. O exemplo foi compilado com Node.js 24.18.

## Arquivos

| Arquivo                                            | Responsabilidade                                 |
| -------------------------------------------------- | ------------------------------------------------ |
| [main.ts](src/main.ts)                             | Inicialização e registro de HttpClient           |
| [produto.ts](src/app/produto.ts)                   | Tipos dos dados recebidos e enviados             |
| [produtos.service.ts](src/app/produtos.service.ts) | GET, POST, PUT e DELETE para a API na porta 5027 |
| [app.component.ts](src/app/app.component.ts)       | Formulário, lista, edição e mensagens de erro    |
| [app.component.html](src/app/app.component.html)   | Campos, botões e tabela                          |
| [styles.css](src/styles.css)                       | Estilos da tela                                  |
| [Program.cs da API](../back/Program.cs)            | Política de CORS                                 |

## CORS

O Angular chama diretamente `http://localhost:5027/api/produtos`. A API permite a origem `http://localhost:4200` com `WithOrigins`, e aplica a política com `UseCors`. Não há proxy entre as aplicações.

No navegador, a aba Network mostra as requisições. POST e PUT com JSON provocam uma consulta OPTIONS antes da operação, quando o navegador precisa verificar a permissão. Na resposta, confira `Access-Control-Allow-Origin: http://localhost:4200`.

Para observar um bloqueio, mude temporariamente a origem permitida na API para outra porta e reinicie a API. Uma chamada feita pelo Angular será bloqueada pelo navegador, mesmo que o Swagger continue funcionando. Restaure a origem 4200 ao terminar.

[API completa](../back/README.md) · [Explicação de CORS no plantão 2](../../plantao-2/README.md#21-cors-chamadas-de-outra-origem)
