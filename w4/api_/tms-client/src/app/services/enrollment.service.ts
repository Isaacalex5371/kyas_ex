import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { Enrollment, EnrollmentStatus, PagedResponsee } from '../models/enrollment.model';
import { environment } from '../../environments/environment';

@Service()
export class EnrollmentService {
    private http = inject(HttpClient);
    private baseUrl = `${environment.apiUrl}/v2/enrollments`

    getEnrollments(page=1, pageSize=20): Observable<PagedResponsee<Enrollment>>{
        return this.http.get <PagedResponsee<Enrollment>>(`${this.baseUrl}?page=${page}&pageSize=${pageSize}`);
    };

    approve(id: number, status:EnrollmentStatus): Observable<Enrollment>{
        return this.http.post<Enrollment>(`${this.baseUrl}/${id}/approve`, null)
    }
}