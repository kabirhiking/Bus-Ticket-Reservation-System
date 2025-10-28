import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';

// API Response wrapper
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
  errors: string[];
}

// API Request/Response DTOs
interface SearchBusesApiRequest {
  fromCity: string;
  toCity: string;
  journeyDate: string;
  passengerCount: number;
}

interface BusScheduleInfo {
  scheduleId: string;
  busId: string;
  busName: string;
  companyName: string;
  busType: string;
  fromCity: string;
  toCity: string;
  departureTime: string;
  arrivalTime: string;
  price: number;
  currency: string;
  totalSeats: number;
  availableSeats: number;
  distance: number;
  duration: string;
}

interface SearchBusesApiResponse {
  availableBuses: BusScheduleInfo[];
  fromCity: string;
  toCity: string;
  journeyDate: string;
  searchResultCount: number;
}

// Interfaces matching the frontend models
export interface BusSchedule {
  id: number;
  routeId: number;
  busId: number;
  departureTime: string;
  arrivalTime: string;
  price: number;
  availableSeats: number;
  busNumber: string;
  busType: string;
  routeFrom: string;
  routeTo: string;
  // Additional UI properties
  operatorName?: string;
  hasOffer?: boolean;
  hasExtraCharge?: boolean;
  originalPrice?: number;
  savings?: number;
  hasAC?: boolean;
}

export interface SearchBusRequest {
  from: string;
  to: string;
  travelDate: string;
  passengers: number;
}

export interface BookReservationRequest {
  scheduleId: number;
  passengerName: string;
  passengerEmail: string;
  passengerPhone: string;
  seatNumbers: number[];
  totalAmount: number;
}

export interface TicketDetails {
  id: string;
  scheduleId: number;
  passengerName: string;
  passengerEmail: string;
  passengerPhone: string;
  seatNumbers: number[];
  totalAmount: number;
  bookingDate: string;
  status: string;
  busNumber: string;
  route: string;
  travelDate: string;
  departureTime: string;
}

export interface SeatAvailability {
  scheduleId: number;
  totalSeats: number;
  availableSeats: number[];
  bookedSeats: number[];
}

export interface BoardingPoint {
  id: number;
  name: string;
  address: string;
  landmarks: string;
}

export interface DroppingPoint {
  id: number;
  name: string;
  address: string;
  landmarks: string;
}

@Injectable({
  providedIn: 'root'
})
export class BusReservationService {
  private apiUrl = environment.apiUrl;
  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  // State management
  private selectedScheduleSubject = new BehaviorSubject<BusSchedule | null>(null);
  private selectedSeatsSubject = new BehaviorSubject<number[]>([]);

  public selectedSchedule$ = this.selectedScheduleSubject.asObservable();
  public selectedSeats$ = this.selectedSeatsSubject.asObservable();

  constructor(private http: HttpClient) { }

  // Search for available buses
  searchBuses(request: SearchBusRequest): Observable<BusSchedule[]> {
    const apiRequest: SearchBusesApiRequest = {
      fromCity: request.from,
      toCity: request.to,
      journeyDate: request.travelDate,
      passengerCount: request.passengers
    };

    return this.http.post<ApiResponse<SearchBusesApiResponse>>(
      `${this.apiUrl}/BusReservation/search`, 
      apiRequest, 
      this.httpOptions
    ).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error(response.message || 'Failed to search buses');
        }
        
        // Map API response to BusSchedule array
        return response.data.availableBuses.map((bus, index) => ({
          id: index + 1,
          routeId: index + 1,
          busId: index + 1,
          departureTime: new Date(bus.departureTime).toLocaleTimeString('en-US', { 
            hour: '2-digit', 
            minute: '2-digit',
            hour12: true 
          }),
          arrivalTime: new Date(bus.arrivalTime).toLocaleTimeString('en-US', { 
            hour: '2-digit', 
            minute: '2-digit',
            hour12: true 
          }),
          price: bus.price,
          availableSeats: bus.availableSeats,
          busNumber: bus.busName || `BUS-${index + 1}`,
          busType: bus.busType,
          routeFrom: bus.fromCity,
          routeTo: bus.toCity,
          operatorName: bus.companyName
        }));
      })
    );
  }

  // Book a reservation
  bookReservation(request: BookReservationRequest): Observable<TicketDetails> {
    return this.http.post<TicketDetails>(`${this.apiUrl}/BusReservation/book`, request, this.httpOptions);
  }

  // Get ticket details
  getTicketDetails(ticketId: string): Observable<TicketDetails> {
    return this.http.get<TicketDetails>(`${this.apiUrl}/BusReservation/ticket/${ticketId}`);
  }

  // Cancel reservation
  cancelReservation(ticketId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/BusReservation/cancel`, { ticketId }, this.httpOptions);
  }

  // Get seat availability
  getSeatAvailability(scheduleId: number): Observable<SeatAvailability> {
    return this.http.get<SeatAvailability>(`${this.apiUrl}/BusReservation/schedule/${scheduleId}/seats`);
  }

  // Get boarding points for a city
  getBoardingPoints(city: string): Observable<BoardingPoint[]> {
    return this.http.get<BoardingPoint[]>(`${this.apiUrl}/BusReservation/boarding-points/${city}`);
  }

  // Get dropping points for a city
  getDroppingPoints(city: string): Observable<DroppingPoint[]> {
    return this.http.get<DroppingPoint[]>(`${this.apiUrl}/BusReservation/dropping-points/${city}`);
  }

  // State management methods
  setSelectedSchedule(schedule: BusSchedule): void {
    this.selectedScheduleSubject.next(schedule);
  }

  getSelectedSchedule(): BusSchedule | null {
    return this.selectedScheduleSubject.value;
  }

  setSelectedSeats(seats: number[]): void {
    this.selectedSeatsSubject.next(seats);
  }

  getSelectedSeats(): number[] {
    return this.selectedSeatsSubject.value;
  }

  clearSelection(): void {
    this.selectedScheduleSubject.next(null);
    this.selectedSeatsSubject.next([]);
  }
}