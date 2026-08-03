import {
  Component
} from '@angular/core';

import {
  CommonModule
} from '@angular/common';

import {
  FormArray,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';

import {
  finalize
} from 'rxjs';

import {
  ToastrService
} from 'ngx-toastr';

import {
  ServicioDeliveryService
} from '../../services/ServicioDelivery/servicio-delivery.service';

import {
  CrearPedidoDTO
} from '../../../Models/CrearPedidoDTO';

interface Producto {
  id: number;
  nombre: string;
  precio: number;
}

@Component({
  selector: 'app-crear-orden',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './crear-orden.html',
  styleUrl: './crear-orden.css'
})
export class CrearOrden {

  guardando = false;

  productos: Producto[] = [
    {
      id: 1,
      nombre: 'Hamburguesa clásica',
      precio: 85
    },
    {
      id: 2,
      nombre: 'Pizza personal',
      precio: 120
    },
    {
      id: 3,
      nombre: 'Pollo con papas',
      precio: 110
    },
    {
      id: 4,
      nombre: 'Tacos de pollo',
      precio: 75
    },
    {
      id: 5,
      nombre: 'Baleada especial',
      precio: 60
    },
    {
      id: 6,
      nombre: 'Refresco',
      precio: 25
    }
  ];

  formulario: FormGroup;

  constructor(
    private fb: FormBuilder,
    private deliveryService: ServicioDeliveryService,
    private toastr: ToastrService
  ) {
    this.formulario = this.fb.group({
      cliente: [
        '',
        Validators.required
      ],

      lugar_envio: [
        '',
        Validators.required
      ],

      detalles: this.fb.array([
        this.crearDetalle()
      ])
    });
  }

  get detalles(): FormArray {
    return this.formulario.get(
      'detalles'
    ) as FormArray;
  }

  get subTotal(): number {
    return this.detalles.controls.reduce(
      (total, control) => {
        const precio =
          Number(
            control.get('precio')?.value
          ) || 0;

        const cantidad =
          Number(
            control.get('cantidad')?.value
          ) || 0;

        return total + precio * cantidad;
      },
      0
    );
  }

  get isv(): number {
    return this.subTotal * 0.15;
  }

  get total(): number {
    return this.subTotal + this.isv;
  }

  crearDetalle(): FormGroup {
    return this.fb.group({
      productoId: [
        '',
        Validators.required
      ],

      productoNombre: [
        ''
      ],

      precio: [
        0
      ],

      cantidad: [
        1,
        [
          Validators.required,
          Validators.min(1)
        ]
      ],

      nota: [
        ''
      ]
    });
  }

  seleccionarProducto(
    indice: number
  ): void {

    const detalle =
      this.detalles.at(indice);

    const productoId =
      Number(
        detalle.get('productoId')?.value
      );

    const producto =
      this.productos.find(
        item => item.id === productoId
      );

    detalle.patchValue({
      productoNombre:
        producto?.nombre ?? '',

      precio:
        producto?.precio ?? 0
    });
  }

  agregarDetalle(): void {
    this.detalles.push(
      this.crearDetalle()
    );
  }

  eliminarDetalle(
    indice: number
  ): void {

    if (this.detalles.length === 1) {
      this.toastr.warning(
        'Debe existir al menos un producto'
      );

      return;
    }

    this.detalles.removeAt(indice);
  }

  guardarOrden(): void {

    if (this.guardando) {
      return;
    }

    if (this.formulario.invalid) {
      this.formulario.markAllAsTouched();

      this.toastr.warning(
        'Complete todos los campos obligatorios',
        'Formulario incompleto'
      );

      console.log(
        'Formulario inválido:',
        this.formulario.value
      );

      console.log(
        'Errores:',
        this.formulario.errors
      );

      return;
    }

    const dto: CrearPedidoDTO = {
      cliente:
        this.formulario.value.cliente.trim(),

      lugar_envio:
        this.formulario.value.lugar_envio.trim(),

      subTotal:
        this.subTotal,

      isv:
        this.isv,

      total:
        this.total,

      detalles:
        this.formulario.value.detalles.map(
          (detalle: any) => ({
            productoNombre:
              detalle.productoNombre,

            cantidad:
              Number(detalle.cantidad),

            nota:
              detalle.nota?.trim() || null
          })
        )
    };

    this.guardando = true;

    this.deliveryService
      .CrearPedidos(dto)
      .pipe(
        finalize(() => {
          this.guardando = false;
        })
      )
      .subscribe({
        next: (respuesta) => {

          console.log(
            'Pedido registrado:',
            respuesta
          );

          this.toastr.success(
            'Orden registrada correctamente',
            'Orden creada'
          );
          this.toastr.info(
            'Orden Enviada a Cocina',
          );

          this.limpiarFormulario();
        },

        error: (error) => {

          console.error(
            'Error registrando orden:',
            error
          );

          this.toastr.error(
            error.error?.mensaje ??
            error.message ??
            'No se pudo registrar la orden',
            'Error'
          );
        }
      });
  }

  limpiarFormulario(): void {

    this.formulario.reset({
      cliente: '',
      lugar_envio: ''
    });

    this.detalles.clear();

    this.detalles.push(
      this.crearDetalle()
    );
  }
}