import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="register-container">
      <div class="register-card">
        <h2>Register</h2>
        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
      <div class="form-group">
        <label for="name">Full Name:</label>
        <input 
          type="text" 
          id="name" 
          formControlName="name" 
          class="form-control"
          [class.is-invalid]="registerForm.get('name')?.invalid && registerForm.get('name')?.touched">
        <div *ngIf="registerForm.get('name')?.invalid && registerForm.get('name')?.touched" class="invalid-feedback">
          Name is required
        </div>
      </div>

      <div class="form-group">
        <label for="email">Email:</label>
        <input 
          type="email" 
          id="email" 
          formControlName="email" 
          class="form-control"
          [class.is-invalid]="registerForm.get('email')?.invalid && registerForm.get('email')?.touched">
        <div *ngIf="registerForm.get('email')?.invalid && registerForm.get('email')?.touched" class="invalid-feedback">
          Email is required and must be valid
        </div>
      </div>

      <div class="form-group">
        <label for="phoneNumber">Phone Number:</label>
        <input 
          type="tel" 
          id="phoneNumber" 
          formControlName="phoneNumber" 
          class="form-control"
          [class.is-invalid]="registerForm.get('phoneNumber')?.invalid && registerForm.get('phoneNumber')?.touched">
        <div *ngIf="registerForm.get('phoneNumber')?.invalid && registerForm.get('phoneNumber')?.touched" class="invalid-feedback">
          Phone number is required
        </div>
      </div>

      <div class="form-group">
        <label for="password">Password:</label>
        <input 
          type="password" 
          id="password" 
          formControlName="password" 
          class="form-control"
          [class.is-invalid]="registerForm.get('password')?.invalid && registerForm.get('password')?.touched">
        <div *ngIf="registerForm.get('password')?.invalid && registerForm.get('password')?.touched" class="invalid-feedback">
          Password must be at least 6 characters
        </div>
      </div>

      <div class="form-group">
        <label for="confirmPassword">Confirm Password:</label>
        <input 
          type="password" 
          id="confirmPassword" 
          formControlName="confirmPassword" 
          class="form-control"
          [class.is-invalid]="registerForm.get('confirmPassword')?.invalid && registerForm.get('confirmPassword')?.touched">
        <div *ngIf="registerForm.get('confirmPassword')?.invalid && registerForm.get('confirmPassword')?.touched" class="invalid-feedback">
          Passwords must match
        </div>
      </div>

      <button 
        type="submit" 
        class="btn btn-primary" 
        [disabled]="registerForm.invalid || loading">
        {{ loading ? 'Registering...' : 'Register' }}
      </button>

      <div *ngIf="error" class="error-message">
        {{ error }}
      </div>
    </form>

    <div class="login-link">
      <p>Already have an account? <a routerLink="/login">Login here</a></p>
    </div>
      </div>
    </div>
  `,
  styles: [`
    .register-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 20px;
    }

    .register-card {
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

    input, select {
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

    .login-link {
      text-align: center;
      margin-top: 1.5rem;
    }

    .login-link a {
      color: #667eea;
      text-decoration: none;
    }

    .login-link a:hover {
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  error = '';

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    this.registerForm = this.formBuilder.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      role: ['user'] // Default role set to 'user', not editable by user
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
    }
    return null;
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(this.registerForm.controls).forEach(key => {
        this.registerForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;
    this.error = '';

    // Prepare the data to match your JSON schema
    const registerData = {
      email: this.registerForm.value.email,
      name: this.registerForm.value.name,
      phoneNumber: this.registerForm.value.phoneNumber,
      password: this.registerForm.value.password,
      confirmPassword: this.registerForm.value.confirmPassword,
      role: this.registerForm.value.role // Will always be 'user'
    };

    this.authService.register(registerData).subscribe({
      next: (user) => {
        this.loading = false;
        if (user.role.toLowerCase() === 'admin') {
          this.router.navigate(['/dashboard']);
        } else {
          this.router.navigate(['/home']);
        }
      },
      error: (error) => {
        this.loading = false;
        this.error = error.error?.message || 'Registration failed. Please try again.';
      }
    });
  }
}