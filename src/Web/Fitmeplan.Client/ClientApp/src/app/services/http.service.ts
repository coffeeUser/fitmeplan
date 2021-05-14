import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable()
export class HttpService {

  constructor(private http: HttpClient) { }

  getData(route: string) {
    return this.http.get(route);
  }

  postData(route: string, body: any) {
    return this.http.post(route, body);
  }
}
