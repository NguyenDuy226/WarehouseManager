import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewSupplier } from './new-supplier';

describe('NewSupplier', () => {
  let component: NewSupplier;
  let fixture: ComponentFixture<NewSupplier>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NewSupplier],
    }).compileComponents();

    fixture = TestBed.createComponent(NewSupplier);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
