import { Badge } from "@/components/ui/badge";

type StatusValue =
  | "available"
  | "occupied"
  | "reserved"
  | "maintenance"
  | "cleaning"
  | "active"
  | "confirmed"
  | "checked-in"
  | "checked-out"
  | "pending"
  | "cancelled"
  | "clean"
  | "dirty"
  | "in-progress";

type StatusBadgeProps = {
  status: StatusValue;
};

const statusVariantMap: Record<StatusValue, "green" | "blue" | "orange" | "red" | "indigo" | "neutral"> =
  {
    available: "green",
    occupied: "red",
    reserved: "indigo",
    maintenance: "neutral",
    cleaning: "orange",
    active: "blue",
    confirmed: "indigo",
    "checked-in": "green",
    "checked-out": "neutral",
    pending: "orange",
    cancelled: "red",
    clean: "green",
    dirty: "red",
    "in-progress": "orange",
  };

export function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <Badge variant={statusVariantMap[status]} size="sm">
      {status.replaceAll("-", " ")}
    </Badge>
  );
}
