import { Component, signal, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GalleryService } from '../../core/gallery.service';
import { GalleryImageDetailResponse } from '../../core/models';

@Component({
    selector: 'app-share',
    standalone: true,
    imports: [],
    templateUrl: './share.html'
})
export class ShareComponent implements OnInit {
    image = signal<GalleryImageDetailResponse | null>(null);
    loading = signal(true);
    error = signal('');

    constructor(
        private route: ActivatedRoute,
        private galleryService: GalleryService
    ) { }

    ngOnInit() {
        const token = this.route.snapshot.paramMap.get('token');
        if (!token) {
            this.error.set('Geçersiz paylaşım linki.');
            this.loading.set(false);
            return;
        }

        this.galleryService.getPublicImage(token).subscribe({
            next: (img) => {
                this.image.set(img);
                this.loading.set(false);
            },
            error: () => {
                this.error.set('Paylaşılan görsel bulunamadı veya paylaşım kapatılmış.');
                this.loading.set(false);
            }
        });
    }
}
