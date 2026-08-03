import {
  ChangeDetectorRef,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal
} from '@angular/core';

import {
  DatePipe,
  DecimalPipe
} from '@angular/common';

import {
  EMPTY,
  Subject,
  catchError,
  exhaustMap,
  finalize,
  merge,
  tap,
  timeout,
  timer
} from 'rxjs';

import {
  takeUntilDestroyed
} from '@angular/core/rxjs-interop';

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
  imports: [
    DatePipe,
    DecimalPipe
  ],
  templateUrl: './pedidos-delivery.html',
  styleUrl: './pedidos-delivery.css'
})
export class PedidosDelivery implements OnInit {

  private readonly destroyRef =
    inject(DestroyRef);

  private readonly cdr =
    inject(ChangeDetectorRef);

  private readonly actualizar$ =
    new Subject<void>();

  private readonly pedidosOcultos =
    new Set<string>();

  pedidos: PedidoDTO[] = [];

  pageNumber = 1;
  pageSize = 10;
  totalPedidos = 0;

  cargandoInicial = signal(true);

  pedidoFinalizandoId: string | null = null;

  error = '';

  constructor(
    private pedidosService: ServicioDeliveryService,
    private toastr: ToastrService
  ) {
  }

  ngOnInit(): void {
    this.iniciarActualizacionAutomatica();
  }

  private iniciarActualizacionAutomatica(): void {

    merge(
      timer(0, 2000),
      this.actualizar$
    )
      .pipe(
        exhaustMap(() => {

          if (this.pedidoFinalizandoId !== null) {
            return EMPTY;
          }

          return this.pedidosService
            .ObtenerPedidosDelivery(
              this.pageNumber,
              this.pageSize
            )
            .pipe(
              timeout(10000),

              tap((respuesta: any) => {

                const lista =
                  respuesta.pedidos ??
                  respuesta.Pedidos ??
                  [];

                const pedidos: PedidoDTO[] = lista;

                for (
                  const pedidoId
                  of Array.from(this.pedidosOcultos)
                ) {
                  const existe =
                    pedidos.some(
                      (pedido: PedidoDTO) =>
                        pedido.id === pedidoId
                    );

                  if (!existe) {
                    this.pedidosOcultos.delete(
                      pedidoId
                    );
                  }
                }

                this.pedidos =
                  pedidos.filter(
                    (pedido: PedidoDTO) =>
                      !this.pedidosOcultos.has(
                        pedido.id
                      ) &&
                      pedido.estadoDelivery
                        .toLowerCase() !==
                      'finalizado'
                  );

                const total =
                  respuesta.totalPedidos ??
                  respuesta.TotalPedidos ??
                  this.pedidos.length;

                this.totalPedidos =
                  Math.max(
                    this.pedidos.length,
                    total -
                    this.pedidosOcultos.size
                  );

                this.error = '';

                this.cdr.markForCheck();
              }),

              catchError((error) => {

                this.error =
                  error.name === 'TimeoutError'
                    ? 'La API tardó demasiado en responder.'
                    : error.error?.mensaje ??
                    'No se pudieron cargar los pedidos.';

                this.cdr.markForCheck();

                return EMPTY;
              }),

              finalize(() => {
                this.cargandoInicial.set(false);
                this.cdr.markForCheck();
              })
            );
        }),

        takeUntilDestroyed(
          this.destroyRef
        )
      )
      .subscribe();
  }


  cargarPedidosDelivery(): void {
    this.actualizar$.next();
  }

  finalizarDelivery(
    pedidoId: string
  ): void {

    if (
      !pedidoId ||
      this.pedidoFinalizandoId !== null
    ) {
      return;
    }

    this.pedidoFinalizandoId =
      pedidoId;

    this.cdr.markForCheck();

    this.pedidosService
      .FinalizarDelivery(pedidoId)
      .pipe(
        timeout(10000),

        finalize(() => {
          this.pedidoFinalizandoId =
            null;

          this.cdr.markForCheck();
          this.actualizar$.next();
        })
      )
      .subscribe({
        next: () => {

          this.pedidosOcultos.add(
            pedidoId
          );

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
            'El pedido fue entregado correctamente.',
            'Pedido finalizado'
          );

          this.cdr.markForCheck();
        },

        error: (error) => {

          const mensaje =
            error.name === 'TimeoutError'
              ? 'La API tardó demasiado en responder.'
              : error.error?.mensaje ??
              'No se pudo finalizar el pedido.';

          this.toastr.error(
            mensaje,
            'Error'
          );

          this.cdr.markForCheck();
        }
      });
  }

  paginaAnterior(): void {

    if (this.pageNumber <= 1) {
      return;
    }

    this.pageNumber--;
    this.actualizar$.next();
  }

  paginaSiguiente(): void {

    if (
      this.pageNumber >=
      this.totalPaginas
    ) {
      return;
    }

    this.pageNumber++;
    this.actualizar$.next();
  }

  cambiarPageSize(
    nuevoPageSize: string
  ): void {

    const valor =
      Number(nuevoPageSize);

    if (
      valor <= 0 ||
      valor === this.pageSize
    ) {
      return;
    }

    this.pageSize = valor;
    this.pageNumber = 1;

    this.actualizar$.next();
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