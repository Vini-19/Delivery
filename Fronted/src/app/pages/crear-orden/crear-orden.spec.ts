import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CrearOrden } from './crear-orden';

describe('CrearOrden', () => {
  let component: CrearOrden;
  let fixture: ComponentFixture<CrearOrden>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CrearOrden]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CrearOrden);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
