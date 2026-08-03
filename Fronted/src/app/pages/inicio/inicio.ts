import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from "@angular/router";

@Component({
  selector: 'app-inicio',
  imports: [    RouterOutlet,
    RouterLink,
    RouterLinkActive],
  templateUrl: './inicio.html',
  styleUrl: './inicio.css',
})
export class Inicio {

}
