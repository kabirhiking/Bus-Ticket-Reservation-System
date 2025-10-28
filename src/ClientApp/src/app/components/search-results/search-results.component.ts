import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BusReservationService, BusSchedule, SearchBusRequest } from '../../services/bus-reservation.service';
import { MockDataService } from '../../services/mock-data.service';

interface Seat {
  number: string;
  status: 'available' | 'booked' | 'sold' | 'selected' | 'empty';
}

interface BoardingPoint {
  time: string;
  name: string;
}

@Component({
  selector: 'app-search-results',
  templateUrl: './search-results.component.html',
  styleUrls: ['./search-results.component.scss']
})
export class SearchResultsComponent implements OnInit {
  searchResults: BusSchedule[] = [];
  isLoading = false;
  searchCriteria: SearchBusRequest = {
    from: '',
    to: '',
    travelDate: '',
    passengers: 1
  };
  errorMessage = '';
  sortBy: string = 'departure';
  showModifyModal = false;
  showFilterPanel = false;
  selectedBusForSeats: BusSchedule | null = null;
  
  // Filter options
  filters = {
    busType: {
      ac: false,
      nonAc: false
    },
    busClass: {
      business: false,
      economy: false,
      sleeper: false
    },
    departureTime: {
      before4am: false,
      morning4to8: false,
      morning8to12: false,
      afternoon12to4: false,
      evening4to8: false,
      after8pm: false
    },
    operators: [] as string[]
  };
  
  availableOperators: string[] = [];
  modifyForm = {
    from: '',
    to: '',
    travelDate: '',
    returnDate: ''
  };
  
  // Seat selection
  seatLayout: Seat[][] = [];
  selectedSeats: string[] = [];
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
  
