import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Issue } from './issue';

describe('Issue', () => {
  let component: Issue;
  let fixture: ComponentFixture<Issue>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [Issue],
    }).compileComponents();

    fixture = TestBed.createComponent(Issue);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
