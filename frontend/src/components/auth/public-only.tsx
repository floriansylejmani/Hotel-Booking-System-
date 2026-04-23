"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { PageLoader } from "@/components/shared/page-loader";
import { useAuth } from "@/hooks/use-auth";

export function PublicOnly({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { expiresAt, hasHydrated, isAuthenticated, logout } = useAuth();

  useEffect(() => {
    if (!hasHydrated) {
      return;
    }

    const isExpired = expiresAt ? new Date(expiresAt).getTime() <= Date.now() : false;

    if (isExpired) {
      logout();
      return;
    }

    if (isAuthenticated) {
      router.replace("/dashboard");
    }
  }, [expiresAt, hasHydrated, isAuthenticated, logout, router]);

  if (!hasHydrated) {
    return <PageLoader label="Preparing authentication..." />;
  }

  if (isAuthenticated) {
    return <PageLoader label="Redirecting to dashboard..." />;
  }

  return <>{children}</>;
}
