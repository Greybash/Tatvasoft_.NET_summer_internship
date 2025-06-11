import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="login-container">
      <div class="login-card">
        <h2>Login</h2>
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="email">Email:</label>
            <input 
              type="email" 
              id="email" 
              formControlName="email"
              [class.error]="loginForm.get('email')?.invalid && loginForm.get('email')?.touched"
            >
            <div class="error-message" *ngIf="loginForm.get('email')?.invalid && loginForm.get('email')?.touched">
              Email is required and must be valid
            </div>
          </div>

          <div class="form-group">
            <label for="password">Password:</label>
            <input 
              type="password" 
              id="password" 
              formControlName="password"
              [class.error]="loginForm.get('password')?.invalid && loginForm.get('password')?.touched"
            >
            <div class="error-message" *ngIf="loginForm.get('password')?.invalid && loginForm.get('password')?.touched">
              Password is required
            </div>
          </div>

          <button type="submit" [disabled]="loginForm.invalid || loading" class="btn-primary">
            {{ loading ? 'Logging in...' : 'Login' }}
          </button>

          <div class="error-message" *ngIf="error">
            {{ error }}
          </div>
        </form>

        <div class="register-link">
          <p>Don't have an account? <a routerLink="/register">Register here</a></p>
        </div>

        
      </div>
    </div>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 20px;
    }

    .login-card {
      background: white;
      padding: 2rem;
      border-radius: 10px;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
      width: 100%;
      max-width: 400px;
    }

    h2 {
      text-align: center;
      margin-bottom: 2rem;
      color: #333;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: bold;
      color: #555;
    }

    input {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 5px;
      font-size: 1rem;
      box-sizing: border-box;
    }

    input.error {
      border-color: #e74c3c;
    }

    .error-message {
      color: #e74c3c;
      font-size: 0.875rem;
      margin-top: 0.5rem;
    }

    .btn-primary {
      width: 100%;
      padding: 0.75rem;
      background: #667eea;
      color: white;
      border: none;
      border-radius: 5px;
      font-size: 1rem;
      cursor: pointer;
      transition: background 0.3s;
    }

    .btn-primary:hover:not(:disabled) {
      background: #5a6fd8;
    }

    .btn-primary:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .register-link {
      text-align: center;
      margin-top: 1.5rem;
    }

    .register-link a {
      color: #667eea;
      text-decoration: none;
    }

    .register-link a:hover {
      text-decoration: underline;
    }

    .demo-credentials {
      margin-top: 2rem;
      padding: 1rem;
      background: #f8f9fa;
      border-radius: 5px;
      text-align: center;
    }

    .demo-credentials h4 {
      margin: 0 0 1rem 0;
      color: #666;
    }

    .demo-credentials p {
      margin: 0.5rem 0;
      font-size: 0.875rem;
      color: #666;
    }
  `]
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = false;
  error = '';

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    this.loginForm = this.formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

 onSubmit() {
  if (this.loginForm.invalid) {
    this.loginForm.markAllAsTouched();
    console.warn('Form invalid', this.loginForm.value);
    return;
  }

  this.loading = true;
  this.error = '';
  console.log('Submitting login', this.loginForm.value);

  this.authService.login(this.loginForm.value).subscribe({
    next: (user) => {
      this.loading = false;
      console.log('Login success:', user);
      if (user.role.toLowerCase() === 'admin') {
        this.router.navigate(['/dashboard']);
      } else {
        this.router.navigate(['/home']);
      }
    },
    error: (error) => {
      this.loading = false;
      console.error('Login error:', error);
      this.error = error.error?.message || 'Login failed. Please try again.';
    }
  });
}

}
