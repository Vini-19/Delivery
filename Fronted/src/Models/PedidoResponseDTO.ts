import { PedidoDTO } from "./PedidoDTO";

export interface PedidoResponseDTO {
    totalPedidos : number;
    paginaNumero : number;
    tamañoPagina : number;
    totalPaginas : number;
    pedidos : PedidoDTO[];
}