import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-values',
  templateUrl: './values.component.html'
})
export class ValuesComponent {
  public values: SecurityValue[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<SecurityValue[]>(baseUrl + 'api/values').subscribe(result => {
      this.values = result;
    }, error => console.error(error));
  }
}

interface SecurityValue {
  value: string;
}
