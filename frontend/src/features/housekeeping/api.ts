import { toHousekeepingStatus, toHousekeepingStatusEnum } from "@/lib/hotel-mappers";
import { apiClient, toQueryString } from "@/services/api-client";
import { getRooms } from "@/features/rooms/api";
import type { HousekeepingStatus, HousekeepingTask } from "@/types/hotel";

type HousekeepingTaskDto = {
  id: string;
  roomNumber: string;
  status: string;
  assignedTo?: string | null;
  notes: string;
  lastCleanedAt?: string | null;
};

type HousekeepingTasksResponseDto = {
  items: HousekeepingTaskDto[];
  totalCount: number;
  page: number;
  pageSize: number;
};

export const housekeepingQueryKey = ["housekeeping"] as const;

function mapTask(
  dto: HousekeepingTaskDto,
  floorByRoom: Map<string, number>,
): HousekeepingTask {
  const status = toHousekeepingStatus(dto.status);
  return {
    id: dto.id,
    roomNumber: dto.roomNumber,
    floor: floorByRoom.get(dto.roomNumber) ?? 0,
    assignedTo: dto.assignedTo ?? undefined,
    lastCleaned: dto.lastCleanedAt ?? undefined,
    status,
    notes: dto.notes,
    maintenanceNotes: status === "maintenance" ? dto.notes : undefined,
    priorityNote: status !== "maintenance" && dto.notes ? dto.notes : undefined,
  };
}

export async function getHousekeepingTasks() {
  const [tasks, rooms] = await Promise.all([
    apiClient<HousekeepingTasksResponseDto>(
      `/housekeeping${toQueryString({ page: 1, pageSize: 100 })}`,
    ),
    getRooms({ pageSize: 100 }),
  ]);

  const floorByRoom = new Map(
    rooms.items.map((room) => [room.roomNumber, room.floor]),
  );

  return tasks.items.map((task) => mapTask(task, floorByRoom));
}

export async function updateHousekeepingTask(
  taskId: string,
  status: HousekeepingStatus,
  notes?: string,
) {
  return apiClient<HousekeepingTaskDto>(`/housekeeping/${taskId}`, {
    method: "PUT",
    body: {
      status: toHousekeepingStatusEnum(status),
      notes,
    },
  });
}

export async function assignHousekeepingTask(taskId: string, staffUserId: string) {
  return apiClient<HousekeepingTaskDto>(`/housekeeping/${taskId}/assign`, {
    method: "PUT",
    body: {
      staffUserId,
    },
  });
}

export type HousekeeperOption = {
  id: string;
  fullName: string;
};

export async function getAssignableHousekeepers(): Promise<HousekeeperOption[]> {
  return apiClient<HousekeeperOption[]>("/staff/housekeepers");
}
