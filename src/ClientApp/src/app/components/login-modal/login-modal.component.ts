import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ModalService } from '../../services/modal.service';
import { AuthService, LoginRequest, SignupRequest, VerifyOtpRequest, ResendOtpRequest } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-login-modal',
  templateUrl: './login-modal.component.html',
  styleUrls: ['./login-modal.component.scss']
})
export class LoginModalComponent implements OnInit, OnDestroy {
  isVisible = false;
  activeTab: 'login' | 'signup' = 'login';
  loginForm!: FormGroup;
  signupForm!: FormGroup;
  otpForm!: FormGroup;
  
  // State management
  showOtpInput = false;
  isLoading = false;
  otpPurpose: 'LOGIN' | 'SIGNUP' = 'LOGIN';
  currentEmail = '';
  otpTimer = 0;
  canResendOtp = true;
  
  private subscription!: Subscription;
  private timerSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private modalService: ModalService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {
    this.createForms();
  }

  ngOnInit(): void {
    this.subscription = this.modalService.loginModal$.subscribe(
      isOpen => {
        this.isVisible = isOpen;
        if (!isOpen) {
          this.resetState();
        }
      }
    );
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
    if (this.timerSubscription) {
      this.timerSubscription.unsubscribe();
    }
  }

  createForms() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.signupForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]]
    });

    this.otpForm = this.fb.group({
      otpCode: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    });
  }

  switchTab(tab: 'login' | 'signup') {
    this.activeTab = tab;
    this.resetState();
  }

  closeModal() {
    this.modalService.closeLoginModal();
    this.resetState();
  }

  resetState() {
    this.showOtpInput = false;
    this.isLoading = false;
    this.currentEmail = '';
    this.otpTimer = 0;
    this.canResendOtp = true;
    this.resetForms();
    this.stopTimer();
  }

  resetForms() {
    this.loginForm.reset();
    this.signupForm.reset();
    this.otpForm.reset();
  }

  onLogin() {
    if (this.loginForm.valid) {
      this.isLoading = true;
      const email = this.authService.formatEmail(this.loginForm.value.email);
      
      if (!this.authService.isValidEmail(email)) {
        this.notificationService.error('Please enter a valid email address');
        this.isLoading = false;
        return;
      }

      const request: LoginRequest = { email };
      
      this.authService.login(request).subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success && response.requiresOtp) {
            this.currentEmail = email;
            this.otpPurpose = 'LOGIN';
            this.showOtpInput = true;
            this.startOtpTimer();
            this.notificationService.success(response.message);
          } else {
            this.notificationService.error(response.message || 'Login failed');
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.notificationService.error(error.message || 'Login failed');
        }
      });
    }
  }

  onSignup() {
    if (this.signupForm.valid) {
      this.isLoading = true;
      const email = this.authService.formatEmail(this.signupForm.value.email);
      const fullName = this.signupForm.value.fullName.trim();
      
      if (!this.authService.isValidEmail(email)) {
        this.notificationService.error('Please enter a valid email address');
        this.isLoading = false;
        return;
      }

      const request: SignupRequest = { email, fullName };
      
      this.authService.signup(request).subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success && response.requiresOtp) {
            this.currentEmail = email;
            this.otpPurpose = 'SIGNUP';
            this.showOtpInput = true;
            this.startOtpTimer();
            this.notificationService.success(response.message);
          } else {
            this.notificationService.error(response.message || 'Signup failed');
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.notificationService.error(error.message || 'Signup failed');
        }
      });
    }
  }

  onVerifyOtp() {
    if (this.otpForm.valid && this.currentEmail) {
      this.isLoading = true;
      
      const request: VerifyOtpRequest = {
        email: this.currentEmail,
        otpCode: this.otpForm.value.otpCode,
        purpose: this.otpPurpose
      };
      
      this.authService.verifyOtp(request).subscribe({
        next: (response) => {
          this.isLoading = false;
          if (response.success && response.token) {
            this.notificationService.success(response.message || 'Authentication successful!');
            this.closeModal();
            // The AuthService already handles storing the token and user
          } else {
            this.notificationService.error(response.message || 'OTP verification failed');
          }
        },
        error: (error) => {
          this.isLoading = false;
          this.notificationService.error(error.message || 'OTP verification failed');
        }
      });
    }
  }

  onResendOtp() {
    if (!this.canResendOtp || !this.currentEmail) return;
    
    this.isLoading = true;
    
    const request: ResendOtpRequest = {
      email: this.currentEmail,
      purpose: this.otpPurpose
    };
    
    this.authService.resendOtp(request).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          this.notificationService.success(response.message);
          this.otpForm.reset();
          this.startOtpTimer();
        } else {
          this.notificationService.error(response.message || 'Failed to resend OTP');
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.notificationService.error(error.message || 'Failed to resend OTP');
      }
    });
  }

  private startOtpTimer() {
    this.otpTimer = 300; // 5 minutes
    this.canResendOtp = false;
    
    this.timerSubscription = setInterval(() => {
      this.otpTimer--;
      if (this.otpTimer <= 0) {
        this.canResendOtp = true;
        this.stopTimer();
      }
    }, 1000) as any;
  }

  private stopTimer() {
    if (this.timerSubscription) {
      clearInterval(this.timerSubscription as any);
      this.timerSubscription = undefined;
    }
  }

  getTimerDisplay(): string {
    const minutes = Math.floor(this.otpTimer / 60);
    const seconds = this.otpTimer % 60;
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  backToForm() {
    this.showOtpInput = false;
    this.stopTimer();
    this.otpForm.reset();
  }
}