import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WaitingPermission } from './waiting-permission';

describe('WaitingPermission', () => {
  let component: WaitingPermission;
  let fixture: ComponentFixture<WaitingPermission>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [WaitingPermission],
    }).compileComponents();

    fixture = TestBed.createComponent(WaitingPermission);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
