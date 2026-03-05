import { Component, signal, OnInit } from '@angular/core';
import { GalleryService } from '../../core/gallery.service';
import { AuthService } from '../../core/auth.service';
import { GalleryImageResponse, GalleryImageDetailResponse, PagedResult } from '../../core/models';
import { environment } from '../../core/environment';

@Component({
    selector: 'app-gallery',
    standalone: true,
    imports: [],
    templateUrl: './gallery.html'
})
export class GalleryComponent implements OnInit {
    images = signal<GalleryImageResponse[]>([]);
    totalPages = signal(0);
    currentPage = signal(1);
    pageSize = 12;
    onlyFavorites = signal(false);
    loading = signal(false);
    selectedImage = signal<GalleryImageDetailResponse | null>(null);
    showDetail = signal(false);
    confirmDelete = signal<string | null>(null);
    toast = signal('');

    constructor(
        private galleryService: GalleryService,
        private authService: AuthService
    ) { }

    ngOnInit() {
        this.loadImages();
    }

    loadImages() {
        this.loading.set(true);
        this.galleryService.getImages(this.currentPage(), this.pageSize, this.onlyFavorites() || undefined)
            .subscribe({
                next: (result) => {
                    this.images.set(result.items);
                    this.totalPages.set(result.totalPages);
                    this.loading.set(false);
                },
                error: () => this.loading.set(false)
            });
    }

    toggleFavoritesFilter() {
        this.onlyFavorites.set(!this.onlyFavorites());
        this.currentPage.set(1);
        this.loadImages();
    }

    goToPage(page: number) {
        if (page < 1 || page > this.totalPages()) return;
        this.currentPage.set(page);
        this.loadImages();
    }

    openDetail(imageId: string) {
        this.galleryService.getDetail(imageId).subscribe({
            next: (detail) => {
                this.selectedImage.set(detail);
                this.showDetail.set(true);
            }
        });
    }

    closeDetail() {
        this.showDetail.set(false);
        this.selectedImage.set(null);
    }

    toggleFavorite(imageId: string, event?: Event) {
        event?.stopPropagation();
        this.galleryService.toggleFavorite(imageId).subscribe({
            next: (updated) => {
                this.images.update(imgs => imgs.map(i =>
                    i.id === imageId ? { ...i, isFavorite: updated.isFavorite } : i
                ));
                if (this.selectedImage()?.id === imageId) {
                    this.selectedImage.update(img => img ? { ...img, isFavorite: updated.isFavorite } : null);
                }
                this.showToast(updated.isFavorite ? '❤️ Favorilere eklendi' : '💔 Favorilerden çıkarıldı');
            }
        });
    }

    toggleShare(imageId: string) {
        this.galleryService.toggleShare(imageId).subscribe({
            next: (res) => {
                this.images.update(imgs => imgs.map(i =>
                    i.id === imageId ? { ...i, isPublic: res.isPublic } : i
                ));
                if (this.selectedImage()?.id === imageId) {
                    this.selectedImage.update(img => img ? {
                        ...img,
                        isPublic: res.isPublic,
                        publicShareToken: res.shareToken ?? undefined
                    } : null);
                }
                if (res.isPublic && res.shareToken) {
                    this.copyShareLink(res.shareToken);
                    this.showToast('🔗 Paylaşım linki kopyalandı!');
                } else {
                    this.showToast('🔒 Paylaşım kapatıldı');
                }
            }
        });
    }

    requestDelete(imageId: string, event?: Event) {
        event?.stopPropagation();
        this.confirmDelete.set(imageId);
    }

    cancelDelete() {
        this.confirmDelete.set(null);
    }

    executeDelete(imageId: string) {
        this.galleryService.deleteImage(imageId).subscribe({
            next: () => {
                this.confirmDelete.set(null);
                if (this.showDetail() && this.selectedImage()?.id === imageId) {
                    this.closeDetail();
                }
                this.loadImages();
                this.showToast('🗑️ Görsel silindi');
            }
        });
    }

    download(imageId: string) {
        const token = this.authService.getToken();
        if (!token) return;

        const url = this.galleryService.getDownloadUrl(imageId);
        fetch(url, { headers: { Authorization: `Bearer ${token}` } })
            .then(res => res.blob())
            .then(blob => {
                const a = document.createElement('a');
                a.href = URL.createObjectURL(blob);
                a.download = this.images().find(i => i.id === imageId)?.fileName || 'image.png';
                a.click();
                URL.revokeObjectURL(a.href);
                this.showToast('⬇️ İndirme başladı');
            });
    }

    private copyShareLink(token: string) {
        const link = `${window.location.origin}/share/${token}`;
        navigator.clipboard.writeText(link).catch(() => { });
    }

    private showToast(message: string) {
        this.toast.set(message);
        setTimeout(() => this.toast.set(''), 3000);
    }

    getPages(): number[] {
        const total = this.totalPages();
        const current = this.currentPage();
        const pages: number[] = [];
        const start = Math.max(1, current - 2);
        const end = Math.min(total, current + 2);
        for (let i = start; i <= end; i++) pages.push(i);
        return pages;
    }
}
