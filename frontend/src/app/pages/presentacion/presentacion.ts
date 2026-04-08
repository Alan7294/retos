import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-presentacion',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './presentacion.html',
  styleUrls: ['./presentacion.css']
})
export class Presentacion {}
