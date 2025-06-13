import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MissionapplicationComponent } from './missionapplication.component';

describe('MissionapplicationComponent', () => {
  let component: MissionapplicationComponent;
  let fixture: ComponentFixture<MissionapplicationComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MissionapplicationComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MissionapplicationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
