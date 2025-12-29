import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private _apiUrl: string | null = null;

  get apiUrl(): string {
    if (this._apiUrl) return this._apiUrl;

    const hostname = window.location.hostname;
    this._apiUrl = hostname === 'localhost' || hostname === '127.0.0.1'
      ? 'http://localhost:8010/'
      : hostname.includes('dev') || hostname.includes('staging')
        ? 'https://api.dev.com/'
        : 'https://api.polluxkart.com/';

    return this._apiUrl;
  }
}

