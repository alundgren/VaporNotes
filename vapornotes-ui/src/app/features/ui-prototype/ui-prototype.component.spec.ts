import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UiPrototypeComponent } from './ui-prototype.component';

describe('UiPrototypeComponent', () => {
  let component: UiPrototypeComponent;
  let fixture: ComponentFixture<UiPrototypeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiPrototypeComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(UiPrototypeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