  // Toggle between mock data and real API
  useMockData = false; // Set to false when API is ready

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private busService: BusReservationService,
    private mockDataService: MockDataService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['from'] && params['to'] && params['date']) {
        this.searchCriteria = {
          from: params['from'],
          to: params['to'],
          travelDate: params['date'],
          passengers: parseInt(params['passengers']) || 1
        };
        this.searchBuses();
      } else {
        // If no search params, redirect to search page
        this.router.navigate(['/search']);
      }
    });
  }

  searchBuses(): void {
    this.isLoading = true;
    this.errorMessage = '';
    
    if (this.useMockData) {
      // Use mock data for development
      this.mockDataService.searchBuses(
        this.searchCriteria.from,
        this.searchCriteria.to,
        this.searchCriteria.travelDate
      ).subscribe({
        next: (results) => {
          this.searchResults = results;
          this.isLoading = false;
          console.log('Mock data loaded:', results.length, 'buses');
        },
        error: (error) => {
          this.errorMessage = 'Failed to load mock data';
          this.isLoading = false;
          console.error('Mock data error:', error);
        }
      });
    } else {
      // Use real API
      this.busService.searchBuses(this.searchCriteria).subscribe({
        next: (results) => {
          // Enhance results with mock data for UI purposes
          this.searchResults = results.map((bus, index) => ({
            ...bus,
            hasOffer: index % 3 === 1,
            hasExtraCharge: index % 2 === 0,
            originalPrice: bus.hasOffer ? bus.price + 100 : undefined,
            savings: bus.hasOffer ? 100 : undefined,
            hasAC: index % 2 === 1
          }));
          this.isLoading = false;
          this.extractAvailableOperators();
        },
        error: (error) => {
          this.errorMessage = 'Failed to search buses. Please try again.';
          this.isLoading = false;
          console.error('Search error:', error);
        }
      });
    }
  }

  getUniqueOperators(): number {
    const uniqueOperators = new Set(this.searchResults.map(bus => bus.operatorName));
    return uniqueOperators.size;
  }
  
  extractAvailableOperators(): void {
    const operators = new Set(this.searchResults.map(bus => bus.operatorName || bus.busNumber));
    this.availableOperators = Array.from(operators);
  }

  getTotalSeats(): number {
    return this.searchResults.reduce((total, bus) => total + bus.availableSeats, 0);
  }

  setSortBy(sortType: string): void {
    this.sortBy = sortType;
  }
  
  toggleFilterPanel(): void {
    this.showFilterPanel = !this.showFilterPanel;
  }
  
  clearFilters(): void {
    this.filters = {
      busType: {
        ac: false,
        nonAc: false
      },
      busClass: {
        business: false,
        economy: false,
        sleeper: false
      },
      departureTime: {
        before4am: false,
        morning4to8: false,
        morning8to12: false,
        afternoon12to4: false,
        evening4to8: false,
        after8pm: false
      },
      operators: []
    };
  }
  
  applyFilters(): void {
    this.showFilterPanel = false;
    // Filter logic will be applied in sortedResults()
  }
  
  toggleOperator(operator: string): void {
    const index = this.filters.operators.indexOf(operator);
    if (index > -1) {
      this.filters.operators.splice(index, 1);
    } else {
      this.filters.operators.push(operator);
    }
  }
  
  isOperatorSelected(operator: string): boolean {
    return this.filters.operators.includes(operator);
  }

  sortedResults(): BusSchedule[] {
    let results = [...this.searchResults];
    
    // Apply filters
    results = this.applyFiltersToResults(results);
    
    // Apply sorting
    switch(this.sortBy) {
      case 'departure':
        return results.sort((a, b) => a.departureTime.localeCompare(b.departureTime));
      case 'seats':
        return results.sort((a, b) => b.availableSeats - a.availableSeats);
      case 'fare':
        return results.sort((a, b) => a.price - b.price);
      case 'offers':
        return results.sort((a, b) => (b.hasOffer ? 1 : 0) - (a.hasOffer ? 1 : 0));
      default:
        return results;
    }
  }
  
  applyFiltersToResults(results: BusSchedule[]): BusSchedule[] {
    let filtered = results;
    
    // Filter by bus type (AC/Non-AC)
    if (this.filters.busType.ac || this.filters.busType.nonAc) {
      filtered = filtered.filter(bus => {
        if (this.filters.busType.ac && bus.hasAC) return true;
        if (this.filters.busType.nonAc && !bus.hasAC) return true;
        return false;
      });
    }
    
    // Filter by operators
    if (this.filters.operators.length > 0) {
      filtered = filtered.filter(bus => 
        this.filters.operators.includes(bus.operatorName || bus.busNumber)
      );
    }
    
    // Filter by departure time
    if (Object.values(this.filters.departureTime).some(v => v)) {
      filtered = filtered.filter(bus => {
        const hour = parseInt(bus.departureTime.split(':')[0]);
        
        if (this.filters.departureTime.before4am && hour < 4) return true;
        if (this.filters.departureTime.morning4to8 && hour >= 4 && hour < 8) return true;
        if (this.filters.departureTime.morning8to12 && hour >= 8 && hour < 12) return true;
        if (this.filters.departureTime.afternoon12to4 && hour >= 12 && hour < 16) return true;
        if (this.filters.departureTime.evening4to8 && hour >= 16 && hour < 20) return true;
        if (this.filters.departureTime.after8pm && hour >= 20) return true;
        
        return false;
      });
    }
    
    return filtered;
  }

  selectBus(schedule: BusSchedule): void {
    console.log('🔵 selectBus called');
    console.log('🔵 Schedule:', schedule);
    console.log('🔵 Schedule ID:', schedule.id);
    console.log('🔵 Current selectedBusForSeats:', this.selectedBusForSeats);
    
    if (this.selectedBusForSeats?.id === schedule.id) {
      // Toggle off if same bus clicked
      console.log('🔴 Toggling OFF');
      this.selectedBusForSeats = null;
      this.selectedSeats = [];
    } else {
      // Expand seat selection for this bus
      console.log('🟢 Expanding seat selection');
      this.selectedBusForSeats = schedule;
      console.log('🟢 selectedBusForSeats set to:', this.selectedBusForSeats);
      
      //Load real seat data from API
      this.loadSeatLayout(schedule);
    }
  }

  loadSeatLayout(schedule: BusSchedule): void {
    this.isLoading = true;
    
    if (this.useMockData) {
      // Use mock data
      this.initializeSeatLayout();
      this.resetBookingForm();
      this.isLoading = false;
    } else {
      // Load real seat data from API
      this.busService.getSeatAvailability(schedule.id).subscribe({
        next: (seatData: any) => {
          console.log('Seat availability data:', seatData);
          
          if (seatData.success && seatData.data) {
            // Map real seat data
            this.initializeSeatLayoutFromAPI(seatData.data.seats);
          } else {
            // Fallback to mock data if API fails
            this.initializeSeatLayout();
          }
          
          this.resetBookingForm();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading seat data:', error);
          // Fallback to mock data
          this.initializeSeatLayout();
          this.resetBookingForm();
          this.isLoading = false;
        }
      });
    }
  }

  initializeSeatLayoutFromAPI(seats: any[]): void {
    this.seatLayout = [];
    
    // Group seats by row
    const seatsByRow: { [key: string]: any[] } = {};
    
    seats.forEach(seat => {
      const row = seat.row || 'Row1';
      if (!seatsByRow[row]) {
        seatsByRow[row] = [];
      }
      seatsByRow[row].push(seat);
    });
    
    // Create seat layout with 4 columns (2 left + aisle + 2 right)
    Object.keys(seatsByRow).sort().forEach(rowKey => {
      const rowSeats = seatsByRow[rowKey].sort((a, b) => a.seatNumber.localeCompare(b.seatNumber));
      const seatRow: Seat[] = [];
      
      // Add first 2 seats
      for (let i = 0; i < 2 && i < rowSeats.length; i++) {
        const apiSeat = rowSeats[i];
        let status: Seat['status'] = 'available';
        
        if (!apiSeat.isAvailable) {
          status = apiSeat.isBooked ? 'booked' : (apiSeat.isSold ? 'sold' : 'booked');
        }
        
        seatRow.push({
          number: apiSeat.seatNumber,
          status: status
        });
      }
      
      // Add aisle (empty spaces)
      seatRow.push({ number: '', status: 'empty' });
      seatRow.push({ number: '', status: 'empty' });
      
      // Add remaining 2 seats
      for (let i = 2; i < 4 && i < rowSeats.length; i++) {
        const apiSeat = rowSeats[i];
        let status: Seat['status'] = 'available';
        
        if (!apiSeat.isAvailable) {
          status = apiSeat.isBooked ? 'booked' : (apiSeat.isSold ? 'sold' : 'booked');
        }
        
        seatRow.push({
          number: apiSeat.seatNumber,
          status: status
        });
      }
      
      this.seatLayout.push(seatRow);
    });
  }

  initializeSeatLayout(): void {
    this.seatLayout = [];
    
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

  resetBookingForm(): void {
    this.bookingForm = {
      boardingPoint: '',
      droppingPoint: '',
      email: ''
    };
    this.selectedSeats = [];
  }

  submitBooking(): void {
    if (!this.isFormValid()) {
      return;
    }
    
    console.log('Booking submitted:', {
      bus: this.selectedBusForSeats,
      ...this.bookingForm,
      selectedSeats: this.selectedSeats,
      totalFare: this.calculateSeatFare()
    });
    
    alert(`Booking confirmed!\nSeats: ${this.selectedSeats.join(', ')}\nTotal: ৳${this.calculateSeatFare()}`);
    this.selectedBusForSeats = null;
  }

  newSearch(): void {
    this.showModifyModal = true;
    this.modifyForm = {
      from: this.searchCriteria.from,
      to: this.searchCriteria.to,
      travelDate: this.searchCriteria.travelDate,
      returnDate: ''
    };
  }

  closeModal(): void {
    this.showModifyModal = false;
  }

  swapLocations(): void {
    const temp = this.modifyForm.from;
    this.modifyForm.from = this.modifyForm.to;
    this.modifyForm.to = temp;
  }

  performModifiedSearch(): void {
    this.searchCriteria = {
      from: this.modifyForm.from,
      to: this.modifyForm.to,
      travelDate: this.modifyForm.travelDate,
      passengers: this.searchCriteria.passengers
    };
    
    // Update URL query params without navigation
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        from: this.modifyForm.from,
        to: this.modifyForm.to,
        date: this.modifyForm.travelDate,
        passengers: this.searchCriteria.passengers
      },
      queryParamsHandling: 'merge'
    });
    
    this.closeModal();
    this.searchBuses();
  }
}
