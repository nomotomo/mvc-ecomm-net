import { HttpClient } from "@angular/common/http";
import { inject, Injectable, signal } from "@angular/core";
import { RegisterDto } from "./models/RegisterDto";
import { Observable } from "rxjs";
import { LoginDto } from "./models/LoginDto";
import {JwtPayload} from './models/JwtPayload';
import {jwtDecode} from 'jwt-decode';
import {ApiService} from '../core/services/api.service';

@Injectable({providedIn:'root'})
export class AuthService {
  private http = inject(HttpClient);
  private apiService = inject(ApiService);
  //private baseUrl = 'http://localhost:8010/identity/api/auth';
  private baseUrl = `${this.apiService.apiUrl}identity/api/auth`;

  //store token in a signal
  userToken = signal<string | null>(localStorage.getItem('token'));

  register(dto: RegisterDto): Observable<{message: string}>{
    return this.http.post<{message:string}>(`${this.baseUrl}/register`, dto);
  }

  login(dto: LoginDto): Observable<{token: string}>{
    return this.http.post<{token: string}>(`${this.baseUrl}/login`, dto);
  }

  saveToken(token: string){
    localStorage.setItem('token', token);
    this.userToken.set(token);
  }

  logout(){
    localStorage.removeItem('token');
    this.userToken.set(null);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  //get username - extract name/email from JWT
  getUserName(): string | null {
    const token = this.userToken();
    if (!token) return null;

    try {
      const decoded = jwtDecode<JwtPayload>(token);
      return (
        decoded.name ||
        decoded.unique_name ||
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        decoded.sub ||
        null
      );
    } catch {
      return null;
    }
  }
}
