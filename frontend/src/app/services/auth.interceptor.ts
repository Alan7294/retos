import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = localStorage.getItem('token');
    
    console.log('=== INTERCEPTOR ===');
    console.log('URL:', req.url);
    console.log('Método:', req.method);
    console.log('Token existe:', !!token);
    
    if (token) {
      console.log('Agregando token...');
      const cloned = req.clone({
        headers: req.headers.set('Authorization', `Bearer ${token}`)
      });
      return next.handle(cloned);
    }

    console.log('Sin token, pasando sin authorization');
    return next.handle(req);
  }
}