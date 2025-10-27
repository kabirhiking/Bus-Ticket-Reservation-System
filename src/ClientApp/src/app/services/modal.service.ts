import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private loginModalSubject = new BehaviorSubject<boolean>(false);
  public loginModal$ = this.loginModalSubject.asObservable();

  constructor() { }

  openLoginModal() {
    this.loginModalSubject.next(true);
  }

  closeLoginModal() {
    this.loginModalSubject.next(false);
  }
}