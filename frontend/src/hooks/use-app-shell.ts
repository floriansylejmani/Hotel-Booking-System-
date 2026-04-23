import { useUiShellStore } from "@/store/ui-shell-store";

export function useAppShell() {
  const isSidebarOpen = useUiShellStore((state) => state.isSidebarOpen);
  const openSidebar = useUiShellStore((state) => state.openSidebar);
  const closeSidebar = useUiShellStore((state) => state.closeSidebar);
  const toggleSidebar = useUiShellStore((state) => state.toggleSidebar);

  return {
    isSidebarOpen,
    openSidebar,
    closeSidebar,
    toggleSidebar,
  };
}
