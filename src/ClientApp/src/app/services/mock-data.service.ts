import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { BusSchedule } from './bus-reservation.service';

@Injectable({
  providedIn: 'root'
})
export class MockDataService {
  
  private mockBuses: BusSchedule[] = [
    {
      id: 1,
      routeId: 1,
      busId: 1,
      departureTime: '7:30 AM',
      arrivalTime: '3:30 PM',
      price: 750,
      availableSeats: 36,
      busNumber: '2001-FUL-DIN',
      busType: 'AC',
      routeFrom: 'Asadgate',
      routeTo: 'Dinajpur 01 No',
      operatorName: 'Shyamoli NR Travels',
      hasOffer: false,
      hasExtraCharge: true,
      hasAC: false
    },
    {
      id: 2,
      routeId: 1,
      busId: 2,
      departureTime: '10:00 AM',
      arrivalTime: '7:15 PM',
      price: 800,
      availableSeats: 40,
      busNumber: 'AC-300 DHK-DIN',
      busType: 'AC (FULBARI)',
      routeFrom: 'Kallyanpur',
      routeTo: 'Dinajpur',
      operatorName: 'Ahad Enterprise',
      hasOffer: true,
      hasExtraCharge: false,
      originalPrice: 900,
      savings: 100,
      hasAC: true
    },
    {
      id: 3,
      routeId: 1,
      busId: 3,
      departureTime: '9:00 AM',
      arrivalTime: '5:30 PM',
      price: 720,
      availableSeats: 28,
      busNumber: 'GL-500',
      busType: 'AC Sleeper',
      routeFrom: 'Gabtoli',
      routeTo: 'Birampur',
      operatorName: 'Green Line Paribahan',
      hasOffer: false,
      hasExtraCharge: true,
      hasAC: true
    },
    {
      id: 4,
      routeId: 1,
      busId: 4,
      departureTime: '11:30 PM',
      arrivalTime: '7:00 AM',
      price: 850,
      availableSeats: 8,
      busNumber: 'HE-700',
      busType: 'AC Sleeper Coach',
      routeFrom: 'Mohakhali',
      routeTo: 'Rangpur',
      operatorName: 'Hanif Enterprise',
      hasOffer: true,
      hasExtraCharge: false,
      originalPrice: 950,
      savings: 100,
      hasAC: true
    },
    {
      id: 5,
      routeId: 1,
      busId: 5,
      departureTime: '8:00 PM',
      arrivalTime: '4:30 AM',
      price: 680,
      availableSeats: 32,
      busNumber: 'ENA-400',
      busType: 'Non-AC',
      routeFrom: 'Sayedabad',
      routeTo: 'Rajshahi',
      operatorName: 'Ena Transport',
      hasOffer: false,
      hasExtraCharge: true,
      hasAC: false
    },
    {
      id: 6,
      routeId: 2,
      busId: 6,
      departureTime: '6:30 AM',
      arrivalTime: '2:00 PM',
      price: 790,
      availableSeats: 42,
      busNumber: 'SHO-600',
      busType: 'AC Business',
      routeFrom: 'Dhaka',
      routeTo: 'Chittagong',
      operatorName: 'Shohagh Paribahan',
      hasOffer: true,
      hasExtraCharge: false,
      originalPrice: 890,
      savings: 100,
      hasAC: true
    },
    {
      id: 7,
      routeId: 2,
      busId: 7,
      departureTime: '10:30 PM',
      arrivalTime: '6:00 AM',
      price: 950,
      availableSeats: 5,
      busNumber: 'TR-800',
      busType: 'AC Luxury',
      routeFrom: 'Dhaka',
      routeTo: "Cox's Bazar",
      operatorName: 'TR Travels',
      hasOffer: false,
      hasExtraCharge: true,
      hasAC: true
    }
  ];

  constructor() { }

  /**
   * Mock search buses with delay to simulate API call
   */
  searchBuses(from: string, to: string, date: string): Observable<BusSchedule[]> {
    // Filter buses based on route
    const filteredBuses = this.mockBuses.filter(bus => 
      bus.routeFrom.toLowerCase().includes(from.toLowerCase()) ||
      bus.routeTo.toLowerCase().includes(to.toLowerCase()) ||
      from.toLowerCase().includes('dhaka') ||
      to.toLowerCase().includes('birampur')
    );

    // Return with delay to simulate network call
    return of(filteredBuses).pipe(delay(800));
  }

  /**
   * Get all mock buses
   */
  getAllBuses(): Observable<BusSchedule[]> {
    return of(this.mockBuses).pipe(delay(500));
  }

  /**
   * Get bus by ID
   */
  getBusById(id: number): Observable<BusSchedule | undefined> {
    const bus = this.mockBuses.find(b => b.id === id);
    return of(bus).pipe(delay(300));
  }

  /**
   * Mock seat availability
   */
  getMockSeats(totalSeats: number, availableSeats: number) {
    const seats = [];
    for (let i = 1; i <= totalSeats; i++) {
      seats.push({
        seatNumber: i,
        isAvailable: i <= availableSeats,
        isSelected: false
      });
    }
    return seats;
  }
}
