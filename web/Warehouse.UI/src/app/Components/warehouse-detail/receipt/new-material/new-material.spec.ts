import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewMaterial } from './new-material';

describe('NewMaterial', () => {
  let component: NewMaterial;
  let fixture: ComponentFixture<NewMaterial>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [NewMaterial],
    }).compileComponents();

    fixture = TestBed.createComponent(NewMaterial);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
