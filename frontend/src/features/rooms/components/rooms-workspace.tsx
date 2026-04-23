"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { EmptyState } from "@/components/shared/empty-state";
import { StatusBadge } from "@/components/shared/status-badge";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Modal } from "@/components/ui/modal";
import { SectionHeading } from "@/components/ui/section-heading";
import { Select } from "@/components/ui/select";
import {
  createRoom,
  deleteRoom,
  getAllAmenities,
  getRooms,
  roomsQueryKey,
  supportedRoomStatuses,
  supportedRoomTypes,
  updateRoom,
} from "@/features/rooms/api";
import { useAuth } from "@/hooks/use-auth";
import { useToast } from "@/hooks/use-toast";
import { formatCurrency } from "@/lib/format";
import { hasPermission } from "@/lib/permissions";
import { getApiErrorMessage } from "@/services/api-client";
import type { Room, RoomStatus, RoomType } from "@/types/hotel";

const roomSchema = z.object({
  roomNumber: z.string().min(1, "Room number is required."),
  type: z.enum(["Standard", "Deluxe", "Family", "Business Suite"]),
  pricePerNight: z.coerce.number().min(1, "Nightly rate is required."),
  floor: z.coerce.number().min(1, "Floor must be at least 1."),
  beds: z.coerce.number().min(1, "Bed count must be at least 1."),
  status: z.enum(["available", "occupied", "cleaning", "maintenance"]),
  amenities: z.string().min(2, "Provide at least one amenity."),
});

type RoomFormValues = z.infer<typeof roomSchema>;
type RoomFormInput = z.input<typeof roomSchema>;

const roomTypes: Array<"All Types" | RoomType> = ["All Types", ...supportedRoomTypes];
const roomStatuses: Array<"All Status" | RoomStatus> = [
  "All Status",
  ...supportedRoomStatuses,
];

