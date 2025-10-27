import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit, OnDestroy {
  activeTab: 'login' | 'signup' = 'login';
  loginForm!: FormGroup;
  signupForm!: FormGroup;
  
  usePassword = false;
  showPassword = false;
  showOtpInput = false;
  isLoading = false;
  resendTimer = 0;
  
  private timerInterval: any;

  constructor(
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForms();
  }

  ngOnDestroy(): void {
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
    }
  }

  initForms(): void {
    // Login Form
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: [''],
      otp: ['']
    });

    // Sign Up Form (Email only, no password)
    this.signupForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  switchTab(tab: 'login' | 'signup'): void {
    this.activeTab = tab;
    this.resetForms();
  }

  togglePasswordMode(): void {
    this.usePassword = !this.usePassword;
    this.showOtpInput = false;
    
    // Update validators
    const passwordControl = this.loginForm.get('password');
    const otpControl = this.loginForm.get('otp');
    
    if (this.usePassword) {
      passwordControl?.setValidators([Validators.required]);
      otpControl?.clearValidators();
    } else {
      passwordControl?.clearValidators();
      otpControl?.clearValidators();
    }
    
    passwordControl?.updateValueAndValidity();
    otpControl?.updateValueAndValidity();
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onLogin(): void {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    
    const formData = this.loginForm.value;

    if (this.showOtpInput) {
      // Verify OTP
      this.verifyOtp(formData.email, formData.otp);
    } else if (this.usePassword) {
      // Password Login
      this.loginWithPassword(formData.email, formData.password);
    } else {
      // Request OTP
      this.requestOtp(formData.email);
    }
  }

  requestOtp(email: string): void {
    // Simulate API call
    setTimeout(() => {
      console.log('OTP requested for:', email);
      this.isLoading = false;
      this.showOtpInput = true;
      this.startResendTimer();
      
      // Update validators
      this.loginForm.get('otp')?.setValidators([Validators.required]);
      this.loginForm.get('otp')?.updateValueAndValidity();
      
      // TODO: Replace with actual API call
      alert('OTP sent to your email! (Check console for demo)');
    }, 1500);
  }

  verifyOtp(email: string, otp: string): void {
    // Simulate API call
    setTimeout(() => {
      console.log('Verifying OTP:', { email, otp });
      this.isLoading = false;
      
      // TODO: Replace with actual API call
      if (otp === '123456') { // Demo OTP
        alert('Login successful!');
        this.router.navigate(['/']);
      } else {
        alert('Invalid OTP. Please try again.');
      }
    }, 1500);
  }

  loginWithPassword(email: string, password: string): void {
    // Simulate API call
    setTimeout(() => {
      console.log('Login with password:', { email, password });
      this.isLoading = false;
      
      // TODO: Replace with actual API call
      alert('Login successful!');
      this.router.navigate(['/']);
    }, 1500);
  }

  onSignup(): void {
    if (this.signupForm.invalid) return;

    this.isLoading = true;
    const formData = this.signupForm.value;

    // Simulate API call
    setTimeout(() => {
      console.log('Signup data:', formData);
      this.isLoading = false;
      
      // TODO: Replace with actual API call
      alert('Account created successfully! Please login.');
      this.switchTab('login');
    }, 1500);
  }

  resendOtp(): void {
    if (this.resendTimer > 0) return;
    
    const email = this.loginForm.get('email')?.value;
    this.requestOtp(email);
  }

  startResendTimer(): void {
    this.resendTimer = 60; // 60 seconds
    
    this.timerInterval = setInterval(() => {
      this.resendTimer--;
      if (this.resendTimer <= 0) {
        clearInterval(this.timerInterval);
      }
    }, 1000);
  }

  resetForms(): void {
    this.loginForm.reset();
    this.signupForm.reset();
    this.usePassword = false;
    this.showPassword = false;
    this.showOtpInput = false;
    this.isLoading = false;
    
    if (this.timerInterval) {
      clearInterval(this.timerInterval);
      this.resendTimer = 0;
    }
  }
}

