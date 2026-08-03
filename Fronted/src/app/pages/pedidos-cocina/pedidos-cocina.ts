import { ChangeDetectorRef, Component, OnDestroy, OnInit } from '@angular/core';
import { PedidoDTO } from '../../../Models/PedidoDTO';
import { ServicioDeliveryService } from '../../services/ServicioDelivery/servicio-delivery.service';
import { ToastrService } from 'ngx-toastr';
import { finalize, timeout } from 'rxjs';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pedidos-cocina',
  imports: [CommonModule],
  templateUrl: './pedidos-cocina.html',
  styleUrl: './pedidos-cocina.css',
})
export class PedidosCocina implements OnInit, OnDestroy {
  
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
    this.cargarPedidosCocina(true);

    this.intervalo = setInterval(() => {
      this.cargarPedidosCocina(false);
    }, 2000);
  }

  ngOnDestroy(): void {
    if (this.intervalo !== null) {
      clearInterval(this.intervalo);
    }
  }

  cargarPedidosCocina(
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
      .ObtenerPedidosPendientesCocina(
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
              'No se pudieron cargar los pedidos de cocina',
              'Error'
            );
          }
        }
      });
  }

  finalizarPedidoCocina(
    pedidoId: string
  ): void {

    if (this.pedidoFinalizandoId !== null) {
      return;
    }

    this.pedidoFinalizandoId =
      pedidoId;

    this.cdr.detectChanges();

    this.pedidosService
      .FinalizarCocina(pedidoId)
      .pipe(
        timeout(10000),

        finalize(() => {
          this.pedidoFinalizandoId =
            null;

          this.cdr.detectChanges();
          this.cargarPedidosCocina(false);
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
            'Pedido hecho correctamente',
            
          );

          this.cdr.detectChanges();
        },

        error: (error) => {
          this.toastr.error(
            error.error?.mensaje ??
            'No se pudo finalizar el pedido en cocina',
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
    this.cargarPedidosCocina(true);
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
    this.cargarPedidosCocina(true);
  }

  cambiarPageSize(
    pageSize: string
  ): void {

    this.pageSize =
      Number(pageSize);

    this.pageNumber = 1;

    this.cargarPedidosCocina(true);
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
