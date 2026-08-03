import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegistroPedidos } from './registro-pedidos';

describe('RegistroPedidos', () => {
  let component: RegistroPedidos;
  let fixture: ComponentFixture<RegistroPedidos>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegistroPedidos]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegistroPedidos);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
