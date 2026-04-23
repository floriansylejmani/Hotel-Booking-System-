import { create } from "zustand";
import type { NotificationTone } from "@/types/hotel";

export type ToastItem = {
  id: string;
  title: string;
  description?: string;
  tone: NotificationTone;
};

type ToastStore = {
  toasts: ToastItem[];
  addToast: (toast: Omit<ToastItem, "id">) => void;
  dismissToast: (id: string) => void;
};

export const useToastStore = create<ToastStore>((set) => ({
  toasts: [],
  addToast: (toast) =>
    set((state) => ({
      toasts: [
        ...state.toasts,
        { id: `toast-${crypto.randomUUID()}`, ...toast },
      ],
    })),
  dismissToast: (id) =>
    set((state) => ({
      toasts: state.toasts.filter((toast) => toast.id !== id),
    })),
}));
