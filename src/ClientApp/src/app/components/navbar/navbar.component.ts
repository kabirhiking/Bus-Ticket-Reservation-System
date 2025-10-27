import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs';
import { ModalService } from '../../services/modal.service';
import { AuthService, User } from '../../services/auth.service';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit, OnDestroy {
  currentTicketId: string | null = null;
  currentUser: User | null = null;
  isLoggingOut = false;
  
  private authSubscription!: Subscription;

  constructor(
    private modalService: ModalService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    // Get current ticket ID from localStorage if exists
    this.currentTicketId = localStorage.getItem('currentTicketId');
    
    // Subscribe to authentication state
    this.authSubscription = this.authService.currentUser.subscribe(
      user => {
        this.currentUser = user;
      }
    );
  }

  ngOnDestroy(): void {
    if (this.authSubscription) {
      this.authSubscription.unsubscribe();
    }
  }

  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated;
  }

  get userDisplayName(): string {
    if (this.currentUser?.fullName) {
      return this.currentUser.fullName;
    }
    return this.currentUser?.email?.split('@')[0] || 'User';
  }

  openLoginModal() {
    this.modalService.openLoginModal();
  }

  logout() {
    if (this.isLoggingOut) return;
    
    this.isLoggingOut = true;
    
    this.authService.logout().subscribe({
      next: () => {
        this.isLoggingOut = false;
        this.notificationService.success('Logged out successfully');
      },
      error: (error) => {
        this.isLoggingOut = false;
        // Even if server logout fails, we still logged out locally
        this.notificationService.success('Logged out successfully');
      }
    });
  }

  getUserInitials(): string {
    if (this.currentUser?.fullName) {
      const names = this.currentUser.fullName.split(' ');
      if (names.length >= 2) {
        return (names[0][0] + names[1][0]).toUpperCase();
      }
      return names[0][0].toUpperCase();
    }
    return this.currentUser?.email?.[0].toUpperCase() || 'U';
  }
}