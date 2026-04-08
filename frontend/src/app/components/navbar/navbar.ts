import { Component, OnInit, HostListener } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class Navbar implements OnInit {
  isLoggedIn = false;
  usuario: any = null;
  menuOpen = false;

  ngOnInit() {
    this.checkAuth();
  }

  @HostListener('window:storage')
  onStorageChange() {
    this.checkAuth();
  }

  checkAuth() {
    const userData = localStorage.getItem('usuario');
    if (userData) {
      this.isLoggedIn = true;
      this.usuario = JSON.parse(userData);
    } else {
      this.isLoggedIn = false;
      this.usuario = null;
    }
  }

  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }

  closeMenu() {
    this.menuOpen = false;
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
    this.isLoggedIn = false;
    this.usuario = null;
    this.menuOpen = false;
    window.location.href = '/presentacion';
  }
}
