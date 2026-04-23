import { toRoomStatus, toRoomStatusEnum, toRoomType, toRoomTypeEnum } from "@/lib/hotel-mappers";
import { apiClient, toQueryString } from "@/services/api-client";
import type { Room, RoomStatus, RoomType } from "@/types/hotel";

type RoomResponseDto = {
  id: string;
  roomNumber: string;
  floorNumber: number;
  type: string;
  status: string;
  pricePerNight: number;
  bedCount: number;
  amenities: string[];
  createdAt: string;
};

type RoomsPagedResultDto = {
  items: RoomResponseDto[];
  totalCount: number;
  page: number;
  pageSize: number;
};

type SaveRoomInput = {
  roomNumber: string;
  type: RoomType;
  pricePerNight: number;
  floor: number;
  beds: number;
  status: RoomStatus;
  amenities: string[];
};

export const roomsQueryKey = ["rooms"] as const;

export const supportedRoomTypes: RoomType[] = [
  "Standard",
  "Deluxe",
  "Family",
  "Business Suite",
];

export const supportedRoomStatuses: RoomStatus[] = [
  "available",
  "occupied",
  "cleaning",
  "maintenance",
];

export const knownAmenities = [
  "WiFi",
  "TV",
  "Air Conditioning",
  "Mini Fridge",
  "Mini Bar",
  "Bathtub",
  "City View",
  "Garden View",
  "Jacuzzi",
  "Living Room",
  "Panoramic View",
  "Work Desk",
  "Espresso Machine",
  "Kitchenette",
  "Sofa Bed",
];

function mapRoom(dto: RoomResponseDto): Room {
  return {
    id: dto.id,
    roomNumber: dto.roomNumber,
    floor: dto.floorNumber,
    type: toRoomType(dto.type),
    status: toRoomStatus(dto.status),
    pricePerNight: dto.pricePerNight,
    beds: dto.bedCount,
    amenities: dto.amenities,
    createdAt: dto.createdAt,
  };
}

function toSaveRoomRequest(input: SaveRoomInput) {
  return {
    roomNumber: input.roomNumber,
    floorNumber: input.floor,
    type: toRoomTypeEnum(input.type),
    pricePerNight: input.pricePerNight,
    bedCount: input.beds,
    amenities: input.amenities,
    status: toRoomStatusEnum(input.status),
  };
}

export async function getRooms(params?: {
  type?: RoomType;
  status?: RoomStatus;
  floor?: number;
  search?: string;
  page?: number;
  pageSize?: number;
}) {
  const response = await apiClient<RoomsPagedResultDto>(
    `/rooms${toQueryString({
      type: params?.type?.replace(" ", ""),
      status: params?.status ? capitalize(params.status) : undefined,
      floor: params?.floor,
      search: params?.search,
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 100,
    })}`,
  );

  return {
    items: response.items.map(mapRoom),
    totalCount: response.totalCount,
    page: response.page,
    pageSize: response.pageSize,
  };
}

export async function getRoom(id: string) {
  return mapRoom(await apiClient<RoomResponseDto>(`/rooms/${id}`));
}

export async function createRoom(input: SaveRoomInput) {
  return mapRoom(
    await apiClient<RoomResponseDto>("/rooms", {
      method: "POST",
      body: toSaveRoomRequest(input),
    }),
  );
}

export async function updateRoom(id: string, input: SaveRoomInput) {
  return mapRoom(
    await apiClient<RoomResponseDto>(`/rooms/${id}`, {
      method: "PUT",
      body: toSaveRoomRequest(input),
    }),
  );
}

export async function deleteRoom(id: string) {
  await apiClient<void>(`/rooms/${id}`, {
    method: "DELETE",
    responseType: "void",
  });
}

export function getAllAmenities(rooms: Room[] = []) {
  return Array.from(
    new Set([...knownAmenities, ...rooms.flatMap((room) => room.amenities)]),
  ).sort();
}

function capitalize(value: string) {
  return value.slice(0, 1).toUpperCase() + value.slice(1);
}
