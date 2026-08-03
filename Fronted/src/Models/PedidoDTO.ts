import { DetallesPedido } from "./DetallesPedido";


export interface PedidoDTO {
  id: string;
  cliente: string;
  estado: string;
  estadoDelivery: string;
  lugar_envio: string;
  subTotal: number;
  isv: number;
  total: number;
  creado: string;
  finalizado: string | null;
  detalles: DetallesPedido[];
}