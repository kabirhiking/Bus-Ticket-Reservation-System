import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Components
import { NavbarComponent } from './components/navbar/navbar.component';
import { FooterComponent } from './components/footer/footer.component';
import { BusSearchComponent } from './components/bus-search/bus-search.component';
import { SearchResultsComponent } from './components/search-results/search-results.component';
import { SeatSelectionComponent } from './components/seat-selection/seat-selection.component';
import { BookingFormComponent } from './components/booking-form/booking-form.component';
import { BookingConfirmationComponent } from './components/booking-confirmation/booking-confirmation.component';
import { LoadingSpinnerComponent } from './shared/loading-spinner/loading-spinner.component';

// Services
import { BusReservationService } from './services/bus-reservation.service';
import { NotificationService } from './services/notification.service';

@NgModule({
  declarations: [
    AppComponent,
    NavbarComponent,
    FooterComponent,
    BusSearchComponent,
    SearchResultsComponent,
    SeatSelectionComponent,
    BookingFormComponent,
    BookingConfirmationComponent,
    LoadingSpinnerComponent
  ],
  imports: [
    BrowserModule,
    CommonModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule,
    FormsModule,
    BrowserAnimationsModule
  ],
  providers: [
    BusReservationService,
    NotificationService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }