import { CommonModule } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { DadosProduto, Produto } from "./produto";
import { ProdutosService } from "./produtos.service";

@Component({
  selector: "app-root",
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: "./app.component.html",
})
export class AppComponent implements OnInit {
  produtos: Produto[] = [];
  formulario: DadosProduto = { nome: "", preco: 0 };
  idEmEdicao: number | null = null;
  carregando = false;
  salvando = false;
  erro = "";

  constructor(private readonly service: ProdutosService) {}

  ngOnInit() {
    this.carregar();
  }

  carregar() {
    this.carregando = true;
    this.erro = "";
    this.service.listar().subscribe({
      next: (produtos) => {
        this.produtos = produtos;
        this.carregando = false;
      },
      error: (erro) => {
        this.carregando = false;
        this.mostrarErro(erro);
      },
    });
  }

  salvar() {
    this.salvando = true;
    this.erro = "";
    const dados = { ...this.formulario, nome: this.formulario.nome.trim() };
    const requisicao =
      this.idEmEdicao === null
        ? this.service.cadastrar(dados)
        : this.service.atualizar(this.idEmEdicao, dados);

    requisicao.subscribe({
      next: () => {
        this.salvando = false;
        this.cancelar();
        this.carregar();
      },
      error: (erro) => {
        this.salvando = false;
        this.mostrarErro(erro);
      },
    });
  }

  editar(produto: Produto) {
    this.idEmEdicao = produto.id;
    this.formulario = { nome: produto.nome, preco: produto.preco };
    this.erro = "";
  }

  cancelar() {
    this.idEmEdicao = null;
    this.formulario = { nome: "", preco: 0 };
  }

  remover(produto: Produto) {
    this.salvando = true;
    this.erro = "";
    this.service.remover(produto.id).subscribe({
      next: () => {
        this.salvando = false;
        if (this.idEmEdicao === produto.id) this.cancelar();
        this.carregar();
      },
      error: (erro) => {
        this.salvando = false;
        this.mostrarErro(erro);
      },
    });
  }

  private mostrarErro(erro: HttpErrorResponse) {
    if (erro.status === 0) {
      this.erro =
        "Não foi possível acessar a API. Confira se ela está ligada e se CORS permite http://localhost:4200.";
      return;
    }
    const validacoes = erro.error?.errors as
      Record<string, string[]> | undefined;
    this.erro = validacoes
      ? Object.values(validacoes).flat().join(" ")
      : `A operação falhou (HTTP ${erro.status}). Tente atualizar a lista.`;
  }
}
