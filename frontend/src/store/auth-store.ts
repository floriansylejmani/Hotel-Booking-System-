import { create } from "zustand";
import { createJSONStorage, persist } from "zustand/middleware";
import type { AuthSession, AuthUser } from "@/types/hotel";

type AuthStore = {
  user: AuthUser | null;
  token: string | null;
  expiresAt: string | null;
  hasHydrated: boolean;
  login: (session: AuthSession) => void;
  logout: () => void;
  setHydrated: (value: boolean) => void;
};

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      expiresAt: null,
      hasHydrated: false,
      login: (session) =>
        set({
          user: session.user,
          token: session.token,
          expiresAt: session.expiresAt,
        }),
      logout: () => set({ user: null, token: null, expiresAt: null }),
      setHydrated: (value) => set({ hasHydrated: value }),
    }),
    {
      name: "hbs-auth",
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        user: state.user,
        token: state.token,
        expiresAt: state.expiresAt,
      }),
      onRehydrateStorage: () => (state) => {
        state?.setHydrated(true);
      },
    },
  ),
);
