import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from './environment';
import { CreateGenerationRequest, GenerationStatusResponse } from './models';

@Injectable({ providedIn: 'root' })
export class GenerationService {
    constructor(private http: HttpClient) { }

    create(request: CreateGenerationRequest) {
        return this.http.post<GenerationStatusResponse>(`${environment.apiUrl}/generation`, request);
    }

    getStatus(promptId: string) {
        return this.http.get<GenerationStatusResponse>(`${environment.apiUrl}/generation/${promptId}/status`);
    }

    getHistory(page = 1, pageSize = 20) {
        return this.http.get<GenerationStatusResponse[]>(
            `${environment.apiUrl}/generation/history?page=${page}&pageSize=${pageSize}`
        );
    }
}
