export interface Produto {
  id: number;
  nome: string;
  preco: number;
}

export type DadosProduto = Omit<Produto, "id">;
