import { DetallesPedido } from './DetallesPedido';

export interface CrearPedidoDTO {
  cliente: string;
  lugar_envio: string;
  subTotal: number;
  isv: number;
  total: number;
  detalles: DetallesPedido[];
}