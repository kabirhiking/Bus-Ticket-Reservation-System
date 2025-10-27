import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BusReservationService, BusSchedule, BookReservationRequest } from '../../services/bus-reservation.service';

@Component({
  selector: 'app-booking-form',
  templateUrl: './booking-form.component.html',
  styleUrls: ['./booking-form.component.scss']
})
export class BookingFormComponent implements OnInit {
  bookingForm: FormGroup;
  selectedSchedule: BusSchedule | null = null;
  selectedSeats: number[] = [];
  totalAmount: number = 0;
  isLoading = false;
  scheduleId: number = 0;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private busService: BusReservationService
  ) {
    this.bookingForm = this.fb.group({
      passengerName: ['', [Validators.required, Validators.minLength(2)]],
      passengerEmail: ['', [Validators.required, Validators.email]],
      passengerPhone: ['', [Validators.required, Validators.pattern(/^[0-9]{11}$/)]]
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.scheduleId = +params['scheduleId'];
      this.selectedSchedule = this.busService.getSelectedSchedule();
      this.selectedSeats = this.busService.getSelectedSeats();
      
      if (!this.selectedSchedule || this.selectedSeats.length === 0) {
        this.router.navigate(['/search']);
        return;
      }
      
      this.calculateTotal();
    });
  }

  calculateTotal(): void {
    if (this.selectedSchedule) {
      this.totalAmount = this.selectedSchedule.price * this.selectedSeats.length;
    }
  }

  onSubmit(): void {
    if (this.bookingForm.valid && this.selectedSchedule) {
      this.isLoading = true;
      
      const bookingRequest: BookReservationRequest = {
        scheduleId: this.scheduleId,
        passengerName: this.bookingForm.value.passengerName,
        passengerEmail: this.bookingForm.value.passengerEmail,
        passengerPhone: this.bookingForm.value.passengerPhone,
        seatNumbers: this.selectedSeats,
        totalAmount: this.totalAmount
      };

      this.busService.bookReservation(bookingRequest).subscribe({
        next: (ticket) => {
          this.isLoading = false;
          localStorage.setItem('currentTicketId', ticket.id);
          this.router.navigate(['/confirmation', ticket.id]);
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Booking failed:', error);
          alert('Booking failed. Please try again.');
        }
      });
    } else {
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.bookingForm.controls).forEach(key => {
      const control = this.bookingForm.get(key);
      control?.markAsTouched();
    });
  }

  goBack(): void {
    this.router.navigate(['/seats', this.scheduleId]);
  }
}
