import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from './environment';
import {
    GalleryImageResponse,
    GalleryImageDetailResponse,
    PagedResult,
    ShareLinkResponse,
} from './models';

@Injectable({ providedIn: 'root' })
export class GalleryService {
    constructor(private http: HttpClient) { }

    getImages(page = 1, pageSize = 20, onlyFavorites?: boolean) {
        let url = `${environment.apiUrl}/gallery?page=${page}&pageSize=${pageSize}`;
        if (onlyFavorites) url += '&onlyFavorites=true';
        return this.http.get<PagedResult<GalleryImageResponse>>(url);
    }

    getDetail(imageId: string) {
        return this.http.get<GalleryImageDetailResponse>(`${environment.apiUrl}/gallery/${imageId}`);
    }

    toggleFavorite(imageId: string) {
        return this.http.patch<GalleryImageResponse>(`${environment.apiUrl}/gallery/${imageId}/favorite`, {});
    }

    toggleShare(imageId: string) {
        return this.http.patch<ShareLinkResponse>(`${environment.apiUrl}/gallery/${imageId}/share`, {});
    }

    deleteImage(imageId: string) {
        return this.http.delete(`${environment.apiUrl}/gallery/${imageId}`);
    }

    getDownloadUrl(imageId: string) {
        return `${environment.apiUrl}/gallery/${imageId}/download`;
    }

    getPublicImage(token: string) {
        return this.http.get<GalleryImageDetailResponse>(`${environment.apiUrl}/public/share/${token}`);
    }
}
