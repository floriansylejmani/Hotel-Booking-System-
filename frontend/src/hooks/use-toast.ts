import { useToastStore } from "@/store/toast-store";

export function useToast() {
  const addToast = useToastStore((state) => state.addToast);
  const dismissToast = useToastStore((state) => state.dismissToast);
  const toasts = useToastStore((state) => state.toasts);

  return {
    addToast,
    dismissToast,
    toasts,
  };
}
