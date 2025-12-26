import { Component, signal } from '@angular/core';
import { Navbar } from "./core/navbar/navbar";
import {Loading} from './core/loading/loading';
import {RouterOutlet} from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [Navbar, Loading, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('Client');
}
