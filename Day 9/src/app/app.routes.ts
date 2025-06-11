import { Routes } from '@angular/router';

import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { HomeComponent } from './components/home/home.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { MissionComponent } from './components/mission/mission.component';
import { MissionapplicationComponent } from './components/missionapplication/missionapplication.component';
import { MissionSkillComponent } from './components/missionskills/missionskills.component';
import { MissionThemeComponent } from './components/missiontheme/missiontheme.component';

import { AuthGuard } from './guards/auth.guard';
import { AdminGuard } from './guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'home', component: HomeComponent, canActivate: [AuthGuard] },

  // Admin routes
  { path: 'dashboard', component: DashboardComponent, canActivate: [AdminGuard] },
  { path: 'mission', component: MissionComponent, canActivate: [AdminGuard] },
  { path: 'mission-application', component: MissionapplicationComponent, canActivate: [AdminGuard] },
  { path: 'mission-skills', component: MissionSkillComponent, canActivate: [AdminGuard] },
  { path: 'mission-theme', component: MissionThemeComponent, canActivate: [AdminGuard] },

  { path: '**', redirectTo: '/login' }
];
