"use client";

import { useEffect } from "react";
import { usePathname, useRouter } from "next/navigation";
import { PageLoader } from "@/components/shared/page-loader";
import { useAuth } from "@/hooks/use-auth";
import { getRequiredPermission, hasPermission } from "@/lib/permissions";

export function AuthGuard({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { expiresAt, hasHydrated, logout, token, user } = useAuth();
  const requiredPermission = user ? getRequiredPermission(pathname) : undefined;
  const canAccessRoute =
    user && requiredPermission ? hasPermission(user.role, requiredPermission) : true;

  useEffect(() => {
    if (!hasHydrated) {
      return;
    }

    const isExpired = expiresAt ? new Date(expiresAt).getTime() <= Date.now() : false;

    if (!user || !token || isExpired) {
      if (isExpired) {
        logout();
      }

      router.replace(`/login?redirect=${encodeURIComponent(pathname)}`);
      return;
    }

    if (!canAccessRoute) {
      router.replace("/dashboard");
    }
  }, [canAccessRoute, expiresAt, hasHydrated, logout, pathname, router, token, user]);

  if (!hasHydrated) {
    return <PageLoader label="Restoring session..." />;
  }

  if (!user || !token) {
    return <PageLoader label="Redirecting to login..." />;
  }

  if (!canAccessRoute) {
    return <PageLoader label="Checking permissions..." />;
  }

  return <>{children}</>;
}
