import { Injectable, NgZone } from '@angular/core';
import { Router } from '@angular/router';

declare const Keycloak: any;

@Injectable({
  providedIn: 'root'
})
export class KeycloakService {
  private keycloak: any = null;
  private initialized = false;

  constructor(private router: Router, private ngZone: NgZone) {}

  async init(): Promise<boolean> {
    if (this.initialized) return true;

    try {
      if (typeof Keycloak === 'undefined') {
        console.error('Keycloak.js no está cargado');
        return false;
      }

      this.keycloak = new Keycloak({
        url: 'http://localhost:8889/',
        realm: 'upds-pw2',
        clientId: 'webapi-client'
      });

      await this.keycloak.init({
        onLoad: 'check-sso',
        checkLoginIframe: false,
        silentCheckSsoRedirectUri: window.location.origin + '/assets/silent-check-sso.html'
      });

      this.initialized = true;
      return true;
    } catch (error) {
      console.error('Error inicializando Keycloak:', error);
      return false;
    }
  }

  async login(): Promise<void> {
    if (!this.keycloak) {
      await this.init();
    }
    
    try {
      await this.keycloak.login({
        redirectUri: window.location.origin + '/retos'
      });
    } catch (error) {
      console.error('Error en login:', error);
    }
  }

  async logout(): Promise<void> {
    if (this.keycloak) {
      await this.keycloak.logout({
        redirectUri: window.location.origin + '/presentacion'
      });
    }
    localStorage.clear();
  }

  getToken(): string | null {
    return this.keycloak?.token || null;
  }

  isLoggedIn(): boolean {
    return !!this.keycloak?.token;
  }

  async refreshToken(): Promise<boolean> {
    if (!this.keycloak) return false;
    
    try {
      return await this.keycloak.updateToken(300);
    } catch {
      return false;
    }
  }
}
