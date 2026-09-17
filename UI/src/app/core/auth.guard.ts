import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";
export const authGuard: CanActivateFn = (route) => { const auth=inject(AuthService); const router=inject(Router); if(!auth.isAuthenticated()) return router.createUrlTree(["/login"]); const permission=route.data?.["permission"] as string|undefined; const accessType=(route.data?.["accessType"] as string|undefined)??"VIEW"; return !permission||auth.hasAccess(permission,accessType)?true:router.createUrlTree(["/dashboard/summary"]); };
