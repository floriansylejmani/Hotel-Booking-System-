import { toUserRole } from "@/lib/hotel-mappers";
import { apiClient, toQueryString } from "@/services/api-client";
import type { UserRole } from "@/types/hotel";

// ─── API response shapes ──────────────────────────────────────────────────────

export type RoleDto = {
  id: string;
  name: string;
};

export type StaffMember = {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
};

type StaffPagedResultDto = {
  items: StaffMemberDto[];
  totalCount: number;
  page: number;
  pageSize: number;
};

type StaffMemberDto = {
  id: string;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
};

export type CreateStaffInput = {
  fullName: string;
  email: string;
  password: string;
  roleName: string;
};

export type UpdateStaffInput = {
  fullName?: string;
  email?: string;
  roleName?: string;
};

export type StaffListResult = {
  items: StaffMember[];
  totalCount: number;
  page: number;
  pageSize: number;
};

// ─── Mapper ───────────────────────────────────────────────────────────────────

function mapStaff(dto: StaffMemberDto): StaffMember {
  return {
    id: dto.id,
    fullName: dto.fullName,
    email: dto.email,
    role: toUserRole(dto.role),
    isActive: dto.isActive,
    createdAt: dto.createdAt,
  };
}

// ─── API functions ────────────────────────────────────────────────────────────

export async function getRoles(): Promise<RoleDto[]> {
  return apiClient<RoleDto[]>("/roles");
}

export async function getStaff(params?: {
  roleName?: string;
  isActive?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}): Promise<StaffListResult> {
  const result = await apiClient<StaffPagedResultDto>(
    `/staff${toQueryString({
      roleName: params?.roleName,
      isActive: params?.isActive,
      search: params?.search,
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 50,
    })}`,
  );
  return {
    items: result.items.map(mapStaff),
    totalCount: result.totalCount,
    page: result.page,
    pageSize: result.pageSize,
  };
}

export async function getStaffMember(id: string): Promise<StaffMember> {
  const dto = await apiClient<StaffMemberDto>(`/staff/${id}`);
  return mapStaff(dto);
}

export async function createStaff(input: CreateStaffInput): Promise<StaffMember> {
  const dto = await apiClient<StaffMemberDto>("/staff", {
    method: "POST",
    body: JSON.stringify(input),
  });
  return mapStaff(dto);
}

export async function updateStaff(id: string, input: UpdateStaffInput): Promise<StaffMember> {
  const dto = await apiClient<StaffMemberDto>(`/staff/${id}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
  return mapStaff(dto);
}

export async function deactivateStaff(id: string): Promise<StaffMember> {
  const dto = await apiClient<StaffMemberDto>(`/staff/${id}/deactivate`, { method: "PUT" });
  return mapStaff(dto);
}

export async function activateStaff(id: string): Promise<StaffMember> {
  const dto = await apiClient<StaffMemberDto>(`/staff/${id}/activate`, { method: "PUT" });
  return mapStaff(dto);
}
