import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';

import {
  CommonModule
} from '@angular/common';

import {
  FormsModule
} from '@angular/forms';

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
  selector: 'app-registro-pedidos',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './registro-pedidos.html',
  styleUrl: './registro-pedidos.css'
})
export class RegistroPedidos implements OnInit {

  pedidos: PedidoDTO[] = [];
  pedidoSeleccionado: PedidoDTO | null = null;

  cargando = false;
  cargandoDetalle = false;

  pageNumber = 1;
  pageSize = 10;
  totalPedidos = 0;
  totalPaginas = 1;

  constructor(
    private deliveryService: ServicioDeliveryService,
    private toastr: ToastrService,
    private cdr: ChangeDetectorRef
  ) {
  }

  ngOnInit(): void {
    this.cargarPedidos();
  }

  cargarPedidos(): void {

    if (this.cargando) {
      return;
    }

    this.cargando = true;

    this.deliveryService
      .ObtenerPerdidosRegistros(
        this.pageNumber,
        this.pageSize
      )
      .pipe(
        timeout(10000),

        finalize(() => {
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

          this.totalPaginas =
            respuesta.totalPaginas ??
            Math.max(
              1,
              Math.ceil(
                this.totalPedidos /
                this.pageSize
              )
            );

          this.cdr.detectChanges();
        },

        error: (error) => {

          console.error(
            'Error cargando pedidos:',
            error
          );

          this.pedidos = [];
          this.totalPedidos = 0;
          this.totalPaginas = 1;

          const mensaje =
            error.name === 'TimeoutError'
              ? 'La API tardó demasiado en responder'
              : error.error?.mensaje ??
                'No se pudieron cargar los pedidos';

          this.toastr.error(
            mensaje,
            'Error'
          );

          this.cdr.detectChanges();
        }
      });
  }

  verDetalle(
    pedidoId: string
  ): void {

    if (
      !pedidoId ||
      this.cargandoDetalle
    ) {
      return;
    }

    this.cargandoDetalle = true;

    this.deliveryService
      .ObtenerPedidoPorId(pedidoId)
      .pipe(
        timeout(10000),

        finalize(() => {
          this.cargandoDetalle = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (pedido) => {
          this.pedidoSeleccionado =
            pedido;

          this.cdr.detectChanges();
        },

        error: (error) => {

          console.error(
            'Error cargando detalle:',
            error
          );

          const mensaje =
            error.name === 'TimeoutError'
              ? 'La API tardó demasiado en responder'
              : error.error?.mensaje ??
                'No se pudo cargar el detalle';

          this.toastr.error(
            mensaje,
            'Error'
          );

          this.cdr.detectChanges();
        }
      });
  }

  cerrarDetalle(): void {
    this.pedidoSeleccionado = null;
  }

  paginaAnterior(): void {

    if (
      this.pageNumber <= 1 ||
      this.cargando
    ) {
      return;
    }

    this.pageNumber--;
    this.cargarPedidos();
  }

  paginaSiguiente(): void {

    if (
      this.pageNumber >=
        this.totalPaginas ||
      this.cargando
    ) {
      return;
    }

    this.pageNumber++;
    this.cargarPedidos();
  }

  cambiarPageSize(): void {
    this.pageNumber = 1;
    this.cargarPedidos();
  }

  irAPagina(
    pagina: number
  ): void {

    if (
      pagina < 1 ||
      pagina > this.totalPaginas ||
      pagina === this.pageNumber ||
      this.cargando
    ) {
      return;
    }

    this.pageNumber = pagina;
    this.cargarPedidos();
  }

  get paginas(): number[] {

    return Array.from(
      {
        length: this.totalPaginas
      },
      (_, indice) =>
        indice + 1
    );
  }

  claseEstado(
    estado: string
  ): string {

    const valor =
      estado?.toLowerCase();

    if (
      valor === 'listo' ||
      valor === 'finalizado'
    ) {
      return 'text-bg-success';
    }
    
    if (valor === 'preparando') {
      return 'text-bg-primary';
    }

    if (valor === 'pendiente') {
      return 'text-bg-warning';
    }

    return 'text-bg-secondary';
  }
}