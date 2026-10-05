import { registerLocaleData } from "@angular/common";
import localePt from "@angular/common/locales/pt";
import { provideHttpClient } from "@angular/common/http";
import { bootstrapApplication } from "@angular/platform-browser";
import { AppComponent } from "./app/app.component";

registerLocaleData(localePt);

bootstrapApplication(AppComponent, {
  providers: [provideHttpClient()],
}).catch((erro) => console.error(erro));
