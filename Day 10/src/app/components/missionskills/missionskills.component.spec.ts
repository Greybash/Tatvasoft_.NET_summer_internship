import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MissionskillsComponent } from './missionskills.component';

describe('MissionskillsComponent', () => {
  let component: MissionskillsComponent;
  let fixture: ComponentFixture<MissionskillsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MissionskillsComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MissionskillsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
