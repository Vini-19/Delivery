import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PedidosDelivery } from './pedidos-delivery';

describe('PedidosDelivery', () => {
  let component: PedidosDelivery;
  let fixture: ComponentFixture<PedidosDelivery>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PedidosDelivery]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PedidosDelivery);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
