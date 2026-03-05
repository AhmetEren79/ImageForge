// ─── Auth ───
export interface LoginRequest {
    emailOrUsername: string;
    password: string;
}

export interface RegisterRequest {
    email: string;
    username: string;
    password: string;
    displayName?: string;
}

export interface AuthResponse {
    token: string;
    userId: string;
    email: string;
    username: string;
    displayName?: string;
}

// ─── Generation ───
export interface CreateGenerationRequest {
    promptText: string;
    negativePrompt?: string;
    selectedModel: number; // 0 = DiscoElysium, 1 = SlayThePrincess
    imageCount: number;
    width: number;
    height: number;
    steps: number;
    cfgScale: number;
    seed?: number;
}

export interface GenerationStatusResponse {
    promptId: string;
    status: string;
    promptText: string;
    selectedModel: string;
    imageCount: number;
    errorMessage?: string;
    createdAt: string;
    images: GeneratedImageDto[];
}

export interface GeneratedImageDto {
    id: string;
    storageUrl: string;
    fileName: string;
    fileSizeBytes: number;
    width: number;
    height: number;
    seed?: number;
    isFavorite: boolean;
    createdAt: string;
}

// ─── Gallery ───
export interface PagedResult<T> {
    items: T[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    hasPrevious: boolean;
    hasNext: boolean;
}

export interface GalleryImageResponse {
    id: string;
    storageUrl: string;
    fileName: string;
    width: number;
    height: number;
    isFavorite: boolean;
    isPublic: boolean;
    createdAt: string;
    promptText: string;
    selectedModel: string;
}

export interface GalleryImageDetailResponse {
    id: string;
    storageUrl: string;
    fileName: string;
    fileSizeBytes: number;
    width: number;
    height: number;
    seed?: number;
    isFavorite: boolean;
    isPublic: boolean;
    publicShareToken?: string;
    createdAt: string;
    updatedAt?: string;
    promptId: string;
    promptText: string;
    negativePrompt?: string;
    selectedModel: string;
    steps: number;
    cfgScale: number;
}

export interface ShareLinkResponse {
    imageId: string;
    isPublic: boolean;
    shareToken?: string;
}
