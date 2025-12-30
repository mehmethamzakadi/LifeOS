export interface ApiResult<T = undefined> {
  success: boolean;
  message: string;
  data: T;
  internalMessage?: string;
  errors?: string[];
}

export interface ApiError extends ApiResult<undefined> {
  statusCode?: number;
}

export interface PaginatedListResponse<T> {
  items: T[];
  size: number;
  index: number;
  count: number;
  pages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface PaginatedRequest {
  pageIndex: number;
  pageSize: number;
}

export interface SortDescriptor {
  field: string;
  dir: 'asc' | 'desc';
}

export interface FilterDescriptor {
  field: string;
  operator: string;
  value?: string | number | boolean;
  logic?: string;
  filters?: FilterDescriptor[];
}

export interface DataGridRequest {
  paginatedRequest: PaginatedRequest;
  dynamicQuery?: {
    sort?: SortDescriptor[];
    filter?: FilterDescriptor;
  };
}

export function normalizeApiResult<T>(data: any): ApiResult<T> {
  if (typeof data === 'string') {
    return {
      success: true,
      message: data,
      data: undefined as T,
      errors: []
    };
  }

  if (data && typeof data === 'object' && 'success' in data) {
    return data as ApiResult<T>;
  }

  const collectedErrors: string[] = [];

  const appendErrors = (errorSource: unknown) => {
    if (!errorSource) {
      return;
    }

    if (Array.isArray(errorSource)) {
      collectedErrors.push(...(errorSource.filter((item) => typeof item === 'string') as string[]));
      return;
    }

    if (typeof errorSource === 'object') {
      Object.values(errorSource).forEach(appendErrors);
    }
  };

  appendErrors(data?.Errors);
  appendErrors(data?.errors);

  const message = data?.Message ?? data?.message ?? collectedErrors[0] ?? '';

  return {
    success: Boolean(data?.Success ?? data?.success),
    message,
    data: (data?.Data ?? data?.data) as T,
    internalMessage: data?.InternalMessage ?? data?.internalMessage,
    errors: collectedErrors.length > 0 ? collectedErrors : undefined
  };
}

export function normalizeApiError(error: unknown, fallbackMessage = 'Beklenmeyen bir hata oluştu'): ApiError {
  const apiError: ApiError = {
    success: false,
    message: fallbackMessage,
    data: undefined,
    errors: [],
    internalMessage: undefined,
    statusCode: undefined
  };

  if (typeof error === 'string') {
    apiError.message = error;
    return apiError;
  }

  if (error && typeof error === 'object') {
    const maybeAxiosError = error as {
      message?: string;
      response?: { status?: number; data?: any };
      request?: unknown;
    };

    if (maybeAxiosError.response) {
      apiError.statusCode = maybeAxiosError.response.status;
      const data = maybeAxiosError.response.data;

      if (typeof data === 'string') {
        apiError.message = data;
        return apiError;
      }

      if (data && typeof data === 'object') {
        const normalized = normalizeApiResult(data);
        apiError.message = normalized.message || fallbackMessage;
        apiError.errors = normalized.errors ?? [];
        apiError.internalMessage = normalized.internalMessage;
        return apiError;
      }
    }

    if (Array.isArray((error as { errors?: unknown }).errors)) {
      const errors = (error as { errors: string[] }).errors;
      apiError.errors = errors;
      apiError.message = errors.join('\n') || fallbackMessage;
      return apiError;
    }

    if (maybeAxiosError.message) {
      apiError.message = maybeAxiosError.message;
      return apiError;
    }
  }

  if (error instanceof Error) {
    apiError.message = error.message || fallbackMessage;
  }

  return apiError;
}

export function normalizePaginatedResponse<T>(data: any): PaginatedListResponse<T> {
  // Eğer ApiResult wrapper'ı içindeyse, önce onu normalize et
  let actualData = data;
  if (data && typeof data === 'object' && ('success' in data || 'Success' in data)) {
    // ApiResult wrapper'ı var, data'yı çıkar
    actualData = data.data ?? data.Data;
  }

  // Backend artık camelCase döndürüyor (JSON serialization ayarı sayesinde)
  // Eğer direkt PaginatedListResponse formatındaysa
  if (actualData && typeof actualData === 'object' && 'items' in actualData) {
    return actualData as PaginatedListResponse<T>;
  }

  // Fallback: Eğer hala büyük harf formatı gelirse (geriye dönük uyumluluk)
  return {
    items: (actualData?.Items ?? actualData?.items ?? []) as T[],
    size: Number(actualData?.Size ?? actualData?.size ?? actualData?.PageSize ?? 0),
    index: Number(actualData?.Index ?? actualData?.index ?? actualData?.PageIndex ?? 0),
    count: Number(actualData?.Count ?? actualData?.count ?? 0),
    pages: Number(actualData?.Pages ?? actualData?.pages ?? 0),
    hasPrevious: Boolean(actualData?.HasPrevious ?? actualData?.hasPrevious ?? false),
    hasNext: Boolean(actualData?.HasNext ?? actualData?.hasNext ?? false)
  };
}
