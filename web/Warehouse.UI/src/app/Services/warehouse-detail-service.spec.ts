import { TestBed } from '@angular/core/testing';

import { WarehouseDetailService } from './warehouse-detail-service';

describe('WarehouseDetailService', () => {
  let service: WarehouseDetailService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WarehouseDetailService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
