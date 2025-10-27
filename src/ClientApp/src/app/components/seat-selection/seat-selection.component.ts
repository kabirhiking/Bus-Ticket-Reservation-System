import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BusReservationService, BusSchedule, SeatAvailability } from '../../services/bus-reservation.service';

interface Seat {
  number: string;
  status: 'available' | 'booked' | 'sold' | 'selected' | 'empty';
}

interface BoardingPoint {
  time: string;
  name: string;
}

@Component({
  selector: 'app-seat-selection',
  templateUrl: './seat-selection.component.html',
  styleUrls: ['./seat-selection.component.scss']
})
export class SeatSelectionComponent implements OnInit {
  selectedSchedule: BusSchedule | null = null;
  seatAvailability: SeatAvailability | null = null;
  selectedSeats: string[] = [];
  isLoading = false;
  scheduleId: number = 0;

  seatLayout: Seat[][] = [];
  
  boardingPoints: BoardingPoint[] = [
    { time: '08:00 AM', name: 'Kallyanpur counter' },
    { time: '10:30 AM', name: 'Baneshwore Counter' },
    { time: '12:30 PM', name: 'Rajshahi Counter' },
    { time: '01:00 PM', name: 'Rajshahi Counter' }
  ];

  droppingPoints: BoardingPoint[] = [
    { time: '10:30 AM', name: 'Baneshwore Counter' },
    { time: '12:30 PM', name: 'Rajshahi Counter' },
    { time: '01:00 PM', name: 'Rajshahi Counter' }
  ];

  bookingForm = {
    boardingPoint: '',
    droppingPoint: '',
    email: ''
  };

  baseFare = 700;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private busService: BusReservationService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.scheduleId = +params['scheduleId'] || +params['id'];
      this.selectedSchedule = this.busService.getSelectedSchedule();
      
      if (!this.selectedSchedule) {
        this.router.navigate(['/search']);
        return;
      }
      
      this.initializeSeatLayout();
    });
  }

  initializeSeatLayout(): void {
    // Create 10 rows x 4 columns seat layout (2 seats - aisle - 2 seats)
    this.seatLayout = [];
    let seatCounter = 1;
    
    for (let row = 0; row < 10; row++) {
      const seatRow: Seat[] = [];
      
      // Left 2 seats
      for (let i = 0; i < 2; i++) {
        let status: Seat['status'] = 'available';
        const rand = Math.random();
        if (rand < 0.15) status = 'booked';
        else if (rand < 0.25) status = 'sold';
        
        seatRow.push({ 
          number: `${String.fromCharCode(65 + i)}${row + 1}`, 
          status: status 
        });
      }
      
      // Aisle (empty spaces)
      seatRow.push({ number: '', status: 'empty' });
      seatRow.push({ number: '', status: 'empty' });
      
      // Right 2 seats
      for (let i = 2; i < 4; i++) {
        let status: Seat['status'] = 'available';
        const rand = Math.random();
        if (rand < 0.15) status = 'booked';
        else if (rand < 0.25) status = 'sold';
        
        seatRow.push({ 
          number: `${String.fromCharCode(65 + i)}${row + 1}`, 
          status: status 
        });
      }
      
      this.seatLayout.push(seatRow);
    }
  }

  toggleSeat(seat: Seat): void {
    if (!seat.number || seat.status === 'booked' || seat.status === 'sold') {
      return;
    }
    
    if (seat.status === 'selected') {
      seat.status = 'available';
      const index = this.selectedSeats.indexOf(seat.number);
      if (index > -1) {
        this.selectedSeats.splice(index, 1);
      }
    } else {
      seat.status = 'selected';
      this.selectedSeats.push(seat.number);
    }
  }

  calculateSeatFare(): number {
    return this.baseFare * this.selectedSeats.length;
  }

  isFormValid(): boolean {
    return this.bookingForm.boardingPoint !== '' &&
           this.bookingForm.droppingPoint !== '' &&
           this.bookingForm.email !== '' &&
           this.selectedSeats.length > 0;
  }

  submitBooking(): void {
    if (!this.isFormValid()) {
      return;
    }
    
    console.log('Booking submitted:', {
      ...this.bookingForm,
      selectedSeats: this.selectedSeats,
      totalFare: this.calculateSeatFare()
    });
    
    // Navigate to booking confirmation or next step
    this.router.navigate(['/booking-confirmation']);
  }
}