export function RoomsWorkspace() {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const { addToast } = useToast();
  const [roomTypeFilter, setRoomTypeFilter] = useState<(typeof roomTypes)[number]>(
    "All Types",
  );
  const [statusFilter, setStatusFilter] = useState<(typeof roomStatuses)[number]>(
    "All Status",
  );
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRoom, setEditingRoom] = useState<Room | null>(null);

  const canManageRooms = user ? hasPermission(user.role, "rooms.manage") : false;
  const roomsQuery = useQuery({
    queryKey: [...roomsQueryKey, roomTypeFilter, statusFilter],
    queryFn: () =>
      getRooms({
        type: roomTypeFilter === "All Types" ? undefined : roomTypeFilter,
        status: statusFilter === "All Status" ? undefined : statusFilter,
        pageSize: 100,
      }),
  });

  const createMutation = useMutation({
    mutationFn: createRoom,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: roomsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["housekeeping"] }),
      ]);
      addToast({
        title: "Room added",
        description: "The room inventory has been updated from the backend.",
        tone: "success",
      });
      setIsModalOpen(false);
    },
    onError: (error) => {
      addToast({
        title: "Unable to add room",
        description: getApiErrorMessage(error, "Room creation failed."),
        tone: "warning",
      });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, values }: { id: string; values: RoomFormValues }) =>
      updateRoom(id, toSaveRoomInput(values)),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: roomsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["housekeeping"] }),
      ]);
      addToast({
        title: "Room updated",
        description: "The room details were saved successfully.",
        tone: "success",
      });
      setIsModalOpen(false);
      setEditingRoom(null);
    },
    onError: (error) => {
      addToast({
        title: "Unable to update room",
        description: getApiErrorMessage(error, "Room update failed."),
        tone: "warning",
      });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteRoom,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: roomsQueryKey }),
        queryClient.invalidateQueries({ queryKey: ["dashboard"] }),
        queryClient.invalidateQueries({ queryKey: ["housekeeping"] }),
      ]);
      addToast({
        title: "Room removed",
        description: "The room was removed from inventory.",
        tone: "warning",
      });
    },
    onError: (error) => {
      addToast({
        title: "Unable to remove room",
        description: getApiErrorMessage(error, "Room deletion failed."),
        tone: "warning",
      });
    },
  });

  const form = useForm<RoomFormInput, unknown, RoomFormValues>({
    resolver: zodResolver(roomSchema),
    defaultValues: {
      roomNumber: "",
      type: "Standard",
      pricePerNight: 120,
      floor: 1,
      beds: 1,
      status: "available",
      amenities: "WiFi, TV",
    },
  });

  const rooms = roomsQuery.data?.items ?? [];
  const amenitySuggestions = getAllAmenities(rooms);

  const openCreateModal = () => {
    form.reset({
      roomNumber: "",
      type: "Standard",
      pricePerNight: 120,
      floor: 1,
      beds: 1,
      status: "available",
      amenities: "WiFi, TV",
    });
    setEditingRoom(null);
    setIsModalOpen(true);
  };

  const openEditModal = (room: Room) => {
    form.reset({
      roomNumber: room.roomNumber,
      type: room.type === "Presidential Suite" ? "Business Suite" : room.type,
      pricePerNight: room.pricePerNight,
      floor: room.floor,
      beds: room.beds,
      status: room.status === "reserved" ? "available" : room.status,
      amenities: room.amenities.join(", "),
    });
    setEditingRoom(room);
    setIsModalOpen(true);
  };

  const onSubmit = form.handleSubmit(async (values) => {
    if (editingRoom) {
      await updateMutation.mutateAsync({
        id: editingRoom.id,
        values,
      });
      return;
    }

    await createMutation.mutateAsync(toSaveRoomInput(values));
  });

  if (roomsQuery.isLoading) {
    return <div className="py-16 text-sm text-muted-foreground">Loading rooms...</div>;
  }

  if (roomsQuery.isError) {
    return (
      <div className="rounded-[1.5rem] border border-accent-red/20 bg-accent-red/10 px-6 py-5 text-sm text-accent-red">
        {getApiErrorMessage(roomsQuery.error, "Unable to load rooms.")}
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <SectionHeading
        eyebrow="Phase 10"
        title="Room Management"
        description="Filter live hotel inventory by type and status, then add, edit, or remove rooms using the backend room API."
        actions={
          <div className="flex items-center gap-3">
            <Badge variant="neutral" size="md">
              {roomsQuery.data?.totalCount ?? rooms.length} rooms
            </Badge>
            <Button onClick={openCreateModal} disabled={!canManageRooms}>
              <Plus className="size-4" />
              Add Room
            </Button>
          </div>
        }
      />

      <div className="grid gap-4 xl:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
        <Card>
          <CardContent className="flex flex-wrap gap-2 pt-6">
            {roomTypes.map((type) => (
              <button
                key={type}
                type="button"
                onClick={() => setRoomTypeFilter(type)}
                className={`rounded-full border px-4 py-2 text-sm font-semibold transition ${
                  roomTypeFilter === type
                    ? "border-accent-indigo/35 bg-accent-indigo/16 text-white"
                    : "border-surface-border bg-white/[0.03] text-muted-foreground hover:text-white"
                }`}
              >
                {type}
              </button>
            ))}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex flex-wrap gap-2 pt-6">
            {roomStatuses.map((status) => (
              <button
                key={status}
                type="button"
                onClick={() => setStatusFilter(status)}
                className={`rounded-full border px-4 py-2 text-sm font-semibold transition ${
                  statusFilter === status
                    ? "border-accent-blue/35 bg-accent-blue/16 text-white"
                    : "border-surface-border bg-white/[0.03] text-muted-foreground hover:text-white"
                }`}
              >
                {status === "All Status" ? status : status.replaceAll("-", " ")}
              </button>
            ))}
          </CardContent>
        </Card>
      </div>

      {rooms.length ? (
        <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-4">
          {rooms.map((room) => (
            <Card key={room.id} className="overflow-hidden">
              <CardHeader>
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <CardTitle>{`Room ${room.roomNumber}`}</CardTitle>
                    <p className="mt-1 text-sm font-semibold text-accent-cyan">
                      {room.type}
                    </p>
                  </div>
                  <StatusBadge status={room.status} />
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center gap-4 text-sm text-slate-200">
                  <div>
                    <p className="text-2xl font-bold text-accent-green">
                      {formatCurrency(room.pricePerNight)}
                    </p>
                    <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      per night
                    </p>
                  </div>
                  <div>
                    <p className="text-xl font-bold">Floor {room.floor}</p>
                    <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      level
                    </p>
                  </div>
                  <div>
                    <p className="text-xl font-bold">{room.beds}</p>
                    <p className="text-xs uppercase tracking-[0.2em] text-muted-foreground">
                      beds
                    </p>
                  </div>
                </div>

                <div className="flex flex-wrap gap-2">
                  {room.amenities.map((amenity) => (
                    <Badge key={`${room.id}-${amenity}`} variant="neutral" size="sm">
                      {amenity}
                    </Badge>
                  ))}
                </div>

                <div className="flex gap-3">
                  <Button
                    variant="secondary"
                    className="flex-1"
                    onClick={() => openEditModal(room)}
                    disabled={!canManageRooms}
                  >
                    Edit
                  </Button>
                  <Button
                    variant="danger"
                    className="flex-1"
                    onClick={() => void deleteMutation.mutateAsync(room.id)}
                    disabled={!canManageRooms || deleteMutation.isPending}
                  >
                    Remove
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <EmptyState
          icon={Plus}
          title="No rooms found"
          description="Adjust the active filters or add a new room to populate inventory."
        />
      )}

      <Modal
        open={isModalOpen}
        onClose={() => {
          setIsModalOpen(false);
          setEditingRoom(null);
        }}
        title={editingRoom ? "Edit room" : "Add room"}
        description="Update room metadata and operational status."
        footer={
          <div className="flex justify-end gap-3">
            <Button
              variant="ghost"
              onClick={() => {
                setIsModalOpen(false);
                setEditingRoom(null);
              }}
            >
              Cancel
            </Button>
            <Button
              onClick={() => void onSubmit()}
              disabled={!canManageRooms || createMutation.isPending || updateMutation.isPending}
            >
              {createMutation.isPending || updateMutation.isPending
                ? "Saving..."
                : "Save Room"}
            </Button>
          </div>
        }
      >
        <div className="grid gap-4 md:grid-cols-2">
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Room number
            </label>
            <Input {...form.register("roomNumber")} />
            {form.formState.errors.roomNumber ? (
              <p className="text-sm text-accent-red">
                {form.formState.errors.roomNumber.message}
              </p>
            ) : null}
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Type
            </label>
            <Select {...form.register("type")}>
              {supportedRoomTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </Select>
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Price per night
            </label>
            <Input type="number" {...form.register("pricePerNight")} />
            {form.formState.errors.pricePerNight ? (
              <p className="text-sm text-accent-red">
                {form.formState.errors.pricePerNight.message}
              </p>
            ) : null}
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Status
            </label>
            <Select {...form.register("status")}>
              {supportedRoomStatuses.map((status) => (
                <option key={status} value={status}>
                  {status.replaceAll("-", " ")}
                </option>
              ))}
            </Select>
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Floor
            </label>
            <Input type="number" {...form.register("floor")} />
            {form.formState.errors.floor ? (
              <p className="text-sm text-accent-red">
                {form.formState.errors.floor.message}
              </p>
            ) : null}
          </div>
          <div className="space-y-2">
            <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Beds
            </label>
            <Input type="number" {...form.register("beds")} />
            {form.formState.errors.beds ? (
              <p className="text-sm text-accent-red">
                {form.formState.errors.beds.message}
              </p>
            ) : null}
          </div>
        </div>

        <div className="mt-4 space-y-2">
          <label className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
            Amenities
          </label>
          <Input {...form.register("amenities")} />
          {form.formState.errors.amenities ? (
            <p className="text-sm text-accent-red">
              {form.formState.errors.amenities.message}
            </p>
          ) : null}
          <p className="text-xs text-muted-foreground">
            Known tags: {amenitySuggestions.join(", ")}
          </p>
        </div>
      </Modal>
    </div>
  );
}

function toSaveRoomInput(values: RoomFormValues) {
  return {
    roomNumber: values.roomNumber,
    type: values.type,
    pricePerNight: values.pricePerNight,
    floor: values.floor,
    beds: values.beds,
    status: values.status,
    amenities: values.amenities
      .split(",")
      .map((item) => item.trim())
      .filter(Boolean),
  };
}
