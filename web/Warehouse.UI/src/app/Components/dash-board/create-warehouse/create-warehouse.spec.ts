import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateWarehouse } from './create-warehouse';

describe('CreateWarehouse', () => {
  let component: CreateWarehouse;
  let fixture: ComponentFixture<CreateWarehouse>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CreateWarehouse],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateWarehouse);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
