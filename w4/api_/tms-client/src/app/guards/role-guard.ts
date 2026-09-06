import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

export const roleGuard = (requireRole: string): CanActivateFn => {

  return async () => {

    const auth = inject(AuthService);
    const router = inject(Router);

    const user = auth.currentUser();

    if (!user) {
      return router.createUrlTree(["/login"]);
    }

    if (auth.hasRole(requireRole)) {
      return true;
    }

    return router.createUrlTree(["/unauthorized"]);
  };
};