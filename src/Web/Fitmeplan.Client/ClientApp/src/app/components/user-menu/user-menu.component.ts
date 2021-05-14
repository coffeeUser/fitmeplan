import { Component } from '@angular/core';
import { HttpService } from '../../services/http.service';

@Component({
  selector: 'app-user-menu',
  templateUrl: './user-menu.component.html',
  styleUrls: ['./user-menu.component.css'],
  providers: [HttpService]
})
export class UserMenuComponent{

  constructor(private httpService: HttpService) { }

  logout() {
    this.httpService.getData("http://localhost:5002/user/logout");
  }
}
