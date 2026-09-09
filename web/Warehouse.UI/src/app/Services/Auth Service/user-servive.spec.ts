import { TestBed } from '@angular/core/testing';

import { UserServive } from './user-servive';

describe('UserServive', () => {
  let service: UserServive;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserServive);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
