// Thin fetch wrapper. All feature API modules (matches.ts, players.ts) go
// through this so error/404/JSON handling lives in exactly one place.

export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5076";

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export class NotFoundError extends ApiError {
  constructor(path: string) {
    super(`Not found: ${path}`, 404);
    this.name = "NotFoundError";
  }
}

interface RequestOptions {
  signal?: AbortSignal;
}

export async function apiGet<T>(
  path: string,
  params?: Record<string, string | number | boolean | undefined>,
  options?: RequestOptions,
): Promise<T> {
  const url = new URL(path, API_BASE_URL);

  if (params) {
    for (const [key, value] of Object.entries(params)) {
      if (value !== undefined && value !== null && value !== "") {
        url.searchParams.set(key, String(value));
      }
    }
  }

  const response = await fetch(url, { signal: options?.signal });

  if (response.status === 404) {
    throw new NotFoundError(path);
  }

  if (!response.ok) {
    const body = await response.text().catch(() => "");
    throw new ApiError(
      `Request to ${path} failed with ${response.status}${body ? `: ${body}` : ""}`,
      response.status,
    );
  }

  return (await response.json()) as T;
}
