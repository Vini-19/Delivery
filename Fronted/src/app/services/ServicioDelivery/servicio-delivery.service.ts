import {
  Injectable
} from '@angular/core';

import {
  HttpClient,
  HttpParams
} from '@angular/common/http';

import {
  Observable
} from 'rxjs';

import {
  environment
} from '../../../environments/environment';

import {
  CrearPedidoDTO
} from '../../../Models/CrearPedidoDTO';

import {
  PedidoDTO
} from '../../../Models/PedidoDTO';

import {
  PedidoResponseDTO
} from '../../../Models/PedidoResponseDTO';

@Injectable({
  providedIn: 'root'
})
export class ServicioDeliveryService {

  private readonly apiURL =
    `${environment.APIurl}/Pedidos`;

  constructor(
    private http: HttpClient
  ) {
  }

  CrearPedidos(
    dto: CrearPedidoDTO
  ): Observable<any> {

    return this.http.post(
      this.apiURL,
      dto
    );
  }

  ObtenerPerdidosRegistros(
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<PedidoResponseDTO> {

    const params =
      new HttpParams()
        .set(
          'pageNumber',
          pageNumber.toString()
        )
        .set(
          'pageSize',
          pageSize.toString()
        )
        /*
         * Evita que el navegador reutilice
         * una respuesta anterior.
         */
        .set(
          '_t',
          Date.now().toString()
        );

    return this.http.get<PedidoResponseDTO>(
      this.apiURL,
      { params }
    );
  }

  ObtenerPedidosDelivery(
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<PedidoResponseDTO> {

    const params =
      new HttpParams()
        .set(
          'pageNumber',
          pageNumber.toString()
        )
        .set(
          'pageSize',
          pageSize.toString()
        )
        /*
         * Cada petición tiene una marca
         * de tiempo diferente.
         */
        .set(
          '_t',
          Date.now().toString()
        );

    return this.http.get<PedidoResponseDTO>(
      `${this.apiURL}/delivery/disponibles`,
      { params }
    );
  }

  ObtenerPedidosPendientesCocina(
    pageNumber: number = 1,
    pageSize: number = 10
  ): Observable<PedidoResponseDTO> {

    const params =
      new HttpParams()
        .set(
          'pageNumber',
          pageNumber.toString()
        )
        .set(
          'pageSize',
          pageSize.toString()
        )
        .set(
          '_t',
          Date.now().toString()
        );

    return this.http.get<PedidoResponseDTO>(
      `${this.apiURL}/Cocina/Pendientes`,
      { params }
    );
  }

  FinalizarCocina(
    id: string
  ): Observable<any> {

    return this.http.patch(
      `${this.apiURL}/${id}/finalizar-cocina`,
      {}
    );
  }

  FinalizarDelivery(
    id: string
  ): Observable<any> {

    return this.http.patch(
      `${this.apiURL}/${id}/finalizar-delivery`,
      {}
    );
  }

  ObtenerPedidoPorId(
    id: string
  ): Observable<PedidoDTO> {

    const params =
      new HttpParams()
        .set(
          '_t',
          Date.now().toString()
        );

    return this.http.get<PedidoDTO>(
      `${this.apiURL}/${id}`,
      { params }
    );
  }
}