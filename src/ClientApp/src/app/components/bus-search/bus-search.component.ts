import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BusReservationService, SearchBusRequest } from '../../services/bus-reservation.service';

interface TrendingRoute {
  from: string;
  to: string;
}

@Component({
  selector: 'app-bus-search',
  templateUrl: './bus-search.component.html',
  styleUrls: ['./bus-search.component.scss']
})
export class BusSearchComponent implements OnInit {
  searchForm: FormGroup;
  isLoading = false;
  minDate: string;
  
  // City autocomplete
  allCities: string[] = [
    'Abdullahpur', 'Adabor', 'Aditimari', 'Agartala', 'Agrabad', 'Akhaura',
    'Bandarban', 'Barisal', 'Barguna', 'Bhairab', 'Bhola', 'Bogra', 'Brahmanbaria',
    'Chapainawabganj', 'Chittagong', 'Chuadanga', 'Comilla', 'Coxs-Bazar',
    'Dhaka', 'Dinajpur', 'Faridpur', 'Feni',
    'Gaibandha', 'Gazipur', 'Gopalganj', 'Habiganj',
    'Jamalpur', 'Jessore', 'Jhenaidah', 'Joypurhat',
    'Khagrachhari', 'Khulna', 'Kishoreganj', 'Kurigram', 'Kushtia',
    'Lakshmipur', 'Lalmonirhat', 'Madaripur', 'Magura', 'Manikganj', 'Maulvibazar', 'Meherpur', 'Munshiganj', 'Mymensingh',
    'Naogaon', 'Narail', 'Narayanganj', 'Narsingdi', 'Natore', 'Netrokona', 'Nilphamari', 'Noakhali',
    'Pabna', 'Panchagarh', 'Patuakhali', 'Pirojpur',
    'Rajbari', 'Rajshahi', 'Rangamati', 'Rangpur',
    'Satkhira', 'Shariatpur', 'Sherpur', 'Sirajganj', 'Sunamganj', 'Sylhet',
    'Tangail', 'Thakurgaon'
  ];
  
  filteredFromCities: string[] = [];
  filteredToCities: string[] = [];
  showFromDropdown = false;
  showToDropdown = false;
  
  trendingRoutes: TrendingRoute[] = [
    { from: 'Dhaka', to: 'Rajshahi' },
    { from: 'Dhaka', to: 'Barisal' },
    { from: 'Dhaka', to: 'Coxs-Bazar' },
    { from: 'Dhaka', to: 'Chittagong' },
    { from: 'Dhaka', to: 'Chapainawabganj' }
  ];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private busService: BusReservationService
  ) {
    // Set minimum date to today
    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];

    this.searchForm = this.fb.group({
      from: ['', [Validators.required, Validators.minLength(2)]],
      to: ['', [Validators.required, Validators.minLength(2)]],
      travelDate: ['', Validators.required],
      returnDate: [''],
      passengers: ['1', [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.setDefaultValues();
    
    // Subscribe to form control changes for autocomplete
    this.searchForm.get('from')?.valueChanges.subscribe(value => {
      this.filterFromCities(value);
    });
    
    this.searchForm.get('to')?.valueChanges.subscribe(value => {
      this.filterToCities(value);
    });
  }

  filterFromCities(searchTerm: string): void {
    if (!searchTerm || searchTerm.length < 1) {
      this.filteredFromCities = this.allCities.slice(0, 20); // Show first 20 cities when empty
      this.showFromDropdown = true;
      return;
    }
    
    const search = searchTerm.toLowerCase();
    this.filteredFromCities = this.allCities.filter(city =>
      city.toLowerCase().includes(search)
    );
    this.showFromDropdown = this.filteredFromCities.length > 0;
  }

  filterToCities(searchTerm: string): void {
    if (!searchTerm || searchTerm.length < 1) {
      this.filteredToCities = this.allCities.slice(0, 20); // Show first 20 cities when empty
      this.showToDropdown = true;
      return;
    }
    
    const search = searchTerm.toLowerCase();
    this.filteredToCities = this.allCities.filter(city =>
      city.toLowerCase().includes(search)
    );
    this.showToDropdown = this.filteredToCities.length > 0;
  }

  selectFromCity(city: string): void {
    this.searchForm.patchValue({ from: city });
    this.showFromDropdown = false;
  }

  selectToCity(city: string): void {
    this.searchForm.patchValue({ to: city });
    this.showToDropdown = false;
  }

  onFromFocus(): void {
    const currentValue = this.searchForm.get('from')?.value;
    if (currentValue) {
      this.filterFromCities(currentValue);
    } else {
      // Show all cities when field is empty
      this.filteredFromCities = this.allCities.slice(0, 20);
      this.showFromDropdown = true;
    }
  }

  onToFocus(): void {
    const currentValue = this.searchForm.get('to')?.value;
    if (currentValue) {
      this.filterToCities(currentValue);
    } else {
      // Show all cities when field is empty
      this.filteredToCities = this.allCities.slice(0, 20);
      this.showToDropdown = true;
    }
  }

  setDefaultValues(): void {
    // Set default values
    const today = new Date();
    this.searchForm.patchValue({
      from: 'Dhaka',
      to: 'Birampur',
      travelDate: today.toISOString().split('T')[0],
      passengers: '1'
    });
  }

  swapLocations(): void {
    const from = this.searchForm.get('from')?.value;
    const to = this.searchForm.get('to')?.value;
    
    this.searchForm.patchValue({
      from: to,
      to: from
    });
  }

  loadTrendingRoute(route: TrendingRoute): void {
    this.searchForm.patchValue({
      from: route.from,
      to: route.to
    });
  }

  onSearch(): void {
    console.log('Search button clicked!');
    console.log('Form valid:', this.searchForm.valid);
    console.log('Form values:', this.searchForm.value);
    
    if (this.searchForm.valid && !this.isLoading) {
      const searchRequest: SearchBusRequest = {
        from: this.searchForm.value.from,
        to: this.searchForm.value.to,
        travelDate: this.searchForm.value.travelDate,
        passengers: parseInt(this.searchForm.value.passengers) || 1
      };

      console.log('Navigating to results with:', searchRequest);

      // Navigate to results immediately - API call will happen there
      this.router.navigate(['/results'], {
        queryParams: {
          from: searchRequest.from,
          to: searchRequest.to,
          date: searchRequest.travelDate,
          passengers: searchRequest.passengers
        }
      }).then(success => {
        console.log('Navigation successful:', success);
      }).catch(error => {
        console.error('Navigation error:', error);
      });
    } else {
      console.log('Form is invalid, marking all fields as touched');
      // Mark all fields as touched to show validation errors
      this.markFormGroupTouched();
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.searchForm.controls).forEach(key => {
      const control = this.searchForm.get(key);
      control?.markAsTouched();
    });
  }
}