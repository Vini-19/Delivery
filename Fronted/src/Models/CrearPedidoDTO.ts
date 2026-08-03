import { DetallesPedido } from "./DetallesPedido";


export interface CrearPedidoDTO
{
    cliente : string;
    LugarEnvio : string;
    SubTotal : number;
    ISV : number;
    Total : number;
    detalle : DetallesPedido[];
}