import { useAuthStore } from "@/store/auth-store";

const API_BASE_URL = (
  process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5259/api"
).replace(/\/$/, "");

export type ApiEnvelope<T> = {
  success: boolean;
  message: string;
  data: T;
  errors?: Record<string, string[]>;
  traceId?: string;
};

type ApiRequestOptions = Omit<RequestInit, "body"> & {
  auth?: boolean;
  body?: BodyInit | Record<string, unknown> | null;
  responseType?: "json" | "blob" | "void";
};

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly errors?: Record<string, string[]>,
    public readonly traceId?: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

function isPlainObject(value: unknown): value is Record<string, unknown> {
  return (
    typeof value === "object" &&
    value !== null &&
    !(value instanceof FormData) &&
    !(value instanceof Blob) &&
    !(value instanceof ArrayBuffer) &&
    !(value instanceof URLSearchParams)
  );
}

function getRedirectTarget() {
  if (typeof window === "undefined") {
    return "/dashboard";
  }

  const path = `${window.location.pathname}${window.location.search}`;
  return path.startsWith("/login") || path.startsWith("/register")
    ? "/dashboard"
    : path;
}

function handleUnauthorized() {
  useAuthStore.getState().logout();

  if (typeof window === "undefined") {
    return;
  }

  const redirect = encodeURIComponent(getRedirectTarget());
  const target = `/login?redirect=${redirect}`;

  if (window.navigator.userAgent.toLowerCase().includes("jsdom")) {
    window.history.replaceState(null, "", target);
    return;
  }

  window.location.href = target;
}

async function parseError(response: Response) {
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    const payload = (await response.json()) as
      | ApiEnvelope<unknown>
      | { message?: string; errors?: Record<string, string[]>; traceId?: string };

    return new ApiError(
      payload.message ?? `Request failed with status ${response.status}.`,
      response.status,
      payload.errors,
      payload.traceId,
    );
  }

  const text = await response.text();
  return new ApiError(
    text || `Request failed with status ${response.status}.`,
    response.status,
  );
}

export async function apiClient<T>(
  endpoint: string,
  options: ApiRequestOptions = {},
): Promise<T> {
  const { auth = true, body, headers, responseType = "json", ...init } = options;
  const token = auth ? useAuthStore.getState().token : null;
  const requestHeaders = new Headers(headers);

  let requestBody: BodyInit | null | undefined;
  if (isPlainObject(body)) {
    requestHeaders.set("Content-Type", "application/json");
    requestBody = JSON.stringify(body);
  } else {
    requestBody = body as BodyInit | null | undefined;

    if (body && !(body instanceof FormData) && !requestHeaders.has("Content-Type")) {
      requestHeaders.set("Content-Type", "application/json");
    }
  }

  if (token) {
    requestHeaders.set("Authorization", `Bearer ${token}`);
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...init,
    body: requestBody,
    headers: requestHeaders,
    cache: "no-store",
  });

  if (!response.ok) {
    const error = await parseError(response);

    if (response.status === 401 && auth) {
      handleUnauthorized();
    }

    throw error;
  }

  if (responseType === "void" || response.status === 204) {
    return undefined as T;
  }

  if (responseType === "blob") {
    return (await response.blob()) as T;
  }

  const payload = (await response.json()) as ApiEnvelope<T> | T;

  if (
    typeof payload === "object" &&
    payload !== null &&
    "success" in payload &&
    "data" in payload
  ) {
    const envelope = payload as ApiEnvelope<T>;

    if (!envelope.success) {
      throw new ApiError(
        envelope.message || "The request failed.",
        response.status,
        envelope.errors,
        envelope.traceId,
      );
    }

    return envelope.data;
  }

  return payload as T;
}

export function toQueryString(
  params: Record<string, string | number | boolean | undefined | null>,
) {
  const searchParams = new URLSearchParams();

  Object.entries(params).forEach(([key, value]) => {
    if (value === undefined || value === null || value === "") {
      return;
    }

    searchParams.set(key, String(value));
  });

  const query = searchParams.toString();
  return query ? `?${query}` : "";
}

export function getApiErrorMessage(
  error: unknown,
  fallback = "Something went wrong.",
) {
  if (error instanceof ApiError) {
    if (error.errors) {
      const firstError = Object.values(error.errors)[0]?.[0];
      return firstError ?? error.message;
    }

    return error.message;
  }

  if (error instanceof Error) {
    return error.message;
  }

  return fallback;
}

export const apiConfig = {
  baseUrl: API_BASE_URL,
};
