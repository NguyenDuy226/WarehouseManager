import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SetWarehouseModal } from './set-warehouse-modal';

describe('SetWarehouseModal', () => {
  let component: SetWarehouseModal;
  let fixture: ComponentFixture<SetWarehouseModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SetWarehouseModal],
    }).compileComponents();

    fixture = TestBed.createComponent(SetWarehouseModal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
