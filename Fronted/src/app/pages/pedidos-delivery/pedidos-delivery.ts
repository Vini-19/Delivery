import {
  ChangeDetectorRef,
  Component,
  OnDestroy,
  OnInit
} from '@angular/core';

import {
  CommonModule
} from '@angular/common';

import {
  finalize,
  timeout
} from 'rxjs';

import {
  ToastrService
} from 'ngx-toastr';

import {
  ServicioDeliveryService
} from '../../services/ServicioDelivery/servicio-delivery.service';

import {
  PedidoDTO
} from '../../../Models/PedidoDTO';

@Component({
  selector: 'app-pedidos-delivery',
  standalone: true,
  imports: [
    CommonModule
  ],
  templateUrl: './pedidos-delivery.html',
  styleUrl: './pedidos-delivery.css'
})
export class PedidosDelivery implements OnInit, OnDestroy {

  pedidos: PedidoDTO[] = [];

  pageNumber = 1;
  pageSize = 100;
  totalPedidos = 0;

  cargando = true;
  consultando = false;

  pedidoFinalizandoId: string | null = null;

  private intervalo:
    ReturnType<typeof setInterval> | null = null;

  constructor(
    private pedidosService: ServicioDeliveryService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {
  }

  ngOnInit(): void {
    this.cargarPedidosDelivery(true);

    this.intervalo = setInterval(() => {
      this.cargarPedidosDelivery(false);
    }, 2000);
  }

  ngOnDestroy(): void {
    if (this.intervalo !== null) {
      clearInterval(this.intervalo);
    }
  }

  cargarPedidosDelivery(
    mostrarCarga: boolean = false
  ): void {

    if (
      this.consultando ||
      this.pedidoFinalizandoId !== null
    ) {
      return;
    }

    this.consultando = true;

    if (mostrarCarga) {
      this.cargando = true;
    }

    this.pedidosService
      .ObtenerPedidosDelivery(
        this.pageNumber,
        this.pageSize
      )
      .pipe(
        timeout(10000),

        finalize(() => {
          this.consultando = false;
          this.cargando = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (respuesta) => {
          this.pedidos =
            respuesta.pedidos ?? [];

          this.totalPedidos =
            respuesta.totalPedidos ?? 0;

          this.cdr.detectChanges();
        },

        error: (error) => {
          console.error(
            'Error cargando pedidos:',
            error
          );

          if (mostrarCarga) {
            this.toastr.error(
              'No se pudieron cargar los pedidos',
              'Error'
            );
          }
        }
      });
  }

  finalizarDelivery(
    pedidoId: string
  ): void {

    if (this.pedidoFinalizandoId !== null) {
      return;
    }

    this.pedidoFinalizandoId =
      pedidoId;

    this.cdr.detectChanges();

    this.pedidosService
      .FinalizarDelivery(pedidoId)
      .pipe(
        timeout(10000),

        finalize(() => {
          this.pedidoFinalizandoId =
            null;

          this.cdr.detectChanges();
          this.cargarPedidosDelivery(false);
        })
      )
      .subscribe({
        next: () => {
          this.pedidos =
            this.pedidos.filter(
              pedido =>
                pedido.id !== pedidoId
            );

          this.totalPedidos =
            Math.max(
              0,
              this.totalPedidos - 1
            );

          if (
            this.pedidos.length === 0 &&
            this.pageNumber > 1
          ) {
            this.pageNumber--;
          }

          this.toastr.success(
            'Pedido entregado correctamente',
            'Delivery'
          );

          this.cdr.detectChanges();
        },

        error: (error) => {
          this.toastr.error(
            error.error?.mensaje ??
            'No se pudo finalizar el pedido',
            'Error'
          );
        }
      });
  }

  paginaAnterior(): void {

    if (
      this.pageNumber <= 1 ||
      this.consultando
    ) {
      return;
    }

    this.pageNumber--;
    this.cargarPedidosDelivery(true);
  }

  paginaSiguiente(): void {

    if (
      this.pageNumber >=
        this.totalPaginas ||
      this.consultando
    ) {
      return;
    }

    this.pageNumber++;
    this.cargarPedidosDelivery(true);
  }

  cambiarPageSize(
    pageSize: string
  ): void {

    this.pageSize =
      Number(pageSize);

    this.pageNumber = 1;

    this.cargarPedidosDelivery(true);
  }

  get totalPaginas(): number {

    return Math.max(
      1,
      Math.ceil(
        this.totalPedidos /
        this.pageSize
      )
    );
  }
}