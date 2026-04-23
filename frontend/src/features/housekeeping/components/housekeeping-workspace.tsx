"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ClipboardList } from "lucide-react";
import { StatusBadge } from "@/components/shared/status-badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { SectionHeading } from "@/components/ui/section-heading";
import { Select } from "@/components/ui/select";
import {
  assignHousekeepingTask,
  getAssignableHousekeepers,
  getHousekeepingTasks,
  housekeepingQueryKey,
  updateHousekeepingTask,
} from "@/features/housekeeping/api";
import { useAuth } from "@/hooks/use-auth";
import { useToast } from "@/hooks/use-toast";
import { hasPermission } from "@/lib/permissions";
import { getApiErrorMessage } from "@/services/api-client";
import type { HousekeepingStatus } from "@/types/hotel";

const statusActions: HousekeepingStatus[] = [
  "clean",
  "dirty",
  "in-progress",
  "maintenance",
];

export function HousekeepingWorkspace() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const { addToast } = useToast();
  const canManageHousekeeping = user
    ? hasPermission(user.role, "housekeeping.manage")
    : false;

  const tasksQuery = useQuery({
    queryKey: housekeepingQueryKey,
    queryFn: getHousekeepingTasks,
  });

  const housekeepersQuery = useQuery({
    queryKey: ["housekeepers"],
    queryFn: getAssignableHousekeepers,
    enabled: canManageHousekeeping,
  });

  const updateMutation = useMutation({
    mutationFn: ({
      taskId,
      status,
      notes,
    }: {
      taskId: string;
      status: HousekeepingStatus;
      notes?: string;
    }) => updateHousekeepingTask(taskId, status, notes),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: housekeepingQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["notifications"] }),
        queryClient.invalidateQueries({ queryKey: ["rooms"] }),
      ]);
      addToast({
        title: "Housekeeping updated",
        description: "The task status was saved successfully.",
        tone: "success",
      });
    },
    onError: (error) => {
      addToast({
        title: "Unable to update housekeeping task",
        description: getApiErrorMessage(error, "Housekeeping update failed."),
        tone: "warning",
      });
    },
  });

  const assignMutation = useMutation({
    mutationFn: ({ taskId, staffUserId }: { taskId: string; staffUserId: string }) =>
      assignHousekeepingTask(taskId, staffUserId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: housekeepingQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
      ]);
      addToast({
        title: "Task assigned",
        description: "The housekeeping assignment was updated.",
        tone: "info",
      });
    },
    onError: (error) => {
      addToast({
        title: "Unable to assign task",
        description: getApiErrorMessage(error, "Assignment failed."),
        tone: "warning",
      });
    },
  });

  const tasks = tasksQuery.data ?? [];
  const summary = {
    clean: tasks.filter((task) => task.status === "clean").length,
    dirty: tasks.filter((task) => task.status === "dirty").length,
    "in-progress": tasks.filter((task) => task.status === "in-progress").length,
    maintenance: tasks.filter((task) => task.status === "maintenance").length,
  };

  if (tasksQuery.isLoading) {
    return <div className="py-16 text-sm text-muted-foreground">Loading housekeeping...</div>;
  }

  if (tasksQuery.isError) {
    return (
      <div className="rounded-3xl border border-accent-red/20 bg-accent-red/10 px-6 py-5 text-sm text-accent-red">
        {getApiErrorMessage(tasksQuery.error, "Unable to load housekeeping tasks.")}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Phase 10"
        title="Housekeeping"
        description="Track room readiness, update task status, and assign work using the live housekeeping endpoints."
      />

      <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-5">
        {Object.entries(summary).map(([label, value]) => (
          <Card key={label}>
            <CardContent className="pt-6">
              <p className="text-xs uppercase tracking-[0.22em] text-muted-foreground">
                {label.replaceAll("-", " ")}
              </p>
              <p className="mt-3 text-4xl font-bold text-white">{value}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      {tasks.length ? (
        <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
          {tasks.map((task) => (
            <Card key={task.id}>
              <CardHeader className="flex flex-row items-start justify-between gap-4">
                <div>
                  <CardTitle>{`Room ${task.roomNumber}`}</CardTitle>
                  <p className="mt-1 text-sm text-muted-foreground">Floor {task.floor}</p>
                </div>
                <StatusBadge status={task.status} />
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2 text-sm text-slate-200">
                  <p>Assigned: {task.assignedTo ?? "Unassigned"}</p>
                  <p>Last cleaned: {task.lastCleaned ?? "Pending update"}</p>
                </div>

                {task.priorityNote ? (
                  <div className="rounded-2xl bg-accent-orange/12 px-3 py-2 text-sm text-accent-orange">
                    {task.priorityNote}
                  </div>
                ) : null}

                {task.maintenanceNotes ? (
                  <div className="rounded-2xl bg-accent-red/10 px-3 py-2 text-sm text-accent-orange">
                    {task.maintenanceNotes}
                  </div>
                ) : null}

                {task.notes && !task.priorityNote && !task.maintenanceNotes ? (
                  <div className="rounded-2xl bg-white/4 px-3 py-2 text-sm text-slate-200">
                    {task.notes}
                  </div>
                ) : null}

                <div className="space-y-2">
                  <label className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                    Assign housekeeper
                  </label>
                  <Select
                    value=""
                    onChange={(event) => {
                      if (!event.target.value) return;
                      void assignMutation.mutateAsync({
                        taskId: task.id,
                        staffUserId: event.target.value,
                      });
                    }}
                    disabled={!canManageHousekeeping || assignMutation.isPending}
                  >
                    <option value="">
                      {task.assignedTo ? `Current: ${task.assignedTo}` : "Assign task"}
                    </option>
                    {(housekeepersQuery.data ?? []).map((h) => (
                      <option key={h.id} value={h.id}>
                        {h.fullName}
                      </option>
                    ))}
                  </Select>
                </div>

                <div className="flex flex-wrap gap-2">
                  {statusActions.map((status) => (
                    <Button
                      key={status}
                      variant={task.status === status ? "primary" : "secondary"}
                      size="sm"
                      onClick={() =>
                        void updateMutation.mutateAsync({
                          taskId: task.id,
                          status,
                          notes: task.notes ?? task.priorityNote ?? task.maintenanceNotes,
                        })
                      }
                      disabled={!canManageHousekeeping || updateMutation.isPending}
                    >
                      {status.replaceAll("-", " ")}
                    </Button>
                  ))}
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <CardContent className="flex min-h-72 flex-col items-center justify-center text-center">
            <ClipboardList className="size-10 text-accent-cyan" />
            <h2 className="mt-6 text-2xl font-bold text-white">No housekeeping tasks</h2>
            <p className="mt-3 max-w-md text-sm leading-6 text-muted-foreground">
              The backend did not return any housekeeping tasks.
            </p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
