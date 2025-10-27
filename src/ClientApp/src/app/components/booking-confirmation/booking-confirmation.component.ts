import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BusReservationService, TicketDetails } from '../../services/bus-reservation.service';

@Component({
  selector: 'app-booking-confirmation',
  templateUrl: './booking-confirmation.component.html',
  styleUrls: ['./booking-confirmation.component.scss']
})
export class BookingConfirmationComponent implements OnInit {
  ticket: TicketDetails | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private busService: BusReservationService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const ticketId = params['ticketId'];
      if (ticketId) {
        this.loadTicketDetails(ticketId);
      } else {
        this.router.navigate(['/search']);
      }
    });
  }

  loadTicketDetails(ticketId: string): void {
    this.isLoading = true;
    this.busService.getTicketDetails(ticketId).subscribe({
      next: (ticket) => {
        this.ticket = ticket;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load ticket details.';
        this.isLoading = false;
        console.error('Error loading ticket:', error);
      }
    });
  }

  printTicket(): void {
    window.print();
  }

  newSearch(): void {
    this.busService.clearSelection();
    this.router.navigate(['/search']);
  }

  cancelBooking(): void {
    if (this.ticket && confirm('Are you sure you want to cancel this booking?')) {
      this.busService.cancelReservation(this.ticket.id).subscribe({
        next: () => {
          alert('Booking cancelled successfully.');
          localStorage.removeItem('currentTicketId');
          this.router.navigate(['/search']);
        },
        error: (error) => {
          console.error('Cancellation failed:', error);
          alert('Failed to cancel booking. Please try again.');
        }
      });
    }
  }
}
