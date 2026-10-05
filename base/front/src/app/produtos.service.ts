import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { DadosProduto, Produto } from "./produto";

@Injectable({ providedIn: "root" })
export class ProdutosService {
  private readonly url = "http://localhost:5027/api/produtos";

  constructor(private readonly http: HttpClient) {}

  listar() {
    return this.http.get<Produto[]>(this.url);
  }

  cadastrar(dados: DadosProduto) {
    return this.http.post<Produto>(this.url, dados);
  }

  atualizar(id: number, dados: DadosProduto) {
    return this.http.put<Produto>(`${this.url}/${id}`, dados);
  }

  remover(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
