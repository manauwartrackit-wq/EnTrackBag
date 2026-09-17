import { HttpContextToken } from "@angular/common/http";
// Polling, automatic refresh and other non-user initiated requests must opt out.
export const BACKGROUND_REQUEST = new HttpContextToken<boolean>(() => false);
