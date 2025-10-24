import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor() { }

  showSuccess(message: string): void {
    // You can integrate with a toast library like ngx-toastr here
    alert(`Success: ${message}`);
  }

  showError(message: string): void {
    // You can integrate with a toast library like ngx-toastr here
    alert(`Error: ${message}`);
  }

  showWarning(message: string): void {
    // You can integrate with a toast library like ngx-toastr here
    alert(`Warning: ${message}`);
  }

  showInfo(message: string): void {
    // You can integrate with a toast library like ngx-toastr here
    alert(`Info: ${message}`);
  }
}