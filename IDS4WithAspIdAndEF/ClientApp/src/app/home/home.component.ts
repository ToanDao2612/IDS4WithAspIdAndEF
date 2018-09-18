import { Component } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

    constructor(private oauthService: OAuthService) {
    }

    public login() {
      this.oauthService.initImplicitFlow();
    }

    public logoff() {
      this.oauthService.logOut();
    }

    public get name() {
      let claims = this.oauthService.getIdentityClaims();
      if (!claims) return null;
      return claims[0];
    }
}
