import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { BusSearchComponent } from './components/bus-search/bus-search.component';
import { SearchResultsComponent } from './components/search-results/search-results.component';
import { SeatSelectionComponent } from './components/seat-selection/seat-selection.component';
import { BookingFormComponent } from './components/booking-form/booking-form.component';
import { BookingConfirmationComponent } from './components/booking-confirmation/booking-confirmation.component';

const routes: Routes = [
  { path: '', redirectTo: '/search', pathMatch: 'full' },
  { path: 'search', component: BusSearchComponent },
  { path: 'results', component: SearchResultsComponent },
  { path: 'seats/:scheduleId', component: SeatSelectionComponent },
  { path: 'booking/:scheduleId', component: BookingFormComponent },
  { path: 'confirmation/:ticketId', component: BookingConfirmationComponent },
  { path: '**', redirectTo: '/search' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }