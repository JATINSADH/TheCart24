
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';

export const adminGuard: CanActivateFn = () => {
  const router = inject(Router);
  const token = sessionStorage.getItem('thecart24_token');

  if (!token) {
    return router.createUrlTree(['/login']);
  }

  try {
    const payloadPart = token.split('.')[1];
    const base64 = payloadPart
      .replace(/-/g, '+')
      .replace(/_/g, '/');

    const payload = JSON.parse(
      atob(base64.padEnd(
        base64.length + (4 - base64.length % 4) % 4,
        '='
      ))
    );

    const roleClaim =
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

    const roleValue =
      payload[roleClaim] ?? payload['role'] ?? payload['roles'];

    const roles: string[] = Array.isArray(roleValue)
      ? roleValue
      : roleValue
        ? [roleValue]
        : [];

    const isAdmin = roles.some(
      role => role.toLowerCase() === 'admin'
    );

    if (isAdmin) {
      return true;
    }

    return router.createUrlTree(['/']);
  } catch {
    sessionStorage.removeItem('thecart24_token');
    return router.createUrlTree(['/login']);
  }
};
