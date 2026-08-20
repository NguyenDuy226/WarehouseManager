import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddUserToWarehouse } from './add-user-to-warehouse';

describe('AddUserToWarehouse', () => {
  let component: AddUserToWarehouse;
  let fixture: ComponentFixture<AddUserToWarehouse>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AddUserToWarehouse],
    }).compileComponents();

    fixture = TestBed.createComponent(AddUserToWarehouse);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
