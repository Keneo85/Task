import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from './auth';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="card login">
      <h2>{{ isRegister() ? 'Crear cuenta' : 'Iniciar sesión' }}</h2>

      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>
          Email
          <input formControlName="email" />
        </label>
        @if (form.controls.email.touched && form.controls.email.invalid) {
          <small>Email inválido</small>
        }

        <label>
          Contraseña
          <input formControlName="password" type="password" />
        </label>
        @if (form.controls.password.touched && form.controls.password.invalid) {
          <small>Mínimo 8 caracteres</small>
        }

        <button type="submit">{{ isRegister() ? 'Registrarme' : 'Entrar' }}</button>
      </form>

      <p class="error">{{ error() }}</p>

      <button class="link" (click)="isRegister.set(!isRegister())">
        {{ isRegister() ? 'Ya tengo cuenta' : 'Crear una cuenta' }}
      </button>
    </div>
  `,
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);

  isRegister = signal(false);
  error = signal('');

  form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password, this.isRegister()).subscribe({
      next: () => this.router.navigate(['/tasks']),
      error: (e) =>
        this.error.set(
          e.status === 409 ? 'El email ya está registrado' : 'Email o contraseña incorrectos',
        ),
    });
  }
}
