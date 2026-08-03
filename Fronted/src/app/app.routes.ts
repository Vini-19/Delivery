import { Routes } from '@angular/router';

import { Inicio } from './pages/inicio/inicio';
import { Home } from './pages/home/home';
import { RegistroPedidos } from './pages/registro-pedidos/registro-pedidos';
import { PedidosCocina } from './pages/pedidos-cocina/pedidos-cocina';
import { PedidosDelivery } from './pages/pedidos-delivery/pedidos-delivery';
import { CrearOrden } from './pages/crear-orden/crear-orden';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'Inicio',
    pathMatch: 'full'
  },
  {
    path: 'Inicio',
    component: Inicio,
    children: [
      {
        path: '',
        component: Home
      },
      {
        path: 'RegistroPedidos',
        component: RegistroPedidos
      },
      {
        path: 'PedidosCocina',
        component: PedidosCocina
      },
      {
        path: 'CrearOden',
        component: CrearOrden
      },
      {
        path: 'PedidosDelivery',
        component: PedidosDelivery
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'Inicio'
  }
];