import { toAuthUserAvatar, toUserRole } from "@/lib/hotel-mappers";
import { apiClient } from "@/services/api-client";
import type { AuthSession } from "@/types/hotel";

type AuthResponseDto = {
  token: string;
  expiresAt: string;
  userId: string;
  fullName: string;
  email: string;
  role: string;
};

type LoginRequest = {
  email: string;
  password: string;
};

type RegisterRequest = {
  fullName: string;
  email: string;
  password: string;
};

function mapAuthSession(dto: AuthResponseDto): AuthSession {
  return {
    token: dto.token,
    expiresAt: dto.expiresAt,
    user: {
      id: dto.userId,
      name: dto.fullName,
      email: dto.email,
      role: toUserRole(dto.role),
      avatar: toAuthUserAvatar(dto.fullName),
    },
  };
}

export async function login(credentials: LoginRequest) {
  const response = await apiClient<AuthResponseDto>("/auth/login", {
    method: "POST",
    auth: false,
    body: credentials,
  });

  return mapAuthSession(response);
}

export async function registerAccount(input: RegisterRequest) {
  const response = await apiClient<AuthResponseDto>("/auth/register", {
    method: "POST",
    auth: false,
    body: {
      fullName: input.fullName,
      email: input.email,
      password: input.password,
      roleName: "Guest",
    },
  });

  return mapAuthSession(response);
}
