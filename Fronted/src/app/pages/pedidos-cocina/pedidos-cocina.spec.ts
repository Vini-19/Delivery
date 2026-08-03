import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PedidosCocina } from './pedidos-cocina';

describe('PedidosCocina', () => {
  let component: PedidosCocina;
  let fixture: ComponentFixture<PedidosCocina>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PedidosCocina]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PedidosCocina);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
