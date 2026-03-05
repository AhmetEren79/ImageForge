import { Component, signal, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GenerationService } from '../../core/generation.service';
import { GenerationStatusResponse } from '../../core/models';

@Component({
    selector: 'app-studio',
    standalone: true,
    imports: [FormsModule],
    templateUrl: './studio.html'
})
export class StudioComponent implements OnDestroy {
    // Form
    promptText = '';
    negativePrompt = '';
    selectedModel = 0;
    imageCount = 2;
    width = 1024;
    height = 1024;
    steps = 30;
    cfgScale = 7.0;
    seed: number | null = null;

    // State
    loading = signal(false);
    error = signal('');
    activeGeneration = signal<GenerationStatusResponse | null>(null);
    history = signal<GenerationStatusResponse[]>([]);

    private pollTimer: any = null;

    models = [
        { value: 0, name: 'Disco Elysium', desc: 'Renkli, ekspresyonist fırça darbeleri', emoji: '🎨' },
        { value: 1, name: 'Slay the Princess', desc: 'Siyah-beyaz, karanlık çizim', emoji: '🖤' }
    ];

    constructor(private genService: GenerationService) {
        this.loadHistory();
    }

    ngOnDestroy() {
        this.stopPolling();
    }

    submit() {
        if (!this.promptText.trim()) {
            this.error.set('Prompt metni zorunludur.');
            return;
        }

        this.loading.set(true);
        this.error.set('');

        this.genService.create({
            promptText: this.promptText,
            negativePrompt: this.negativePrompt || undefined,
            selectedModel: this.selectedModel,
            imageCount: this.imageCount,
            width: this.width,
            height: this.height,
            steps: this.steps,
            cfgScale: this.cfgScale,
            seed: this.seed ?? undefined
        }).subscribe({
            next: (res) => {
                this.loading.set(false);
                this.activeGeneration.set(res);
                this.startPolling(res.promptId);
            },
            error: (err) => {
                this.loading.set(false);
                this.error.set(err.error?.error || 'Üretim başlatılamadı.');
            }
        });
    }

    private startPolling(promptId: string) {
        this.stopPolling();
        this.pollTimer = setInterval(() => {
            this.genService.getStatus(promptId).subscribe({
                next: (res) => {
                    this.activeGeneration.set(res);
                    if (res.status === 'Completed' || res.status === 'Failed') {
                        this.stopPolling();
                        this.loadHistory();
                    }
                },
                error: () => this.stopPolling()
            });
        }, 3000);
    }

    private stopPolling() {
        if (this.pollTimer) {
            clearInterval(this.pollTimer);
            this.pollTimer = null;
        }
    }

    private loadHistory() {
        this.genService.getHistory(1, 10).subscribe({
            next: (history) => this.history.set(history)
        });
    }

    clearGeneration() {
        this.activeGeneration.set(null);
        this.stopPolling();
    }

    getStatusColor(status: string): string {
        switch (status) {
            case 'Completed': return 'text-success';
            case 'Failed': return 'text-danger';
            case 'Processing': return 'text-warning';
            default: return 'text-text-muted';
        }
    }

    getStatusIcon(status: string): string {
        switch (status) {
            case 'Completed': return '✅';
            case 'Failed': return '❌';
            case 'Processing': return '⏳';
            default: return '🕐';
        }
    }
}
