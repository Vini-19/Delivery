import { TestBed } from '@angular/core/testing';

import { ServicioDeliveryService } from './servicio-delivery.service';

describe('ServicioDeliveryService', () => {
  let service: ServicioDeliveryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ServicioDeliveryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
